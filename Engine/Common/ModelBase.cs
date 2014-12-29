﻿using System;
using System.Collections.Generic;
using SharpDX;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Common
{
    using Engine.Content;
    using Engine.Helpers;

    public abstract class ModelBase : Drawable
    {
        #region Classes

        protected class MeshDictionary : Dictionary<string, MeshMaterialsDictionary>
        {
            public void Add(string meshName, string materialName, Mesh mesh)
            {
                if (!this.ContainsKey(meshName))
                {
                    this.Add(meshName, new MeshMaterialsDictionary());
                }

                this[meshName].Add(materialName, mesh);
            }
        }
        protected class MeshMaterialsDictionary : Dictionary<string, Mesh>
        {

        }
        protected class MaterialDictionary : Dictionary<string, MeshMaterial>
        {

        }
        protected class TextureDictionary : Dictionary<string, ShaderResourceView>
        {
            public new ShaderResourceView this[string material]
            {
                get
                {
                    if (!string.IsNullOrEmpty(material))
                    {
                        if (!base.ContainsKey(material))
                        {
                            throw new Exception(string.Format("Texture resource not found: {0}", material));
                        }

                        return base[material];
                    }

                    return null;
                }
            }
        }
        protected class TechniqueDictionary : Dictionary<Mesh, string>
        {

        }

        #endregion

        public const string StaticMesh = "static";
        public const string NoMaterial = "none";

        /// <summary>
        /// Scene
        /// </summary>
        public new Scene3D Scene
        {
            get
            {
                return base.Scene as Scene3D;
            }
            set
            {
                base.Scene = value;
            }
        }
        /// <summary>
        /// Materials dictionary
        /// </summary>
        protected MaterialDictionary Materials = new MaterialDictionary();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        protected TextureDictionary Textures = new TextureDictionary();
        /// <summary>
        /// Normal maps
        /// </summary>
        protected TextureDictionary NormalMaps = new TextureDictionary();
        /// <summary>
        /// Meshes
        /// </summary>
        protected MeshDictionary Meshes = new MeshDictionary();
        /// <summary>
        /// Techniques
        /// </summary>
        protected TechniqueDictionary Techniques = new TechniqueDictionary();
        /// <summary>
        /// Datos de animación
        /// </summary>
        protected SkinnedData SkinnedData = null;

        #region Static Helpers

        /// <summary>
        /// Perform transform flatten for skinning
        /// </summary>
        /// <param name="controller">Animation controller</param>
        /// <param name="joint">Joint to process</param>
        /// <param name="parentIndex">Parent index</param>
        /// <param name="animations">Animation dictionary</param>
        /// <param name="hierarchy">Hierarchy of bones</param>
        /// <param name="offsets">Transform offsets</param>
        /// <param name="bones">Bone list flattened</param>
        private static void FlattenTransforms(
            ControllerContent controller,
            Joint joint,
            int parentIndex,
            Dictionary<string, AnimationContent[]> animations,
            List<int> hierarchy,
            List<Matrix> offsets,
            List<BoneAnimation> bones)
        {
            hierarchy.Add(parentIndex);

            int index = offsets.Count;

            //Bind shape Matrix * Inverse shape Matrix -> Rest Position
            offsets.Add(controller.BindShapeMatrix * controller.InverseBindMatrix[index]);

            //Find keyframes for current bone
            AnimationContent[] c = FindJointKeyframes(joint.Name, animations);

            //Set bones
            Array.ForEach(c, (a) => bones.Add(new BoneAnimation() { Keyframes = a.Keyframes }));

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                //Continue with childs
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    FlattenTransforms(
                        controller,
                        joint.Childs[i],
                        index,
                        animations,
                        hierarchy,
                        offsets,
                        bones);
                }
            }
        }
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
        /// <param name="scene">Scene</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        public ModelBase(Game game, Scene3D scene, ModelContent modelContent, bool instanced = false, int instances = 0)
            : base(game, scene)
        {
            this.Initialize(modelContent, instanced, instances);
        }

        /// <summary>
        /// Model initialization
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        protected virtual void Initialize(ModelContent modelContent, bool instanced, int instances)
        {
            //Images
            this.InitializeTextures(modelContent);

            //Materials
            this.InitializeMaterials(modelContent);

            //Skins & Meshes
            this.InitializeGeometry(modelContent, instanced, instances);

            //Animation
            this.InitializeSkinnedData(modelContent);

            foreach (MeshMaterialsDictionary dictionary in this.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    mesh.Initialize(this.Device);
                }
            }
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
        /// <param name="meshes">Mesh dictionary created</param>
        /// <param name="skinList">Skins readed</param>
        protected virtual void InitializeGeometry(ModelContent modelContent, bool instanced, int instances)
        {
            foreach (string meshName in modelContent.Geometry.Keys)
            {
                Dictionary<string, SubMeshContent> dict = modelContent.Geometry[meshName];

                if (meshName == ModelContent.StaticMesh)
                {
                    foreach (string material in dict.Keys)
                    {
                        SubMeshContent geometry = dict[material];

                        IVertexData[] vertexList = VertexData.Convert(
                            geometry.VertexType, 
                            geometry.Vertices, 
                            null);

                        Mesh nMesh = null;

                        if (instanced)
                        {
                            nMesh = new MeshInstanced(
                                geometry.Material,
                                geometry.Topology,
                                vertexList,
                                geometry.Indices,
                                instances);
                        }
                        else
                        {
                            nMesh = new Mesh(
                                geometry.Material,
                                geometry.Topology,
                                vertexList,
                                geometry.Indices);
                        }

                        this.Meshes.Add(meshName, geometry.Material, nMesh);
                    }
                }
                else
                {
                    ControllerContent cInfo = modelContent.GetControllerForMesh(meshName);

                    Matrix bindShapeMatrix = cInfo.BindShapeMatrix;
                    Weight[] weights = cInfo.Weights;

                    foreach (string material in dict.Keys)
                    {
                        SubMeshContent geometry = dict[material];

                        IVertexData[] vertexList = VertexData.Convert(
                            VertexData.GetSkinnedEquivalent(geometry.VertexType), 
                            geometry.Vertices, 
                            weights);

                        Mesh nMesh = null;

                        if (instanced)
                        {
                            nMesh = new MeshInstanced(
                                geometry.Material,
                                geometry.Topology,
                                vertexList,
                                geometry.Indices,
                                instances);
                        }
                        else
                        {
                            nMesh = new Mesh(
                                geometry.Material,
                                geometry.Topology,
                                vertexList,
                                geometry.Indices);
                        }

                        this.Meshes.Add(meshName, geometry.Material, nMesh);
                    }
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
                string[] skins = modelContent.Controllers.Skins;

                ControllerContent controller = modelContent.Controllers[modelContent.SkinningInfo.Controller];

                List<int> hierarchy = new List<int>();
                List<Matrix> offsets = new List<Matrix>();
                List<BoneAnimation> bones = new List<BoneAnimation>();

                FlattenTransforms(
                    controller,
                    modelContent.SkinningInfo.Skeleton,
                    -1,
                    modelContent.Animations,
                    hierarchy,
                    offsets,
                    bones);

                AnimationClip clip = new AnimationClip
                {
                    BoneAnimations = bones.ToArray()
                };

                Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();

                animations.Add(SkinnedData.DEFAULTCLIP, clip);

                this.SkinnedData = SkinnedData.Create(
                    skins,
                    hierarchy.ToArray(),
                    offsets.ToArray(),
                    animations);
            }
        }
        /// <summary>
        /// Load effect layouts for each mesh
        /// </summary>
        /// <param name="drawer">Drawer</param>
        protected virtual void LoadEffectLayouts(Effects.Drawer drawer)
        {
            foreach (string material in this.Meshes.Keys)
            {
                MeshMaterialsDictionary dictMat = this.Meshes[material];

                foreach (string mesh in dictMat.Keys)
                {
                    Mesh m = dictMat[mesh];

                    string technique = drawer.AddInputLayout(m.VertextType);

                    this.Techniques.Add(m, technique);
                }
            }
        }

        /// <summary>
        /// Update model
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
        {
            if (this.SkinnedData != null)
            {
                this.SkinnedData.Update(gameTime);
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

            if (this.NormalMaps != null)
            {
                foreach (ShaderResourceView view in this.NormalMaps.Values)
                {
                    if (view != null)
                    {
                        view.Dispose();
                    }
                }
                this.NormalMaps.Clear();
                this.NormalMaps = null;
            }
        }
        /// <summary>
        /// Handle viewport resize
        /// </summary>
        public override void HandleResizing()
        {

        }
    }
}
