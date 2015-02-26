﻿using System;
using System.Collections.Generic;
using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Common
{
    using Engine.Content;
    using Engine.Helpers;

    /// <summary>
    /// Model basic implementation
    /// </summary>
    public abstract class ModelBase : Drawable
    {
        #region Classes

        /// <summary>
        /// Mesh by mesh name dictionary
        /// </summary>
        /// <remarks>
        /// A mesh could be composed of one or more sub-meshes, depending on the number of different specified materials
        /// Key: mesh name
        /// Value: dictionary of meshes by material
        /// </remarks>
        protected class MeshDictionary : Dictionary<string, MeshMaterialsDictionary>
        {
            /// <summary>
            /// Adds new mesh to dictionary
            /// </summary>
            /// <param name="meshName">Mesh name</param>
            /// <param name="materialName">Material name</param>
            /// <param name="mesh">Mesh object</param>
            public void Add(string meshName, string materialName, Mesh mesh)
            {
                if (!this.ContainsKey(meshName))
                {
                    this.Add(meshName, new MeshMaterialsDictionary());
                }

                this[meshName].Add(string.IsNullOrEmpty(materialName) ? ModelContent.NoMaterial : materialName, mesh);
            }
        }
        /// <summary>
        /// Mesh by material dictionary
        /// </summary>
        protected class MeshMaterialsDictionary : Dictionary<string, Mesh>
        {

        }
        /// <summary>
        /// Material by name dictionary
        /// </summary>
        protected class MaterialDictionary : Dictionary<string, MeshMaterial>
        {
            /// <summary>
            /// Gets material description by name
            /// </summary>
            /// <param name="material">Material name</param>
            /// <returns>Return material description by name if exists</returns>
            public new MeshMaterial this[string material]
            {
                get
                {
                    if (!string.IsNullOrEmpty(material))
                    {
                        if (base.ContainsKey(material))
                        {
                            return base[material];
                        }
                    }

                    return null;
                }
            }
        }
        /// <summary>
        /// Texture by material dictionary
        /// </summary>
        protected class TextureDictionary : Dictionary<string, ShaderResourceView>
        {
            /// <summary>
            /// Gets textures by image name
            /// </summary>
            /// <param name="image">Image name</param>
            /// <returns>Return texture by image name if exists</returns>
            public new ShaderResourceView this[string image]
            {
                get
                {
                    if (!string.IsNullOrEmpty(image))
                    {
                        if (!base.ContainsKey(image))
                        {
                            throw new Exception(string.Format("Texture resource not found: {0}", image));
                        }

                        return base[image];
                    }

                    return null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Materials dictionary
        /// </summary>
        protected MaterialDictionary Materials = new MaterialDictionary();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        protected TextureDictionary Textures = new TextureDictionary();
        /// <summary>
        /// Meshes
        /// </summary>
        protected MeshDictionary Meshes = new MeshDictionary();
        /// <summary>
        /// Datos de animación
        /// </summary>
        protected SkinningData SkinningData = null;

        #region Static Helpers

        /// <summary>
        /// Find keyframes of a joint
        /// </summary>
        /// <param name="jointName">Joint name</param>
        /// <param name="animations">Animation dictionary</param>
        /// <returns>Returns animation content of joint</returns>
        private static AnimationContent[] FindJointKeyframes(
            string jointName,
            Dictionary<string, AnimationContent[]> animations)
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

        #endregion

        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="content">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        public ModelBase(Game game, ModelContent content, bool instanced = false, int instances = 0, bool loadAnimation = true, bool loadNormalMaps = true)
            : base(game)
        {
            this.Initialize(content, instanced, instances, loadAnimation, loadNormalMaps);
        }

        /// <summary>
        /// Model initialization
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        protected virtual void Initialize(ModelContent modelContent, bool instanced, int instances, bool loadAnimation, bool loadNormalMaps)
        {
            //Images
            this.InitializeTextures(modelContent);

            //Materials
            this.InitializeMaterials(modelContent);

            //Skins & Meshes
            this.InitializeGeometry(modelContent, instanced, instances, loadAnimation, loadNormalMaps);

            //Animation
            if (loadAnimation) this.InitializeSkinnedData(modelContent);

            //Update meshes into device
            this.InitializeMeshes();
        }
        /// <summary>
        /// Initialize textures
        /// </summary>
        /// <param name="modelContent">Model content</param>
        protected virtual void InitializeTextures(ModelContent modelContent)
        {
            if (modelContent.Images != null)
            {
                foreach (string images in modelContent.Images.Keys)
                {
                    ImageContent info = modelContent.Images[images];

                    ShaderResourceView view = null;

                    if (info.Stream != null)
                    {
                        byte[] buffer = info.Stream.GetBuffer();

                        view = this.Game.Graphics.Device.LoadTexture(buffer);
                    }
                    else
                    {
                        if (info.IsArray)
                        {
                            string[] res = info.Paths;

                            view = this.Game.Graphics.Device.LoadTextureArray(res);
                        }
                        else if (info.IsCubic)
                        {
                            string res = info.Path;
                            int faceSize = info.CubicFaceSize;

                            view = this.Game.Graphics.Device.LoadTextureCube(res, faceSize);
                        }
                        else
                        {
                            string res = info.Path;

                            view = this.Game.Graphics.Device.LoadTexture(res);
                        }
                    }

                    if (view != null)
                    {
                        this.Textures.Add(images, view);
                    }
                }
            }
        }
        /// <summary>
        /// Initialize materials
        /// </summary>
        /// <param name="modelContent">Model content</param>
        protected virtual void InitializeMaterials(ModelContent modelContent)
        {
            if (modelContent.Materials != null)
            {
                foreach (string mat in modelContent.Materials.Keys)
                {
                    MaterialContent effectInfo = modelContent.Materials[mat];

                    MeshMaterial meshMaterial = new MeshMaterial()
                    {
                        Material = new Material(effectInfo),

                        EmissionTexture = this.Textures[effectInfo.EmissionTexture],
                        AmbientTexture = this.Textures[effectInfo.AmbientTexture],
                        DiffuseTexture = this.Textures[effectInfo.DiffuseTexture],
                        SpecularTexture = this.Textures[effectInfo.SpecularTexture],
                        ReflectiveTexture = this.Textures[effectInfo.ReflectiveTexture],
                        NormalMap = this.Textures[effectInfo.NormalMapTexture],
                    };

                    this.Materials.Add(mat, meshMaterial);
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
        protected virtual void InitializeGeometry(ModelContent modelContent, bool instanced, int instances, bool loadAnimation, bool loadNormalMaps)
        {
            foreach (string meshName in modelContent.Geometry.Keys)
            {
                Dictionary<string, SubMeshContent> dictGeometry = modelContent.Geometry[meshName];

                bool isSkinned = false;
                ControllerContent cInfo = null;
                Matrix bindShapeMatrix = Matrix.Identity;
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
                            MeshMaterial meshMaterial = this.Materials[material];
                            if (meshMaterial.NormalMap != null)
                            {
                                //Get tangent equivalent
                                vertexType = VertexData.GetTangentEquivalent(vertexType);

                                //Compute tangents
                                geometry.ComputeTangents();
                            }
                        }
                    }

                    IVertexData[] vertexList = VertexData.Convert(
                        vertexType,
                        geometry.Vertices,
                        weights,
                        jointNames,
                        bindShapeMatrix);

                    Mesh nMesh = null;

                    if (instanced)
                    {
                        nMesh = new MeshInstanced(
                            geometry.Material,
                            geometry.Topology,
                            vertexList,
                            geometry.Indices,
                            instances,
                            true);
                    }
                    else
                    {
                        nMesh = new Mesh(
                            geometry.Material,
                            geometry.Topology,
                            vertexList,
                            geometry.Indices,
                            true);
                    }

                    this.Meshes.Add(meshName, geometry.Material, nMesh);
                }
            }
        }
        /// <summary>
        /// Initialize skinned data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="skinList">Skins</param>
        protected virtual void InitializeSkinnedData(ModelContent modelContent)
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

                this.SkinningData = SkinningData.Create(
                    modelContent.SkinningInfo.Skeleton.JointIndices,
                    animations,
                    skinInfo);
            }
        }
        /// <summary>
        /// Initialize mesh buffers in the graphics device
        /// </summary>
        protected virtual void InitializeMeshes()
        {
            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    mesh.Initialize(this.Device);
                }
            }
        }

        /// <summary>
        /// Update model
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="context">Context</param>
        public override void Update(GameTime gameTime, Context context)
        {
            if (this.SkinningData != null)
            {
                this.SkinningData.Update(gameTime);
            }
        }
        /// <summary>
        /// Dispose model buffers
        /// </summary>
        public override void Dispose()
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
        }
        /// <summary>
        /// Sets clip to play
        /// </summary>
        /// <param name="clipName">Clip name</param>
        public void SetClip(string clipName)
        {
            if (this.SkinningData != null)
            {
                this.SkinningData.SetClip(clipName);
            }
        }
        /// <summary>
        /// Sets clip velocity
        /// </summary>
        /// <param name="velocity">Velocity</param>
        public void SetAnimationVelocity(float velocity)
        {
            if (this.SkinningData != null)
            {
                this.SkinningData.AnimationVelocity = velocity;
            }
        }

        /// <summary>
        /// Gets the transformed points
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <returns>Returns the transformed points</returns>
        public virtual Vector3[] GetPoints(Matrix transform)
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

            Vector3[] trnPoints = new Vector3[points.Count];
            Vector3.TransformCoordinate(points.ToArray(), ref transform, trnPoints);

            return trnPoints;
        }
        /// <summary>
        /// Gets the transformed triangles
        /// </summary>
        /// <param name="transform">Transform to apply</param>
        /// <returns>Returns the transformed triangles</returns>
        public virtual Triangle[] GetTriangles(Matrix transform)
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

            return Triangle.Transform(triangles.ToArray(), transform);
        }

        /// <summary>
        /// Gets animation state in specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns animation state</returns>
        public virtual string GetState(float time)
        {
            if (this.SkinningData != null)
            {
                return this.SkinningData.GetState(time);
            }
            else
            {
                return "Static";
            }
        }
    }
}
