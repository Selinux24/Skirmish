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
        private readonly Dictionary<string, Dictionary<string, Mesh>> meshes = new();
        /// <summary>
        /// Materials dictionary
        /// </summary>
        private readonly Dictionary<string, IMeshMaterial> materials = new();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        private readonly Dictionary<string, MeshTexture> textures = new();
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

            foreach (var item in meshes.Values)
            {
                foreach (var mesh in item.Values)
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
                var info = texture.Content;

                textures.Add(texture.Name, new() { Content = info });
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

            foreach (var mat in modelMaterials)
            {
                var matName = mat.Name;
                var matContent = mat.Content;

                var meshMaterial = matContent.CreateMeshMaterial(textures);
                materials.Add(matName, meshMaterial);
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
            if (meshes?.Any() != true)
            {
                return;
            }

            Logger.WriteTrace(nameof(DrawingData), $"{name} Processing Material Meshes Dictionary => {meshes.Keys.AsEnumerable().Join("|")}");

            var beforeCount = Game.BufferManager.PendingRequestCount;
            Logger.WriteTrace(nameof(DrawingData), $"{name} Pending Requests before Initialization => {beforeCount}");

            //Initilizes mesh textures
            await InitializeMeshTextures();

            //Generates a task list from the materials-mesh dictionary
            var taskList = meshes.Values
                .Where(dictionary => dictionary?.Any() == true)
                .Select(dictionary => Task.Run(() => InitializeMeshDictionary(Game, name, Description.DynamicBuffers, instancingBuffer, dictionary)));

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
        /// Initializes the texture dictionary
        /// </summary>
        private async Task InitializeMeshTextures()
        {
            foreach (var textureName in textures.Keys)
            {
                var meshTexture = textures[textureName];

                var info = meshTexture.Content;

                var view = await Game.ResourceManager.RequestResource(info);
                if (view == null)
                {
                    string errorMessage = $"Texture cannot be requested: {info}";

                    Logger.WriteError(nameof(DrawingData), errorMessage);

                    throw new EngineException(errorMessage);
                }

                meshTexture.Resource = view;

                textures[textureName] = meshTexture;
            }
        }
        /// <summary>
        /// Initializes a mesh dictionary
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Owner name</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        /// <param name="meshes">Mesh dictionary</param>
        private static void InitializeMeshDictionary(Game game, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer, Dictionary<string, Mesh> meshes)
        {
            Logger.WriteTrace(nameof(DrawingData), $"{name} Processing Mesh Dictionary => {meshes.Keys.AsEnumerable().Join("|")}");

            foreach (var mesh in meshes.Values)
            {
                mesh.Initialize(game, name, dynamicBuffers, instancingBuffer);
            }
        }

        /// <summary>
        /// Iterates the materials list
        /// </summary>
        public IEnumerable<(string MaterialName, IMeshMaterial Material, string MeshName, Mesh Mesh)> IterateMaterials()
        {
            foreach (string meshName in meshes.Keys)
            {
                var meshDict = meshes[meshName];

                foreach (string materialName in meshDict.Keys)
                {
                    var mesh = meshDict[materialName];
                    if (!mesh.Ready)
                    {
                        Logger.WriteTrace(this, $"{nameof(DrawingData)} - {nameof(IterateMaterials)}: {meshName}.{materialName} discard => Ready {mesh.Ready}");
                        continue;
                    }

                    var material = materials[materialName];

                    yield return new(materialName, material, meshName, mesh);
                }
            }
        }
        /// <summary>
        /// Iterates the mesh list
        /// </summary>
        private IEnumerable<(string MeshName, Mesh Mesh)> IterateMeshes()
        {
            foreach (var meshMaterial in meshes.Values)
            {
                foreach (var mesh in meshMaterial)
                {
                    yield return (mesh.Key, mesh.Value);
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
            var points = new List<Vector3>();

            foreach (var mesh in IterateMeshes())
            {
                var meshPoints = mesh.Mesh.GetPoints(refresh);
                if (!meshPoints.Any())
                {
                    continue;
                }

                var trnPoints = meshPoints.ToArray();
                if (transform.IsIdentity)
                {
                    points.AddRange(trnPoints);
                    continue;
                }

                Vector3.TransformCoordinate(trnPoints, ref transform, trnPoints);
                points.AddRange(trnPoints);
            }

            return points.Distinct().ToArray();
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
            var points = new List<Vector3>();

            foreach (var mesh in IterateMeshes())
            {
                var meshPoints = mesh.Mesh.GetPoints(boneTransforms, refresh);
                if (!meshPoints.Any())
                {
                    continue;
                }

                var trnPoints = meshPoints.ToArray();
                if (transform.IsIdentity)
                {
                    points.AddRange(trnPoints);
                    continue;
                }

                Vector3.TransformCoordinate(trnPoints, ref transform, trnPoints);
                points.AddRange(trnPoints);
            }

            return points.Distinct().ToArray();
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
            var triangles = new List<Triangle>();

            foreach (var mesh in IterateMeshes())
            {
                var meshTriangles = mesh.Mesh.GetTriangles(refresh);
                if (!meshTriangles.Any())
                {
                    continue;
                }

                if (transform.IsIdentity)
                {
                    triangles.AddRange(meshTriangles);
                    continue;
                }

                triangles.AddRange(Triangle.Transform(meshTriangles, transform));
            }

            return triangles.Distinct().ToArray();
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
            var triangles = new List<Triangle>();

            foreach (var mesh in IterateMeshes())
            {
                var meshTriangles = mesh.Mesh.GetTriangles(boneTransforms, refresh);
                if (!meshTriangles.Any())
                {
                    continue;
                }

                if (transform.IsIdentity)
                {
                    triangles.AddRange(meshTriangles);
                    continue;
                }

                triangles.AddRange(Triangle.Transform(meshTriangles, transform));
            }

            return triangles.Distinct().ToArray();
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

            return meshes[name].Values.FirstOrDefault();
        }

        /// <summary>
        /// Gets the material list
        /// </summary>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return materials.Values.ToArray();
        }
        /// <summary>
        /// Gets the material list by name
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        public IEnumerable<IMeshMaterial> GetMaterials(string meshMaterialName)
        {
            var meshMaterial = materials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
            if (meshMaterial != null)
            {
                yield return materials[meshMaterial];
            }
        }
        /// <summary>
        /// Gets the first material by name
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        /// <returns>Returns the material by name</returns>
        public IMeshMaterial GetFirstMaterial(string meshMaterialName)
        {
            var meshMaterial = materials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
            if (meshMaterial != null)
            {
                return materials[meshMaterial];
            }

            return null;
        }
        /// <summary>
        /// Replaces the first material
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        /// <param name="material">Material</param>
        /// <returns>Returns true if the material is replaced</returns>
        public bool ReplaceFirstMaterial(string meshMaterialName, IMeshMaterial material)
        {
            var meshMaterial = materials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
            if (meshMaterial != null)
            {
                materials[meshMaterial] = material;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Replaces the materials by name
        /// </summary>
        /// <param name="meshMaterialName">Mesh material name</param>
        /// <param name="material">Material</param>
        /// <returns>Returns true if the materials were replaced</returns>
        public bool ReplaceMaterials(string meshMaterialName, IMeshMaterial material)
        {
            var meshMaterials = materials.Keys.Where(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
            if (!meshMaterials.Any())
            {
                return false;
            }

            foreach (var meshMaterial in meshMaterials)
            {
                materials[meshMaterial] = material;
            }

            return true;
        }

        /// <summary>
        /// Gets a copy of the lights collection
        /// </summary>
        public IEnumerable<ISceneLight> GetLights()
        {
            return lights.Select(l => l.Clone()).ToArray();
        }
    }

    /// <summary>
    /// Mesh texture
    /// </summary>
    public struct MeshTexture
    {
        /// <summary>
        /// Texture content
        /// </summary>
        public IImageContent Content;
        /// <summary>
        /// Texture resource
        /// </summary>
        public EngineShaderResourceView Resource;
    }
}
