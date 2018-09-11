using SharpDX;
using System;
using System.Collections.Generic;

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
        /// Materials dictionary
        /// </summary>
        public MaterialDictionary Materials = new MaterialDictionary();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        public TextureDictionary Textures = new TextureDictionary();
        /// <summary>
        /// Meshes
        /// </summary>
        public MeshDictionary Meshes = new MeshDictionary();
        /// <summary>
        /// Volume mesh
        /// </summary>
        public Triangle[] VolumeMesh = null;
        /// <summary>
        /// Datos de animación
        /// </summary>
        public SkinningData SkinningData = null;
        /// <summary>
        /// Lights collection
        /// </summary>
        public SceneLight[] Lights = null;

        /// <summary>
        /// Buffer manager
        /// </summary>
        protected BufferManager BufferManager = null;

        /// <summary>
        /// Model initialization
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Data description</param>
        /// <returns>Returns the generated drawing data objects</returns>
        public static DrawingData Build(Game game, BufferManager bufferManager, ModelContent modelContent, DrawingDataDescription description)
        {
            DrawingData res = new DrawingData(bufferManager);

            //Animation
            if (description.LoadAnimation)
            {
                InitializeSkinningData(ref res, modelContent);
            }

            //Images
            InitializeTextures(ref res, game, modelContent, description.TextureCount);

            //Materials
            InitializeMaterials(ref res, modelContent);

            //Skins & Meshes
            InitializeGeometry(ref res, modelContent, description);

            //Update meshes into device
            InitializeMeshes(ref res, bufferManager, description.Instances);

            //Lights
            InitializeLights(ref res, modelContent);

            return res;
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="textureCount">Texture count</param>
        private static void InitializeTextures(ref DrawingData drw, Game game, ModelContent modelContent, int textureCount)
        {
            if (modelContent.Images != null)
            {
                foreach (string images in modelContent.Images.Keys)
                {
                    var info = modelContent.Images[images];

                    var view = game.ResourceManager.CreateResource(info);
                    if (view != null)
                    {
                        drw.Textures.Add(images, view);

                        //Set the maximum texture index in the model
                        if (info.Count > textureCount) textureCount = info.Count;
                    }
                }
            }
        }
        /// <summary>
        /// Initialize materials
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeMaterials(ref DrawingData drw, ModelContent modelContent)
        {
            if (modelContent.Materials != null)
            {
                foreach (string mat in modelContent.Materials.Keys)
                {
                    MaterialContent effectInfo = modelContent.Materials[mat];

                    MeshMaterial meshMaterial = new MeshMaterial()
                    {
                        Material = new Material(effectInfo),

                        EmissionTexture = drw.Textures[effectInfo.EmissionTexture],
                        AmbientTexture = drw.Textures[effectInfo.AmbientTexture],
                        DiffuseTexture = drw.Textures[effectInfo.DiffuseTexture],
                        SpecularTexture = drw.Textures[effectInfo.SpecularTexture],
                        ReflectiveTexture = drw.Textures[effectInfo.ReflectiveTexture],
                        NormalMap = drw.Textures[effectInfo.NormalMapTexture],
                    };

                    drw.Materials.Add(mat, meshMaterial);
                }
            }
        }
        /// <summary>
        /// Initilize geometry
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Description</param>
        private static void InitializeGeometry(ref DrawingData drw, ModelContent modelContent, DrawingDataDescription description)
        {
            List<Triangle> volumeMesh = new List<Triangle>();

            foreach (string meshName in modelContent.Geometry.Keys)
            {
                Dictionary<string, SubMeshContent> dictGeometry = modelContent.Geometry[meshName];

                bool isSkinned = false;
                ControllerContent cInfo = null;
                Matrix bindShapeMatrix = Matrix.Identity;
                VertexData[] vertices = null;
                uint[] indices = null;
                Weight[] weights = null;
                string[] jointNames = null;
                if (description.LoadAnimation && modelContent.Controllers != null && modelContent.SkinningInfo != null)
                {
                    cInfo = modelContent.Controllers.GetControllerForMesh(meshName);
                    if (cInfo != null)
                    {
                        //Apply shape matrix if controller exists but we are not loading animation info
                        bindShapeMatrix = cInfo.BindShapeMatrix;
                        weights = cInfo.Weights;
                        jointNames = modelContent.SkinningInfo.Skeleton.GetJointNames();

                        isSkinned = true;
                    }
                }

                foreach (string material in dictGeometry.Keys)
                {
                    SubMeshContent geometry = dictGeometry[material];

                    if (geometry.IsVolume)
                    {
                        volumeMesh.AddRange(geometry.GetTriangles());
                    }
                    else
                    {
                        VertexTypes vertexType = geometry.VertexType;

                        if (isSkinned)
                        {
                            //Get skinned equivalent
                            vertexType = VertexData.GetSkinnedEquivalent(vertexType);
                        }

                        if (description.LoadNormalMaps &&
                            VertexData.IsTextured(vertexType) &&
                            !VertexData.IsTangent(vertexType))
                        {
                            MeshMaterial meshMaterial = drw.Materials[material];
                            if (meshMaterial.NormalMap != null)
                            {
                                //Get tangent equivalent
                                vertexType = VertexData.GetTangentEquivalent(vertexType);

                                //Compute tangents
                                geometry.ComputeTangents();
                            }
                        }

                        vertices = geometry.Vertices;
                        indices = geometry.Indices;

                        if (description.Constraint.HasValue)
                        {
                            List<VertexData> tmpVertices = new List<VertexData>();
                            List<uint> tmpIndices = new List<uint>();

                            if (indices != null && indices.Length > 0)
                            {
                                uint index = 0;
                                for (int i = 0; i < indices.Length; i += 3)
                                {
                                    if (description.Constraint.Value.Contains(vertices[indices[i + 0]].Position.Value) != ContainmentType.Disjoint ||
                                        description.Constraint.Value.Contains(vertices[indices[i + 1]].Position.Value) != ContainmentType.Disjoint ||
                                        description.Constraint.Value.Contains(vertices[indices[i + 1]].Position.Value) != ContainmentType.Disjoint)
                                    {
                                        tmpVertices.Add(vertices[indices[i + 0]]);
                                        tmpVertices.Add(vertices[indices[i + 1]]);
                                        tmpVertices.Add(vertices[indices[i + 2]]);
                                        tmpIndices.Add(index++);
                                        tmpIndices.Add(index++);
                                        tmpIndices.Add(index++);
                                    }
                                }

                                vertices = tmpVertices.ToArray();
                                indices = tmpIndices.ToArray();
                            }
                            else
                            {
                                for (int i = 0; i < vertices.Length; i += 3)
                                {
                                    if (description.Constraint.Value.Contains(vertices[i + 0].Position.Value) != ContainmentType.Disjoint ||
                                        description.Constraint.Value.Contains(vertices[i + 1].Position.Value) != ContainmentType.Disjoint ||
                                        description.Constraint.Value.Contains(vertices[i + 2].Position.Value) != ContainmentType.Disjoint)
                                    {
                                        tmpVertices.Add(vertices[i + 0]);
                                        tmpVertices.Add(vertices[i + 1]);
                                        tmpVertices.Add(vertices[i + 2]);
                                    }
                                }

                                vertices = tmpVertices.ToArray();
                            }
                        }

                        IVertexData[] vertexList = VertexData.Convert(
                            vertexType,
                            vertices,
                            weights,
                            jointNames,
                            bindShapeMatrix);

                        if (vertexList.Length > 0)
                        {
                            Mesh nMesh = new Mesh(
                                meshName,
                                geometry.Material,
                                geometry.Transparent,
                                geometry.Topology,
                                vertexList,
                                indices,
                                description.Instanced);

                            drw.Meshes.Add(meshName, geometry.Material, nMesh);
                        }
                    }
                }
            }

            drw.VolumeMesh = volumeMesh.ToArray();
        }
        /// <summary>
        /// Initialize skinning data
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeSkinningData(ref DrawingData drw, ModelContent modelContent)
        {
            if (modelContent.SkinningInfo != null)
            {
                //Use the definition to read animation data into a clip dictionary
                List<JointAnimation> animations = new List<JointAnimation>();

                InitializeJoints(modelContent, modelContent.SkinningInfo.Skeleton.Root, animations);

                drw.SkinningData = new SkinningData(
                    modelContent.SkinningInfo.Skeleton,
                    animations.ToArray(),
                    modelContent.Animations.Definition);
            }
        }
        /// <summary>
        /// Initialize skeleton data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="joint">Joint to initialize</param>
        /// <param name="animations">Animation list to feed</param>
        private static void InitializeJoints(ModelContent modelContent, Joint joint, List<JointAnimation> animations)
        {
            List<JointAnimation> boneAnimations = new List<JointAnimation>();

            //Find keyframes for current bone
            var c = FindJointKeyframes(joint.Name, modelContent.Animations);
            if (c != null && c.Length > 0)
            {
                //Set bones
                Array.ForEach(c, (a) =>
                {
                    boneAnimations.Add(new JointAnimation(a.Joint, a.Keyframes));
                });
            }

            if (boneAnimations.Count > 0)
            {
                //TODO: Only one bone animation for now
                animations.Add(boneAnimations[0]);
            }

            foreach (string controllerName in modelContent.SkinningInfo.Controller)
            {
                var controller = modelContent.Controllers[controllerName];

                Matrix ibm = Matrix.Identity;

                if (controller.InverseBindMatrix.ContainsKey(joint.Name))
                {
                    ibm = controller.InverseBindMatrix[joint.Name];
                }

                joint.Offset = ibm;
            }

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                foreach (var child in joint.Childs)
                {
                    InitializeJoints(modelContent, child, animations);
                }
            }
        }
        /// <summary>
        /// Initialize mesh buffers in the graphics device
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="game">Game</param>
        private static void InitializeMeshes(ref DrawingData drw, BufferManager bufferManager, int instances)
        {
            foreach (MeshMaterialsDictionary dictionary in drw.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    //Vertices
                    mesh.VertexBuffer = bufferManager.Add(mesh.Name, mesh.Vertices, false, mesh.Instanced ? instances : 0);

                    if (mesh.Indexed)
                    {
                        //Indices
                        mesh.IndexBuffer = bufferManager.Add(mesh.Name, mesh.Indices, false);
                    }
                    else
                    {
                        mesh.IndexBuffer = new BufferDescriptor(-1, -1, 0);
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
        private static AnimationContent[] FindJointKeyframes(string jointName, Dictionary<string, AnimationContent[]> animations)
        {
            foreach (string key in animations.Keys)
            {
                if (Array.Exists(animations[key], a => a.Joint == jointName))
                {
                    return Array.FindAll(animations[key], a => a.Joint == jointName);
                }
            }

            return null;
        }
        /// <summary>
        /// Initialize lights
        /// </summary>
        /// <param name="drw">Drawing data</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeLights(ref DrawingData drw, ModelContent modelContent)
        {
            List<SceneLight> lights = new List<SceneLight>();

            foreach (var key in modelContent.Lights.Keys)
            {
                var l = modelContent.Lights[key];

                if (l.LightType == LightContentTypeEnum.Point)
                {
                    lights.Add(l.CreatePointLight());
                }
                else if (l.LightType == LightContentTypeEnum.Spot)
                {
                    lights.Add(l.CreateSpotLight());
                }
            }

            drw.Lights = lights.ToArray();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bufferManager">Buffer manager</param>
        public DrawingData(BufferManager bufferManager)
        {
            this.BufferManager = bufferManager;
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
                if (this.BufferManager != null)
                {
                    //Remove data from buffer manager
                    foreach (var dictionary in this.Meshes.Values)
                    {
                        foreach (var mesh in dictionary.Values)
                        {
                            this.BufferManager.RemoveVertexData(mesh.VertexBuffer);
                            this.BufferManager.RemoveIndexData(mesh.IndexBuffer);
                        }
                    }
                }

                if (this.Meshes != null)
                {
                    foreach (var item in this.Meshes)
                    {
                        foreach (var value in item.Value)
                        {
                            value.Value?.Dispose();
                        }
                    }

                    this.Meshes.Clear();
                    this.Meshes = null;
                }

                if (this.Materials != null)
                {
                    foreach (var item in this.Materials)
                    {
                        item.Value?.Dispose();
                    }

                    this.Materials.Clear();
                    this.Materials = null;
                }

                if (this.Textures != null)
                {
                    //Don't dispose textures!
                    this.Textures.Clear();
                    this.Textures = null;
                }

                this.SkinningData = null;
            }
        }

        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public Vector3[] GetPoints(bool refresh = false)
        {
            return this.GetPoints(Matrix.Identity, refresh);
        }
        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public Vector3[] GetPoints(Matrix transform, bool refresh = false)
        {
            List<Vector3> points = new List<Vector3>();

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    Vector3[] meshPoints = mesh.GetPoints(refresh);
                    if (meshPoints != null && meshPoints.Length > 0)
                    {
                        Vector3[] trnPoints = new Vector3[meshPoints.Length];
                        Vector3.TransformCoordinate(meshPoints, ref transform, trnPoints);
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
        public Vector3[] GetPoints(Matrix[] boneTransforms, bool refresh = false)
        {
            return this.GetPoints(Matrix.Identity, boneTransforms, refresh);
        }
        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="transform">Global transform</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's point list</returns>
        public Vector3[] GetPoints(Matrix transform, Matrix[] boneTransforms, bool refresh = false)
        {
            List<Vector3> points = new List<Vector3>();

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    Vector3[] meshPoints = mesh.GetPoints(boneTransforms, refresh);
                    if (meshPoints != null && meshPoints.Length > 0)
                    {
                        Vector3[] trnPoints = new Vector3[meshPoints.Length];
                        Vector3.TransformCoordinate(meshPoints, ref transform, trnPoints);
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
        public Triangle[] GetTriangles(bool refresh = false)
        {
            return this.GetTriangles(Matrix.Identity, refresh);
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public Triangle[] GetTriangles(Matrix transform, bool refresh = false)
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    Triangle[] meshTriangles = mesh.GetTriangles(refresh);
                    meshTriangles = Triangle.Transform(meshTriangles, transform);
                    triangles.AddRange(meshTriangles);
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
        public Triangle[] GetTriangles(Matrix[] boneTransforms, bool refresh = false)
        {
            return this.GetTriangles(Matrix.Identity, boneTransforms, refresh);
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <param name="boneTransforms">Bone transforms list</param>
        /// <param name="refresh">Sets if the cache must be refresehd or not</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public Triangle[] GetTriangles(Matrix transform, Matrix[] boneTransforms, bool refresh = false)
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    Triangle[] meshTriangles = mesh.GetTriangles(boneTransforms, refresh);
                    meshTriangles = Triangle.Transform(meshTriangles, transform);
                    triangles.AddRange(meshTriangles);
                }
            }

            return triangles.ToArray();
        }
    }
}
