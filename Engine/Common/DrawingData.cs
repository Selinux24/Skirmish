﻿using SharpDX;
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
        /// Lights collection
        /// </summary>
        public IEnumerable<ISceneLight> Lights
        {
            get
            {
                return lights.ToArray();
            }
        }

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
                InitializeSkinningData(res, modelContent);
            }

            //Images
            InitializeTextures(res, game, modelContent, description.TextureCount);

            //Materials
            InitializeMaterials(res, modelContent);

            //Skins & Meshes
            await InitializeGeometry(res, modelContent, description);

            //Update meshes into device
            InitializeMeshes(res, game, name, description.DynamicBuffers, instancingBuffer);

            //Lights
            InitializeLights(res, modelContent);

            return await Task.FromResult(res);
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="textureCount">Texture count</param>
        private static void InitializeTextures(DrawingData drw, Game game, ContentData modelContent, int textureCount)
        {
            if (modelContent.Images == null)
            {
                return;
            }

            foreach (string images in modelContent.Images.Keys)
            {
                var info = modelContent.Images[images];

                var view = game.ResourceManager.RequestResource(info);
                if (view != null)
                {
                    drw.Textures.Add(images, view);

                    //Set the maximum texture index in the model
                    if (info.Count > textureCount) textureCount = info.Count;
                }
            }
        }
        /// <summary>
        /// Initialize materials
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeMaterials(DrawingData drw, ContentData modelContent)
        {
            foreach (string mat in modelContent.Materials?.Keys)
            {
                var effectInfo = modelContent.Materials[mat];

                var meshMaterial = effectInfo.CreateMeshMaterial(drw.Textures);

                drw.Materials.Add(mat, meshMaterial);
            }
        }
        /// <summary>
        /// Initilize geometry
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Description</param>
        private static async Task InitializeGeometry(DrawingData drw, ContentData modelContent, DrawingDataDescription description)
        {
            foreach (string meshName in modelContent.Geometry.Keys)
            {
                await InitializeGeometryMesh(drw, modelContent, description, meshName);
            }
        }
        /// <summary>
        /// Initialize geometry mesh
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Description</param>
        /// <param name="meshName">Mesh name</param>
        private static async Task InitializeGeometryMesh(DrawingData drw, ContentData modelContent, DrawingDataDescription description, string meshName)
        {
            //Get skinning data
            var isSkinned = ReadSkinningData(
                description, modelContent, meshName,
                out var bindShapeMatrix, out var weights, out var jointNames);

            //Process the mesh geometry material by material
            var dictGeometry = modelContent.Geometry[meshName];

            foreach (string material in dictGeometry.Keys)
            {
                var geometry = dictGeometry[material];

                if (geometry.IsVolume)
                {
                    //If volume, store position only
                    drw.volumeMesh.AddRange(geometry.GetTriangles());

                    continue;
                }

                //Get vertex type
                var vertexType = GetVertexType(geometry.VertexType, isSkinned, description.LoadNormalMaps, drw.Materials, material);

                //Process the vertex data
                var vertexData = await geometry.ProcessVertexData(vertexType, description.Constraint);
                var vertices = vertexData.vertices;
                var indices = vertexData.indices;

                if (!bindShapeMatrix.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, bindShapeMatrix);
                }

                //Convert the vertex data to final mesh data
                var vertexList = VertexData.Convert(
                    vertexType,
                    vertices,
                    weights,
                    jointNames);

                if (vertexList.Any())
                {
                    //Create and store the mesh into the drawing data
                    Mesh nMesh = new Mesh(
                        meshName,
                        geometry.Topology,
                        geometry.Transform,
                        vertexList,
                        indices);

                    if (!drw.Meshes.ContainsKey(meshName))
                    {
                        drw.Meshes.Add(meshName, new Dictionary<string, Mesh>());
                    }

                    string materialName = string.IsNullOrEmpty(geometry.Material) ? ContentData.NoMaterial : geometry.Material;

                    drw.Meshes[meshName].Add(materialName, nMesh);
                }
            }
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
        /// <param name="bindShapeMatrix">Resulting bind shape matrix</param>
        /// <param name="weights">Resulting weights</param>
        /// <param name="jointNames">Resulting joints</param>
        /// <returns>Returns true if the model has skinnging data</returns>
        private static bool ReadSkinningData(DrawingDataDescription description, ContentData modelContent, string meshName, out Matrix bindShapeMatrix, out IEnumerable<Weight> weights, out IEnumerable<string> jointNames)
        {
            bindShapeMatrix = Matrix.Identity;
            weights = null;
            jointNames = null;

            if (description.LoadAnimation && modelContent.Controllers != null && modelContent.SkinningInfo != null)
            {
                var cInfo = modelContent.GetControllerForMesh(meshName);
                if (cInfo != null)
                {
                    //Apply shape matrix if controller exists but we are not loading animation info
                    bindShapeMatrix = cInfo.BindShapeMatrix;
                    weights = cInfo.Weights;

                    //Find skeleton for controller
                    var sInfo = modelContent.SkinningInfo[cInfo.Armature];
                    jointNames = sInfo.Skeleton.GetJointNames();

                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Initialize skinning data
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeSkinningData(DrawingData drw, ContentData modelContent)
        {
            if (modelContent.SkinningInfo?.Any() != true)
            {
                return;
            }

            //Use the definition to read animation data into a clip dictionary
            foreach (var sInfo in modelContent.SkinningInfo.Values)
            {
                if (drw.SkinningData != null)
                {
                    continue;
                }

                drw.SkinningData = new SkinningData(sInfo.Skeleton);

                var animations = InitializeJoints(modelContent, sInfo.Skeleton.Root, sInfo.Controllers);

                drw.SkinningData.Initialize(
                    animations,
                    modelContent.AnimationDefinition);
            }
        }
        /// <summary>
        /// Initialize skeleton data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="joint">Joint to initialize</param>
        /// <param name="animations">Animation list to feed</param>
        private static IEnumerable<JointAnimation> InitializeJoints(ContentData modelContent, Joint joint, IEnumerable<string> skinController)
        {
            List<JointAnimation> animations = new List<JointAnimation>();

            List<JointAnimation> boneAnimations = new List<JointAnimation>();

            //Find keyframes for current bone
            var c = FindJointKeyframes(joint.Name, modelContent.Animations);
            if (c?.Any() == true)
            {
                //Set bones
                var ja = c.Select(a => new JointAnimation(a.Joint, a.Keyframes)).ToArray();
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

                if (controller.InverseBindMatrix.ContainsKey(joint.Name))
                {
                    ibm = controller.InverseBindMatrix[joint.Name];
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
        private static void InitializeMeshes(DrawingData drw, Game game, string name, bool dynamicBuffers, BufferDescriptor instancingBuffer)
        {
            foreach (var dictionary in drw.Meshes.Values)
            {
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
            }
        }
        /// <summary>
        /// Find keyframes for a joint
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <param name="animations">Animation dictionary</param>
        /// <returns>Returns joint's animation content</returns>
        private static IEnumerable<AnimationContent> FindJointKeyframes(string jointName, Dictionary<string, IEnumerable<AnimationContent>> animations)
        {
            foreach (string key in animations.Keys)
            {
                if (animations[key].Any(a => a.Joint == jointName))
                {
                    return animations[key].Where(a => a.Joint == jointName).ToArray();
                }
            }

            return Enumerable.Empty<AnimationContent>();
        }
        /// <summary>
        /// Initialize lights
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeLights(DrawingData drw, ContentData modelContent)
        {
            foreach (var key in modelContent.Lights.Keys)
            {
                var l = modelContent.Lights[key];

                if (l.LightType == LightContentTypes.Point)
                {
                    drw.lights.Add(l.CreatePointLight());
                }
                else if (l.LightType == LightContentTypes.Spot)
                {
                    drw.lights.Add(l.CreateSpotLight());
                }
            }
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
    }
}
