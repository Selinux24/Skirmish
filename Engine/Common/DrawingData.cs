using SharpDX;
using System;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Common
{
    using Engine.Animation;
    using Engine.Content;

    /// <summary>
    /// Mesh data
    /// </summary>
    [Serializable]
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
        /// Datos de animación
        /// </summary>
        public SkinningData SkinningData = null;

        /// <summary>
        /// Model initialization
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="textureCount">Number of textures</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public static DrawingData Build(Game game, ModelContent modelContent, DrawingDataDescription description)
        {
            DrawingData res = new DrawingData();

            //Animation
            if (description.LoadAnimation) InitializeSkinnedData(ref res, game, modelContent);

            //Images
            InitializeTextures(ref res, game, modelContent, description.TextureCount);

            //Materials
            InitializeMaterials(ref res, game, modelContent);

            //Skins & Meshes
            InitializeGeometry(ref res, game, modelContent, description);

            //Update meshes into device
            InitializeMeshes(ref res, game);

            return res;
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="modelContent">Model content</param>
        private static void InitializeTextures(ref DrawingData drw, Game game, ModelContent modelContent, int textureCount)
        {
            if (modelContent.Images != null)
            {
                foreach (string images in modelContent.Images.Keys)
                {
                    ImageContent info = modelContent.Images[images];

                    ShaderResourceView view = info.CreateResource(game.Graphics.Device);
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
        /// <param name="modelContent">Model content</param>
        private static void InitializeMaterials(ref DrawingData drw, Game game, ModelContent modelContent)
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
        /// <param name="modelContent">Model content</param>
        /// <param name="instanced">Instaced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        private static void InitializeGeometry(ref DrawingData drw, Game game, ModelContent modelContent, DrawingDataDescription description)
        {
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
                        jointNames = modelContent.SkinningInfo.Skeleton.JointNames;

                        isSkinned = true;
                    }
                }

                foreach (string material in dictGeometry.Keys)
                {
                    SubMeshContent geometry = dictGeometry[material];

                    VertexTypes vertexType = geometry.VertexType;

                    if (isSkinned)
                    {
                        //Get skinned equivalent
                        vertexType = VertexData.GetSkinnedEquivalent(vertexType);
                    }

                    if (description.LoadNormalMaps)
                    {
                        if (!VertexData.IsTangent(vertexType))
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
                                    tmpVertices.Add(vertices[indices[i + 1]]);
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

                    Mesh nMesh = new Mesh(
                        geometry.Material,
                        geometry.Topology,
                        vertexList,
                        indices,
                        description.Instanced,
                        description.DynamicBuffers);

                    drw.Meshes.Add(meshName, geometry.Material, nMesh);
                }
            }
        }
        /// <summary>
        /// Initialize skinned data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="skinList">Skins</param>
        private static void InitializeSkinnedData(ref DrawingData drw, Game game, ModelContent modelContent)
        {
            if (modelContent.SkinningInfo != null)
            {
                InitializeJoints(modelContent, modelContent.SkinningInfo.Skeleton.Root);

                drw.SkinningData = SkinningData.Create(modelContent.SkinningInfo.Skeleton);
            }
        }

        private static void InitializeJoints(ModelContent modelContent, Joint rootJoint)
        {
            List<BoneAnimation> boneAnimations = new List<BoneAnimation>();

            //Find keyframes for current bone
            var c = FindJointKeyframes(rootJoint.Name, modelContent.Animations);
            if (c != null && c.Length > 0)
            {
                //Set bones
                Array.ForEach(c, (a) =>
                {
                    boneAnimations.Add(new BoneAnimation() { Joint = a.Joint, Keyframes = a.Keyframes });
                });
            }

            if (boneAnimations.Count > 0)
            {
                //TODO: Only one bone animation at a time
                rootJoint.Animations.Add(SkinningData.DefaultClip, boneAnimations.ToArray()[0]);
            }

            foreach (string controllerName in modelContent.SkinningInfo.Controller)
            {
                var controller = modelContent.Controllers[controllerName];

                Matrix ibm = Matrix.Identity;

                if (controller.InverseBindMatrix.ContainsKey(rootJoint.Name))
                {
                    ibm = controller.InverseBindMatrix[rootJoint.Name];
                }

                rootJoint.InverseBindMatrix = ibm;
            }

            if (rootJoint.Childs != null && rootJoint.Childs.Length > 0)
            {
                foreach (var child in rootJoint.Childs)
                {
                    InitializeJoints(modelContent, child);
                }
            }
        }
        /// <summary>
        /// Initialize mesh buffers in the graphics device
        /// </summary>
        private static void InitializeMeshes(ref DrawingData drw, Game game)
        {
            foreach (MeshMaterialsDictionary dictionary in drw.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    mesh.Initialize(game.Graphics.Device);
                }
            }
        }
        /// <summary>
        /// Find keyframes of a joint
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <param name="animations">Animation dictionary</param>
        /// <returns>Returns animation content of joint</returns>
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
        /// Constructor
        /// </summary>
        public DrawingData()
        {

        }

        /// <summary>
        /// Free resources from memory
        /// </summary>
        public void Dispose()
        {
            if (this.Meshes != null)
            {
                foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
                {
                    foreach (Mesh mesh in dictionary.Values)
                    {
                        mesh.Dispose();
                    }
                }
                this.Meshes.Clear();
                this.Meshes = null;
            }

            if (this.Materials != null)
            {
                foreach (MeshMaterial material in this.Materials.Values)
                {
                    if (material != null)
                    {
                        material.Dispose();
                    }
                }
                this.Materials.Clear();
                this.Materials = null;
            }

            if (this.Textures != null)
            {
                foreach (ShaderResourceView view in this.Textures.Values)
                {
                    if (view != null)
                    {
                        view.Dispose();
                    }
                }
                this.Textures.Clear();
                this.Textures = null;
            }

            if (this.SkinningData != null)
            {
                this.SkinningData = null;
            }
        }

        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <returns>Returns the drawing data's point list</returns>
        public Vector3[] GetPoints()
        {
            List<Vector3> points = new List<Vector3>();

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    Vector3[] meshPoints = mesh.GetPoints();
                    if (meshPoints != null && meshPoints.Length > 0)
                    {
                        points.AddRange(meshPoints);
                    }
                }
            }

            return points.ToArray();
        }
        /// <summary>
        /// Gets the drawing data's point list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <returns>Returns the drawing data's point list</returns>
        public Vector3[] GetPoints(Matrix transform)
        {
            Vector3[] points = this.GetPoints();

            if (transform != Matrix.Identity)
            {
                Vector3[] trnPoints = new Vector3[points.Length];
                Vector3.TransformCoordinate(points, ref transform, trnPoints);

                return trnPoints;
            }
            else
            {
                return points;
            }
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <returns>Returns the drawing data's triangle list</returns>
        public Triangle[] GetTriangles()
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    Triangle[] meshTriangles = mesh.GetTriangles();
                    if (meshTriangles != null && meshTriangles.Length > 0)
                    {
                        triangles.AddRange(meshTriangles);
                    }
                }
            }

            return triangles.ToArray();
        }
        /// <summary>
        /// Gets the drawing data's triangle list
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <returns>Returns the drawing data's triangle list</returns>
        public Triangle[] GetTriangles(Matrix transform)
        {
            Triangle[] triangles = this.GetTriangles();

            if (transform != Matrix.Identity)
            {
                return Triangle.Transform(triangles, transform);
            }
            else
            {
                return triangles;
            }
        }
    }
}
