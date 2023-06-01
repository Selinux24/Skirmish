using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.Animation;
    using Engine.Content;

    /// <summary>
    /// Mesh data
    /// </summary>
    public class DrawingData : IDisposable
    {
        /// <summary>
        /// Hull mesh triangle list
        /// </summary>
        private readonly List<Triangle> hullMesh = new List<Triangle>();
        /// <summary>
        /// Light list
        /// </summary>
        private readonly List<ISceneLight> lights = new List<ISceneLight>();

        /// <summary>
        /// Game instance
        /// </summary>
        protected readonly Game Game = null;
        /// <summary>
        /// Description
        /// </summary>
        protected readonly DrawingDataDescription Description;

        /// <summary>
        /// Materials dictionary
        /// </summary>
        public Dictionary<string, IMeshMaterial> Materials { get; private set; } = new Dictionary<string, IMeshMaterial>();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        public Dictionary<string, EngineShaderResourceView> Textures { get; private set; } = new Dictionary<string, EngineShaderResourceView>();
        /// <summary>
        /// Meshes
        /// </summary>
        public Dictionary<string, Dictionary<string, Mesh>> Meshes { get; private set; } = new Dictionary<string, Dictionary<string, Mesh>>();
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
        /// Model initialization
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Owner name</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Data description</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        /// <returns>Returns the generated drawing data objects</returns>
        public static async Task<DrawingData> Build(Game game, string name, ContentData modelContent, DrawingDataDescription description, BufferDescriptor instancingBuffer = null)
        {
            DrawingData res = new DrawingData(game, description);

            //Animation
            if (description.LoadAnimation)
            {
                await InitializeSkinningData(res, modelContent);
            }

            //Images
            await InitializeTextures(res, game, modelContent);

            //Materials
            await InitializeMaterials(res, modelContent);

            //Skins & Meshes
            await InitializeGeometry(res, modelContent, description);

            //Update meshes into device
            await InitializeMeshes(res, game, name, description.DynamicBuffers, instancingBuffer);

            //Lights
            await InitializeLights(res, modelContent);

            return res;
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        private static async Task InitializeTextures(DrawingData drw, Game game, ContentData modelContent)
        {
            if (modelContent.Images?.Any() != true)
            {
                return;
            }

            foreach (var images in modelContent.Images)
            {
                var info = images.Value;

                var view = await game.ResourceManager.RequestResource(info);
                if (view == null)
                {
                    string errorMessage = $"Texture cannot be requested: {info}";

                    Logger.WriteError(nameof(DrawingData), errorMessage);

                    throw new EngineException(errorMessage);
                }

                drw.Textures.Add(images.Key, view);
            }
        }
        /// <summary>
        /// Initialize materials
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static async Task InitializeMaterials(DrawingData drw, ContentData modelContent)
        {
            if (modelContent.Materials?.Any() != true)
            {
                return;
            }

            await Task.Run(() =>
            {
                foreach (var mat in modelContent.Materials)
                {
                    var matName = mat.Key;
                    var matContent = mat.Value;

                    var meshMaterial = matContent.CreateMeshMaterial(drw.Textures);
                    drw.Materials.Add(matName, meshMaterial);
                }
            });
        }
        /// <summary>
        /// Initilize geometry
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Description</param>
        private static async Task InitializeGeometry(DrawingData drw, ContentData modelContent, DrawingDataDescription description)
        {
            if (modelContent.Geometry?.Any() != true)
            {
                return;
            }

            foreach (var meshName in modelContent.Geometry.Keys)
            {
                //Get the mesh geometry
                var submeshes = modelContent.Geometry[meshName];

                //Get the mesh skinning info
                var skinningInfo = ReadSkinningData(description, modelContent, meshName);

                await InitializeGeometryMesh(drw, description, skinningInfo, meshName, submeshes);
            }
        }
        /// <summary>
        /// Initialize geometry mesh
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="description">Description</param>
        /// <param name="skinningInfo">Skinning information</param>
        /// <param name="meshName">Mesh name</param>
        /// <param name="submeshes">Submesh dictionary</param>
        private static async Task InitializeGeometryMesh(DrawingData drw, DrawingDataDescription description, SkinningInfo? skinningInfo, string meshName, Dictionary<string, SubMeshContent> submeshes)
        {
            var isSkinned = skinningInfo.HasValue;

            //Extract hull geometry
            var hullTriangles = submeshes
                .Where(g => g.Value.IsHull)
                .SelectMany(material => material.Value.GetTriangles())
                .ToArray();
            drw.hullMesh.AddRange(hullTriangles);

            //Extract meshes
            var subMeshList = submeshes
                .Where(g => !g.Value.IsHull)
                .ToArray();

            foreach (var subMesh in subMeshList)
            {
                var geometry = subMesh.Value;

                //Get vertex type
                var vertexType = GetVertexType(geometry.VertexType, isSkinned, description.LoadNormalMaps, drw.Materials, subMesh.Key);

                var meshInfo = await CreateMesh(meshName, geometry, vertexType, description.Constraint, skinningInfo);
                if (!meshInfo.Any())
                {
                    continue;
                }

                var nMesh = meshInfo.First().Mesh;
                var materialName = meshInfo.First().MaterialName;

                if (!drw.Meshes.ContainsKey(meshName))
                {
                    var dict = new Dictionary<string, Mesh>
                    {
                        { materialName, nMesh }
                    };

                    drw.Meshes.Add(meshName, dict);
                }
                else
                {
                    drw.Meshes[meshName].Add(materialName, nMesh);
                }
            }
        }
        /// <summary>
        /// Creates a mesh
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="geometry">Submesh content</param>
        /// <param name="vertexType">Vertext type</param>
        /// <param name="constraint">Geometry constraint</param>
        /// <param name="skinningInfo">Skinning information</param>
        private static async Task<IEnumerable<MeshInfo>> CreateMesh(string meshName, SubMeshContent geometry, VertexTypes vertexType, BoundingBox? constraint, SkinningInfo? skinningInfo)
        {
            //Process the vertex data
            var vertexData = await geometry.ProcessVertexData(vertexType, constraint);
            var vertices = vertexData.vertices;
            var indices = vertexData.indices;

            IEnumerable<IVertexData> vertexList;
            if (skinningInfo.HasValue)
            {
                if (!skinningInfo.Value.BindShapeMatrix.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, skinningInfo.Value.BindShapeMatrix);
                }

                //Convert the vertex data to final mesh data
                vertexList = await VertexData.Convert(
                    vertexType,
                    vertices,
                    skinningInfo.Value.Weights,
                    skinningInfo.Value.BoneNames);
            }
            else
            {
                vertexList = await VertexData.Convert(
                    vertexType,
                    vertices,
                    Enumerable.Empty<Weight>(),
                    Enumerable.Empty<string>());
            }

            if (!vertexList.Any())
            {
                return Enumerable.Empty<MeshInfo>();
            }

            //Create the mesh
            var nMesh = new Mesh(
                meshName,
                geometry.Topology,
                geometry.Transform,
                vertexList,
                indices);

            //Material name
            string materialName = string.IsNullOrEmpty(geometry.Material) ? ContentData.NoMaterial : geometry.Material;

            return new[] { new MeshInfo(nMesh, materialName) };
        }
        /// <summary>
        /// Get vertex type from geometry
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="isSkinned">Sets wether the current geometry has skinning data or not</param>
        /// <param name="loadNormalMaps">Load normal maps flag</param>
        /// <param name="materials">Material dictionary</param>
        /// <param name="material">Material name</param>
        /// <returns>Returns the vertex type</returns>
        private static VertexTypes GetVertexType(VertexTypes vertexType, bool isSkinned, bool loadNormalMaps, Dictionary<string, IMeshMaterial> materials, string material)
        {
            var res = vertexType;
            if (isSkinned)
            {
                //Get skinned equivalent
                res = VertexData.GetSkinnedEquivalent(res);
            }

            if (!loadNormalMaps)
            {
                return res;
            }

            if (VertexData.IsTextured(res) && !VertexData.IsTangent(res))
            {
                var meshMaterial = materials[material];
                if (meshMaterial?.NormalMap != null)
                {
                    //Get tangent equivalent
                    res = VertexData.GetTangentEquivalent(res);
                }
            }

            return res;
        }
        /// <summary>
        /// Reads skinning data
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns the skinnging data</returns>
        private static SkinningInfo? ReadSkinningData(DrawingDataDescription description, ContentData modelContent, string meshName)
        {
            if (!description.LoadAnimation)
            {
                return null;
            }

            if (modelContent.Controllers?.Any() != true)
            {
                return null;
            }

            if (modelContent.SkinningInfo?.Any() != true)
            {
                return null;
            }

            var cInfo = modelContent.GetControllerForMesh(meshName);
            if (cInfo == null)
            {
                return null;
            }

            //Apply shape matrix if controller exists but we are not loading animation info
            var bindShapeMatrix = cInfo.BindShapeMatrix;
            var weights = cInfo.Weights;

            //Find skeleton for controller
            if (!modelContent.SkinningInfo.ContainsKey(cInfo.Armature))
            {
                return null;
            }

            var sInfo = modelContent.SkinningInfo[cInfo.Armature];
            var boneNames = sInfo.Skeleton.GetBoneNames();

            return new SkinningInfo
            {
                BindShapeMatrix = bindShapeMatrix,
                Weights = weights,
                BoneNames = boneNames,
            };
        }
        /// <summary>
        /// Initialize skinning data
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static async Task InitializeSkinningData(DrawingData drw, ContentData modelContent)
        {
            if (drw.SkinningData != null)
            {
                return;
            }

            if (modelContent.SkinningInfo?.Any() != true)
            {
                return;
            }

            await Task.Run(() =>
            {
                //Use the definition to read animation data into a clip dictionary
                var sInfo = modelContent.SkinningInfo.Values.First();
                var jointAnimations = InitializeJoints(modelContent, sInfo.Skeleton.Root, sInfo.Controllers);

                drw.SkinningData = new SkinningData(sInfo.Skeleton);
                drw.SkinningData.Initialize(jointAnimations, modelContent.AnimationDefinition);
            });
        }
        /// <summary>
        /// Initialize skeleton data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="joint">Joint to initialize</param>
        /// <param name="skinController">Skin controller</param>
        private static IEnumerable<JointAnimation> InitializeJoints(ContentData modelContent, Joint joint, IEnumerable<string> skinController)
        {
            List<JointAnimation> animations = new List<JointAnimation>();

            List<JointAnimation> boneAnimations = new List<JointAnimation>();

            //Find keyframes for current bone
            var c = modelContent.Animations.Values.FirstOrDefault(a => a.Any(ac => ac.JointName == joint.Name))?.ToArray();
            if (c?.Any() == true)
            {
                //Set bones
                var ja = c.Select(a => new JointAnimation(a.JointName, a.Keyframes)).ToArray();
                boneAnimations.AddRange(ja);
            }

            if (boneAnimations.Count > 0)
            {
                //Only one bone animation (for now)
                animations.Add(boneAnimations[0]);
            }

            foreach (string controllerName in skinController)
            {
                var controller = modelContent.Controllers[controllerName];

                Matrix ibm = Matrix.Identity;

                if (controller.InverseBindMatrix.ContainsKey(joint.Bone))
                {
                    ibm = controller.InverseBindMatrix[joint.Bone];
                }

                joint.Offset = ibm;
            }

            if (joint.Childs?.Length > 0)
            {
                foreach (var child in joint.Childs)
                {
                    var ja = InitializeJoints(modelContent, child, skinController);

                    animations.AddRange(ja);
                }
            }

            return animations.ToArray();
        }
        /// <summary>
        /// Initialize mesh buffers in the graphics device
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="game">Game</param>
        /// <param name="name">Owner name</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        private static async Task InitializeMeshes(DrawingData drw, Game game, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer)
        {
            if (drw.Meshes?.Any() != true)
            {
                return;
            }

            Logger.WriteTrace(nameof(DrawingData), $"{name} Processing Material Meshes Dictionary => {drw.Meshes.Keys.AsEnumerable().Join("|")}");

            var beforeCount = game.BufferManager.PendingRequestCount;
            Logger.WriteTrace(nameof(DrawingData), $"{name} Pending Requests before Initialization => {beforeCount}");

            //Generates a task list from the materials-mesh dictionary
            var taskList = drw.Meshes.Values
                .Where(dictionary => dictionary?.Any() == true)
                .Select(dictionary => Task.Run(() => InitializeMeshDictionaryAsync(game, name, dynamicBuffers, instancingBuffer, dictionary)));

            try
            {
                await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(DrawingData), $"{name} Error processing parallel tasks => {ex.Message}", ex);

                throw;
            }

            var afterCount = game.BufferManager.PendingRequestCount;
            Logger.WriteTrace(nameof(DrawingData), $"{name} Pending Requests after Initialization => {afterCount}");
        }
        /// <summary>
        /// Initializes a mesh dictionary
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Owner name</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        /// <param name="dictionary">Mesh dictionary</param>
        private static void InitializeMeshDictionaryAsync(Game game, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer, Dictionary<string, Mesh> dictionary)
        {
            Logger.WriteTrace(nameof(DrawingData), $"{name} Processing Mesh Dictionary => {dictionary.Keys.AsEnumerable().Join("|")}");

            foreach (var mesh in dictionary.Values)
            {
                InitializeMesh(game, name, dynamicBuffers, instancingBuffer, mesh);
            }
        }
        /// <summary>
        /// Initializes a mesh
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Owner name</param>
        /// <param name="dynamicBuffers">Create dynamic buffers</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        /// <param name="mesh">Mesh</param>
        private static void InitializeMesh(Game game, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer, Mesh mesh)
        {
            try
            {
                Logger.WriteTrace(nameof(DrawingData), $"{name}.{mesh.Name} Processing Mesh => {mesh}");

                //Vertices
                mesh.VertexBuffer = game.BufferManager.AddVertexData($"{name}.{mesh.Name}", dynamicBuffers, mesh.Vertices, instancingBuffer);

                if (mesh.Indexed)
                {
                    //Indices
                    mesh.IndexBuffer = game.BufferManager.AddIndexData($"{name}.{mesh.Name}", dynamicBuffers, mesh.Indices);
                }

                Logger.WriteTrace(nameof(DrawingData), $"{name}.{mesh.Name} Processed Mesh => {mesh}");
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(DrawingData), $"{name}.{mesh.Name} Error Processing Mesh => {ex.Message}", ex);

                throw;
            }
        }
        /// <summary>
        /// Initialize lights
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static async Task InitializeLights(DrawingData drw, ContentData modelContent)
        {
            if (modelContent.Lights?.Any() != true)
            {
                return;
            }

            List<ISceneLight> modelLights = new List<ISceneLight>();

            await Task.Run(() =>
            {
                foreach (var l in modelContent.Lights.Values)
                {
                    if (l.LightType == LightContentTypes.Point)
                    {
                        modelLights.Add(l.CreatePointLight());
                    }
                    else if (l.LightType == LightContentTypes.Spot)
                    {
                        modelLights.Add(l.CreateSpotLight());
                    }
                }
            });

            drw.lights.AddRange(modelLights);
        }

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
        /// <summary>
        /// Dispose resources
        /// </summary>
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
            if (disposing)
            {
                foreach (var item in Meshes?.Values)
                {
                    foreach (var mesh in item.Values)
                    {
                        //Remove data from buffer manager
                        Game.BufferManager?.RemoveVertexData(mesh.VertexBuffer);

                        if (mesh.IndexBuffer != null)
                        {
                            Game.BufferManager?.RemoveIndexData(mesh.IndexBuffer);
                        }

                        //Dispose the mesh
                        mesh.Dispose();
                    }
                }
                Meshes?.Clear();
                Meshes = null;

                Materials?.Clear();
                Materials = null;

                //Don't dispose textures!
                Textures?.Clear();
                Textures = null;

                SkinningData = null;
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
            List<Vector3> points = new List<Vector3>();

            var meshMaterialList = Meshes.Values.ToArray();

            foreach (var dictionary in meshMaterialList)
            {
                var meshList = dictionary.Values.ToArray();

                foreach (var mesh in meshList)
                {
                    var meshPoints = mesh.GetPoints(refresh);
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
            List<Vector3> points = new List<Vector3>();

            var meshMaterialList = Meshes.Values.ToArray();

            foreach (var dictionary in meshMaterialList)
            {
                var meshList = dictionary.Values.ToArray();

                foreach (var mesh in meshList)
                {
                    var meshPoints = mesh.GetPoints(boneTransforms, refresh);
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
            List<Triangle> triangles = new List<Triangle>();

            var meshMaterialList = Meshes.Values.ToArray();

            foreach (var dictionary in meshMaterialList)
            {
                var meshList = dictionary.Values.ToArray();

                foreach (var mesh in meshList)
                {
                    var meshTriangles = mesh.GetTriangles(refresh);
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
            List<Triangle> triangles = new List<Triangle>();

            var meshMaterialList = Meshes.Values.ToArray();

            foreach (var dictionary in meshMaterialList)
            {
                var meshList = dictionary.Values.ToArray();

                foreach (var mesh in meshList)
                {
                    var meshTriangles = mesh.GetTriangles(boneTransforms, refresh);
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
            }

            return triangles.Distinct().ToArray();
        }

        /// <summary>
        /// Gets the first mesh by mesh name
        /// </summary>
        /// <param name="name">Name</param>
        public Mesh GetMeshByName(string name)
        {
            if (!Meshes.ContainsKey(name))
            {
                return null;
            }

            return Meshes[name].Values.FirstOrDefault();
        }

        /// <summary>
        /// Gets a copy of the lights collection
        /// </summary>
        public IEnumerable<ISceneLight> GetLights()
        {
            return lights.Select(l => l.Clone()).ToArray();
        }

        /// <summary>
        /// Skinning information
        /// </summary>
        struct SkinningInfo
        {
            /// <summary>
            /// Bind shape matrix
            /// </summary>
            public Matrix BindShapeMatrix;
            /// <summary>
            /// Weight list
            /// </summary>
            public IEnumerable<Weight> Weights;
            /// <summary>
            /// Bone names
            /// </summary>
            public IEnumerable<string> BoneNames;
        }

        /// <summary>
        /// Mesh information
        /// </summary>
        struct MeshInfo
        {
            /// <summary>
            /// Created mesh
            /// </summary>
            public Mesh Mesh;
            /// <summary>
            /// Material name
            /// </summary>
            public string MaterialName;

            /// <summary>
            /// Constructor
            /// </summary>
            public MeshInfo(Mesh mesh, string materialName)
            {
                Mesh = mesh;
                MaterialName = materialName;
            }
        }
    }
}
