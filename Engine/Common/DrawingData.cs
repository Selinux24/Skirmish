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
        /// <param name="modelContent">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public static DrawingData Build(Game game, LevelOfDetailEnum lod, ModelContent modelContent, bool instanced, int instances, bool loadAnimation, int textureCount, bool loadNormalMaps, bool dynamic)
        {
            DrawingData res = new DrawingData();

            //Images
            InitializeTextures(ref res, game, modelContent, textureCount);

            //Materials
            InitializeMaterials(ref res, game, modelContent);

            //Skins & Meshes
            InitializeGeometry(ref res, game, modelContent, instanced, instances, loadAnimation, loadNormalMaps, dynamic);

            //Animation
            if (loadAnimation) InitializeSkinnedData(ref res, game, modelContent);

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
        private static void InitializeGeometry(ref DrawingData drw, Game game, ModelContent modelContent, bool instanced, int instances, bool loadAnimation, bool loadNormalMaps, bool dynamic)
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
                if (loadAnimation && modelContent.Controllers != null && modelContent.SkinningInfo != null)
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

                    if (loadNormalMaps)
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
                        instanced,
                        dynamic);

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
                List<BoneAnimation> boneAnimations = new List<BoneAnimation>();

                foreach (string jointName in modelContent.SkinningInfo.Skeleton.JointNames)
                {
                    //Find keyframes for current bone
                    AnimationContent[] c = FindJointKeyframes(jointName, modelContent.Animations);

                    //Set bones
                    Array.ForEach(c, (a) =>
                    {
                        boneAnimations.Add(new BoneAnimation() { Keyframes = a.Keyframes });
                    });
                }

                //TODO: Animation dictionary is only for one animation
                Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();
                animations.Add(
                    SkinningData.DefaultClip,
                    new AnimationClip
                    {
                        BoneAnimations = boneAnimations.ToArray()
                    });

                Dictionary<string, SkinInfo> skinInfo = new Dictionary<string, SkinInfo>();

                foreach (string controllerName in modelContent.SkinningInfo.Controller)
                {
                    ControllerContent controller = modelContent.Controllers[controllerName];

                    List<Matrix> boneOffsets = new List<Matrix>();

                    foreach (string jointName in modelContent.SkinningInfo.Skeleton.JointNames)
                    {
                        Matrix ibm = Matrix.Identity;

                        if (controller.InverseBindMatrix.ContainsKey(jointName))
                        {
                            ibm = controller.InverseBindMatrix[jointName];
                        }

                        //Bind shape Matrix * Inverse shape Matrix -> Rest Position
                        boneOffsets.Add(controller.BindShapeMatrix * ibm);
                    }

                    skinInfo.Add(controller.Skin, new SkinInfo(boneOffsets.ToArray()));
                }

                drw.SkinningData = SkinningData.Create(
                    modelContent.SkinningInfo.Skeleton.JointIndices,
                    animations,
                    skinInfo);
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
