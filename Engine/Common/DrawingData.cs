using SharpDX;
using System;
using System.Collections.Concurrent;
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
        /// Volume mesh triangle list
        /// </summary>
        private readonly List<Triangle> volumeMesh = new List<Triangle>();
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
        public ConcurrentDictionary<string, IMeshMaterial> Materials { get; private set; } = new ConcurrentDictionary<string, IMeshMaterial>();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        public ConcurrentDictionary<string, EngineShaderResourceView> Textures { get; private set; } = new ConcurrentDictionary<string, EngineShaderResourceView>();
        /// <summary>
        /// Meshes
        /// </summary>
        public ConcurrentDictionary<string, ConcurrentDictionary<string, Mesh>> Meshes { get; private set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, Mesh>>();
        /// <summary>
        /// Volume mesh
        /// </summary>
        public IEnumerable<Triangle> VolumeMesh
        {
            get
            {
                return volumeMesh.ToArray();
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
            await InitializeTextures(res, game, modelContent, description.TextureCount);

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
        /// <param name="textureCount">Texture count</param>
        private static async Task InitializeTextures(DrawingData drw, Game game, ContentData modelContent, int textureCount)
        {
            if (modelContent.Images?.Any() != true)
            {
                return;
            }

            var taskList = modelContent.Images.Select(images =>
            {
                return Task.Run(() =>
                {
                    var info = images.Value;

                    var view = game.ResourceManager.RequestResource(info);
                    if (view == null)
                    {
                        Logger.WriteWarning(nameof(DrawingData), $"Texture cannot be requested: {info}");
                    }

                    if (!Helper.Retry(() => drw.Textures.TryAdd(images.Key, view), 5))
                    {
                        Logger.WriteWarning(nameof(DrawingData), $"Error adding texture {images.Key} to the texture dictionary.");
                    }

                    return info.Count;
                });
            });
            var taskResults = await Task.WhenAll(taskList);

            foreach (var result in taskResults)
            {
                //Set the maximum texture index in the model
                if (result > textureCount) textureCount = result;
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

            var taskList = modelContent.Materials.Select(mat =>
            {
                return Task.Run(() =>
                {
                    var matName = mat.Key;
                    var matContent = mat.Value;

                    var meshMaterial = matContent.CreateMeshMaterial(drw.Textures);

                    if (!Helper.Retry(() => drw.Materials.TryAdd(matName, meshMaterial), 5))
                    {
                        Logger.WriteWarning(nameof(DrawingData), $"Error adding material {matName} to the material dictionary.");
                    }
                });
            });
            await Task.WhenAll(taskList);
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

            var taskList = modelContent.Geometry
                .Select(mesh =>
                {
                    //Get the mesh geometry
                    var submeshes = modelContent.Geometry[mesh.Key];

                    //Get the mesh skinning info
                    var skinningInfo = ReadSkinningData(description, modelContent, mesh.Key);

                    return InitializeGeometryMesh(drw, description, skinningInfo, mesh.Key, submeshes);
                })
                .ToList();

            List<Exception> ex = new List<Exception>();

            while (taskList.Any())
            {
                var t = await Task.WhenAny(taskList);

                taskList.Remove(t);

                bool completedOk = t.Status == TaskStatus.RanToCompletion;
                if (!completedOk)
                {
                    ex.Add(t.Exception);
                }
            }

            if (ex.Any())
            {
                throw new AggregateException(ex);
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

            //Extract volumes
            var volumeTriangles = submeshes
                .Where(g => g.Value.IsVolume)
                .SelectMany(material => material.Value.GetTriangles())
                .ToArray();
            drw.volumeMesh.AddRange(volumeTriangles);

            //Extract meshes
            var taskList = submeshes
                .Where(g => !g.Value.IsVolume)
                .Select(material =>
                {
                    var geometry = material.Value;

                    //Get vertex type
                    var vertexType = GetVertexType(geometry.VertexType, isSkinned, description.LoadNormalMaps, drw.Materials, material.Key);

                    return CreateMesh(meshName, geometry, vertexType, description.Constraint, skinningInfo);
                });
            var taskResults = await Task.WhenAll(taskList);

            foreach (var result in taskResults)
            {
                if (!result.Any())
                {
                    continue;
                }

                var nMesh = result.First().Mesh;
                var materialName = result.First().MaterialName;

                if (!drw.Meshes.ContainsKey(meshName))
                {
                    var dict = new ConcurrentDictionary<string, Mesh>();
                    dict.TryAdd(materialName, nMesh);

                    if (!Helper.Retry(() => drw.Meshes.TryAdd(meshName, dict), 10))
                    {
                        Logger.WriteWarning(nameof(DrawingData), $"Error adding new mesh {meshName} to the mesh dictionary.");
                    }
                }
                else
                {
                    if (!Helper.Retry(() => drw.Meshes[meshName].TryAdd(materialName, nMesh), 10))
                    {
                        Logger.WriteWarning(nameof(DrawingData), $"Error adding a {meshName} submesh to the mesh dictionary.");
                    }
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
        private static VertexTypes GetVertexType(VertexTypes vertexType, bool isSkinned, bool loadNormalMaps, ConcurrentDictionary<string, IMeshMaterial> materials, string material)
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

            var taskList = drw.Meshes.Values.Select(dictionary =>
            {
                return Task.Run(() =>
                {
                    if (!dictionary.Any())
                    {
                        return;
                    }

                    foreach (var mesh in dictionary.Values)
                    {
                        //Vertices
                        mesh.VertexBuffer = game.BufferManager.AddVertexData($"{name}.{mesh.Name}", dynamicBuffers, mesh.Vertices, instancingBuffer);

                        if (mesh.Indexed)
                        {
                            //Indices
                            mesh.IndexBuffer = game.BufferManager.AddIndexData($"{name}.{mesh.Name}", dynamicBuffers, mesh.Indices);
                        }
                    }
                });
            });
            await Task.WhenAll(taskList);
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

            List<ISceneLight> lights = new List<ISceneLight>();

            await Task.Run(() =>
            {
                foreach (var l in modelContent.Lights.Values)
                {
                    if (l.LightType == LightContentTypes.Point)
                    {
                        lights.Add(l.CreatePointLight());
                    }
                    else if (l.LightType == LightContentTypes.Spot)
                    {
                        lights.Add(l.CreateSpotLight());
                    }
                }
            });

            drw.lights.AddRange(lights);
        }
        /// <summary>
        /// Lights collection
        /// </summary>
        public IEnumerable<ISceneLight> GetLights()
        {
            return lights.Select(l => l.Clone()).ToArray();
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
                    if (meshPoints.Any())
                    {
                        var trnPoints = meshPoints.ToArray();
                        Vector3.TransformCoordinate(trnPoints, ref transform, trnPoints);
                        points.AddRange(trnPoints);
                    }
                }
            }

            return points.ToArray();
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
                    if (meshPoints.Any())
                    {
                        var trnPoints = meshPoints.ToArray();
                        Vector3.TransformCoordinate(trnPoints, ref transform, trnPoints);
                        points.AddRange(trnPoints);
                    }
                }
            }

            return points.ToArray();
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
                    triangles.AddRange(Triangle.Transform(meshTriangles, transform));
                }
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
            List<Triangle> triangles = new List<Triangle>();

            var meshMaterialList = Meshes.Values.ToArray();

            foreach (var dictionary in meshMaterialList)
            {
                var meshList = dictionary.Values.ToArray();

                foreach (var mesh in meshList)
                {
                    var meshTriangles = mesh.GetTriangles(boneTransforms, refresh);
                    triangles.AddRange(Triangle.Transform(meshTriangles, transform));
                }
            }

            return triangles.ToArray();
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
