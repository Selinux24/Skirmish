using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Mesh data
    /// </summary>
    public class DrawingData : IDisposable
    {
        /// <summary>
        /// Model initialization
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Data description</param>
        /// <returns>Returns the generated drawing data objects</returns>
        public static async Task<DrawingData> Read(Game game, ContentData modelContent, DrawingDataDescription description)
        {
            var res = new DrawingData(game, description);

            //Animation
            if (description.LoadAnimation)
            {
                res.ReadSkinningData(modelContent);
            }

            //Images
            res.ReadTextures(modelContent);

            //Materials
            res.ReadMaterials(modelContent);

            //Skins & Meshes
            await res.ReadGeometry(modelContent);

            //Lights
            res.ReadLights(modelContent);

            return res;
        }

        /// <summary>
        /// Meshes
        /// </summary>
        private readonly Dictionary<string, MeshByMaterialCollection> meshes = new();
        /// <summary>
        /// Materials dictionary
        /// </summary>
        private readonly MeshMaterialDataCollection materials = new();
        /// <summary>
        /// Texture collection
        /// </summary>
        private readonly MeshImageDataCollection textures = new();
        /// <summary>
        /// Hull mesh triangle list
        /// </summary>
        private readonly List<Triangle> hullMesh = new();
        /// <summary>
        /// Light list
        /// </summary>
        private readonly List<ISceneLight> lights = new();

        /// <summary>
        /// Game instance
        /// </summary>
        protected readonly Game Game = null;
        /// <summary>
        /// Description
        /// </summary>
        protected readonly DrawingDataDescription Description;

        /// <summary>
        /// Hull mesh
        /// </summary>
        public IEnumerable<Triangle> HullMesh
        {
            get
            {
                return hullMesh.ToArray();
            }
        }
        /// <summary>
        /// Datos de animación
        /// </summary>
        public ISkinningData SkinningData { get; set; } = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Description</param>
        public DrawingData(Game game, DrawingDataDescription description)
        {
            Game = game;
            Description = description;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~DrawingData()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            foreach (var meshCollection in meshes.Values)
            {
                foreach ((_, Mesh mesh) in meshCollection)
                {
                    //Remove data from buffer manager
                    Game.BufferManager?.RemoveVertexData(mesh.VertexBuffer);
                    Game.BufferManager?.RemoveIndexData(mesh.IndexBuffer);

                    //Dispose the mesh
                    mesh.Dispose();
                }
            }
            meshes.Clear();

            materials.Clear();

            //Don't dispose textures!
            textures.Clear();

            SkinningData = null;
        }

        /// <summary>
        /// Initialize skinning data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        private void ReadSkinningData(ContentData modelContent)
        {
            if (SkinningData != null)
            {
                return;
            }

            SkinningData = modelContent.CreateSkinningData();
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="modelContent">Model content</param>
        private void ReadTextures(ContentData modelContent)
        {
            var modelTextures = modelContent.GetTextureContent();
            if (!modelTextures.Any())
            {
                return;
            }

            foreach (var texture in modelTextures)
            {
                textures.Add(texture.Name, MeshImageData.FromContent(texture.Content));
            }
        }
        /// <summary>
        /// Initialize materials
        /// </summary>
        /// <param name="modelContent">Model content</param>
        private void ReadMaterials(ContentData modelContent)
        {
            var modelMaterials = modelContent.GetMaterialContent();
            if (!modelMaterials.Any())
            {
                return;
            }

            foreach (var material in modelMaterials)
            {
                materials.Add(material.Name, MeshMaterialData.FromContent(material.Content));
            }
        }
        /// <summary>
        /// Initilize geometry
        /// </summary>
        /// <param name="modelContent">Model content</param>
        private async Task ReadGeometry(ContentData modelContent)
        {
            //Get drawing geometry
            var geometry = await modelContent.CreateGeometry(Description.LoadAnimation, Description.LoadNormalMaps, Description.Constraint);
            foreach (var mesh in geometry)
            {
                meshes.Add(mesh.Key, mesh.Value);
            }

            //Get hull geometry
            var hulls = modelContent.GetHullMeshes();
            hullMesh.AddRange(hulls);
        }
        /// <summary>
        /// Initialize lights
        /// </summary>
        /// <param name="modelContent">Model content</param>
        private void ReadLights(ContentData modelContent)
        {
            lights.AddRange(modelContent.CreateLights());
        }

        /// <summary>
        /// Initialize mesh buffers in the graphics device
        /// </summary>
        /// <param name="name">Owner name</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        public async Task Initialize(string name, BufferDescriptor instancingBuffer = null)
        {
            if (!meshes.Any())
            {
                return;
            }

            Logger.WriteTrace(nameof(DrawingData), $"{name} Processing Material Meshes Dictionary => {meshes.Keys.AsEnumerable().Join("|")}");

            var beforeCount = Game.BufferManager.PendingRequestCount;
            Logger.WriteTrace(nameof(DrawingData), $"{name} Pending Requests before Initialization => {beforeCount}");

            //Initilizes mesh textures
            var resourceManager = Game.ResourceManager;
            foreach (var (Name, Data) in textures)
            {
                await Data?.RequestResource(resourceManager);
            }

            //Initialize mesh materials
            foreach (var (Name, Data) in materials)
            {
                Data?.AssignTextures(textures);
            }

            //Generates a task list from the materials-mesh dictionary
            var bufferManager = Game.BufferManager;
            var taskList = meshes.Values
                .Where(mList => mList?.Any() == true)
                .Select(mList => Task.Run(() => InitializeMeshCollection(bufferManager, name, Description.DynamicBuffers, instancingBuffer, mList)));

            try
            {
                await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(DrawingData), $"{name} Error processing parallel tasks => {ex.Message}", ex);

                throw;
            }

            var afterCount = Game.BufferManager.PendingRequestCount;
            Logger.WriteTrace(nameof(DrawingData), $"{name} Pending Requests after Initialization => {afterCount}");
        }
        /// <summary>
        /// Initializes a mesh dictionary
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="name">Owner name</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        /// <param name="meshes">Mesh dictionary</param>
        private static void InitializeMeshCollection(BufferManager bufferManager, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer, MeshByMaterialCollection meshes)
        {
            Logger.WriteTrace(nameof(DrawingData), $"{name} Processing Mesh Dictionary => {meshes.MaterialNames.Join("|")}");

            foreach ((_, Mesh mesh) in meshes)
            {
                mesh.Initialize(bufferManager, name, dynamicBuffers, instancingBuffer);
            }
        }

        /// <summary>
        /// Iterates the materials list
        /// </summary>
        public IEnumerable<(string MaterialName, IMeshMaterial MeshMaterial, string MeshName, Mesh Mesh)> IterateMaterials()
        {
            foreach (string meshName in meshes.Keys)
            {
                foreach ((string materialName, var mesh) in meshes[meshName])
                {
                    if (!mesh.Ready)
                    {
                        Logger.WriteTrace(this, $"{nameof(DrawingData)} - {nameof(IterateMaterials)}: {meshName}.{materialName} discard => Ready {mesh.Ready}");
                        continue;
                    }

                    var meshMaterial = materials.GetMaterialData(materialName).Material;

                    yield return new(materialName, meshMaterial, meshName, mesh);
                }
            }
        }
        /// <summary>
        /// Iterates the mesh list
        /// </summary>
        private IEnumerable<(string MaterialName, Mesh Mesh)> IterateMeshes()
        {
            foreach (var meshMaterial in meshes.Values)
            {
                foreach (var mesh in meshMaterial)
                {
                    yield return mesh;
                }
            }
        }

        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public IEnumerable<Vector3> GetPoints(bool refresh = false)
        {
            return GetPoints(Matrix.Identity, refresh);
        }
        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public IEnumerable<Vector3> GetPoints(Matrix transform, bool refresh = false)
        {
            var points = IterateMeshes()
                .SelectMany(m => m.Mesh.GetPoints(refresh))
                .Distinct()
                .ToArray();

            if (!transform.IsIdentity)
            {
                Vector3.TransformCoordinate(points, ref transform, points);
            }

            return points;
        }
        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public IEnumerable<Vector3> GetPoints(IEnumerable<Matrix> boneTransforms, bool refresh = false)
        {
            return GetPoints(Matrix.Identity, boneTransforms, refresh);
        }
        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="transform">Global transform</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public IEnumerable<Vector3> GetPoints(Matrix transform, IEnumerable<Matrix> boneTransforms, bool refresh = false)
        {
            var points = IterateMeshes()
                .SelectMany(m => m.Mesh.GetPoints(boneTransforms, refresh))
                .Distinct()
                .ToArray();

            if (!transform.IsIdentity)
            {
                Vector3.TransformCoordinate(points, ref transform, points);
            }

            return points;
        }

        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(bool refresh = false)
        {
            return GetTriangles(Matrix.Identity, refresh);
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(Matrix transform, bool refresh = false)
        {
            var triangles = IterateMeshes()
                .SelectMany(m => m.Mesh.GetTriangles(refresh))
                .Distinct();

            if (!transform.IsIdentity)
            {
                triangles = Triangle.Transform(triangles, transform);
            }

            return triangles.ToArray();
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(IEnumerable<Matrix> boneTransforms, bool refresh = false)
        {
            return GetTriangles(Matrix.Identity, boneTransforms, refresh);
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public IEnumerable<Triangle> GetTriangles(Matrix transform, IEnumerable<Matrix> boneTransforms, bool refresh = false)
        {
            var triangles = IterateMeshes()
                .SelectMany(m => m.Mesh.GetTriangles(boneTransforms, refresh))
                .Distinct();

            if (!transform.IsIdentity)
            {
                triangles = Triangle.Transform(triangles, transform);
            }

            return triangles.ToArray();
        }

        /// <summary>
        /// Gets a list of meshes by mesh name
        /// </summary>
        /// <param name="name">Name</param>
        public IEnumerable<Mesh> GetMeshesByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                yield break;
            }

            if (!meshes.ContainsKey(name))
            {
                yield break;
            }

            foreach (var mesh in meshes[name])
            {
                yield return mesh.Mesh;
            }
        }
        /// <summary>
        /// Gets the first mesh by mesh name
        /// </summary>
        /// <param name="name">Name</param>
        public Mesh GetMeshByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!meshes.ContainsKey(name))
            {
                return null;
            }

            var meshCollection = meshes[name];
            if (meshCollection.Count == 0)
            {
                return null;
            }

            return meshes[name].First().Mesh;
        }

        /// <summary>
        /// Gets the material list
        /// </summary>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return materials.GetMaterials();
        }
        /// <summary>
        /// Gets the material list by name
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        public IEnumerable<IMeshMaterial> GetMaterials(string meshMaterialName)
        {
            return materials.GetMaterials(meshMaterialName);
        }
        /// <summary>
        /// Gets the first material by name
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        /// <returns>Returns the material by name</returns>
        public IMeshMaterial GetFirstMaterial(string meshMaterialName)
        {
            return materials.GetFirstMaterial(meshMaterialName);
        }
        /// <summary>
        /// Replaces the first material
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        /// <param name="material">Material</param>
        /// <returns>Returns true if the material is replaced</returns>
        public bool ReplaceFirstMaterial(string meshMaterialName, IMeshMaterial material)
        {
            return materials.ReplaceFirstMaterial(meshMaterialName, material);
        }
        /// <summary>
        /// Replaces the materials by name
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        /// <param name="material">Material</param>
        /// <returns>Returns true if the materials were replaced</returns>
        public bool ReplaceMaterials(string meshMaterialName, IMeshMaterial material)
        {
            return materials.ReplaceMaterials(meshMaterialName, material);
        }

        /// <summary>
        /// Gets a copy of the lights collection
        /// </summary>
        public IEnumerable<ISceneLight> GetLights()
        {
            return lights.Select(l => l.Clone()).ToArray();
        }
    }
}
