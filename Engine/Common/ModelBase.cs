using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using Device = SharpDX.Direct3D11.Device;
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
        /// Bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }
        /// <summary>
        /// Bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }
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
        /// Groups meshes by material and create buffers for each group
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="skinnedData">Skinned data</param>
        /// <param name="materials">Materials</param>
        /// <param name="meshes">Meshes</param>
        /// <param name="maxInstances">Instances</param>
        /// <returns>Returns the mesh dictionary by material</returns>
        private static MeshDictionary GroupMeshes(
            Device device,
            SkinnedData skinnedData,
            Dictionary<string, MeshMaterial> materials,
            Dictionary<string, Mesh[]> meshes,
            int maxInstances)
        {
            MeshDictionary dictionary = new MeshDictionary();

            #region Process skin data

            if (skinnedData != null)
            {
                foreach (string skin in skinnedData.Skins)
                {
                    MeshMaterialsDictionary meshSkinned = new MeshMaterialsDictionary();

                    #region By material

                    if (materials.Count > 0)
                    {
                        foreach (string material in materials.Keys)
                        {
                            Mesh[] meshArray = Array.FindAll<Mesh>(meshes[skin], m => m.Material == material);
                            if (meshArray != null && meshArray.Length > 0)
                            {
                                Mesh mesh = GroupMeshesSkinned(device, material, meshArray, maxInstances);

                                meshSkinned.Add(material, mesh);
                            }
                        }
                    }
                    else
                    {
                        Mesh mesh = GroupMeshesSkinned(device, null, meshes[skin], maxInstances);

                        meshSkinned.Add(NoMaterial, mesh);
                    }

                    #endregion

                    if (meshSkinned.Count > 0)
                    {
                        dictionary.Add(skin, meshSkinned);
                    }
                }
            }

            #endregion

            #region Process static data

            MeshMaterialsDictionary meshStatic = new MeshMaterialsDictionary();

            if (materials.Count > 0)
            {
                foreach (string material in materials.Keys)
                {
                    #region Process by material

                    List<Mesh> list = new List<Mesh>();

                    foreach (string mesh in meshes.Keys)
                    {
                        if (skinnedData == null || !Array.Exists(skinnedData.Skins, s => s == mesh))
                        {
                            Mesh[] meshArray = Array.FindAll<Mesh>(meshes[mesh], m => m.Material == material);
                            if (meshArray != null && meshArray.Length > 0)
                            {
                                list.AddRange(meshArray);
                            }
                        }
                    }

                    if (list.Count > 0)
                    {
                        Mesh gMesh = GroupMeshes(device, material, list.ToArray(), maxInstances);

                        meshStatic.Add(material, gMesh);
                    }

                    #endregion
                }
            }
            else
            {
                List<Mesh> list = new List<Mesh>();

                foreach (string mesh in meshes.Keys)
                {
                    if (skinnedData == null || !Array.Exists(skinnedData.Skins, s => s == mesh))
                    {
                        list.AddRange(meshes[mesh]);
                    }
                }

                if (list.Count > 0)
                {
                    Mesh gMesh = GroupMeshes(device, null, list.ToArray(), maxInstances);

                    meshStatic.Add(NoMaterial, gMesh);
                }
            }

            if (meshStatic.Count > 0)
            {
                dictionary.Add(StaticMesh, meshStatic);
            }

            #endregion

            return dictionary;
        }
        /// <summary>
        /// Static mesh grouping
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="material">Material name</param>
        /// <param name="meshArray">Mesh list</param>
        /// <param name="maxInstances">Maximum instances of group</param>
        /// <returns>Returns the new mesh</returns>
        private static Mesh GroupMeshes(Device device, string material, Mesh[] meshArray, int maxInstances)
        {
            Mesh mesh = null;

            if (meshArray.Length == 1)
            {
                mesh = meshArray[0];

                mesh.CreateVertexBuffer(device);
                mesh.CreateIndexBuffer(device);

                if (mesh is MeshInstanced)
                {
                    ((MeshInstanced)mesh).CreateInstancingBuffer(device);
                }
            }
            else
            {
                if (meshArray[0] is MeshInstanced)
                {
                    MeshInstanced iMesh = MeshInstanced.Merge(
                        device,
                        material,
                        Array.ConvertAll(meshArray, c => c as MeshInstanced),
                        maxInstances);

                    iMesh.CreateVertexBuffer(device);
                    iMesh.CreateIndexBuffer(device);
                    iMesh.CreateInstancingBuffer(device);

                    mesh = iMesh;
                }
                else
                {
                    mesh = Mesh.Merge(
                        device,
                        material,
                        meshArray);

                    mesh.CreateVertexBuffer(device);
                    mesh.CreateIndexBuffer(device);
                }
            }

            return mesh;
        }
        /// <summary>
        /// Skinned mesh grouping
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="material">Material name</param>
        /// <param name="meshArray">Mesh list</param>
        /// <param name="maxInstances">Maximum instances of group</param>
        /// <returns>Returns the new mesh</returns>
        private static Mesh GroupMeshesSkinned(Device device, string material, Mesh[] meshArray, int maxInstances)
        {
            Mesh mesh = null;

            if (meshArray.Length == 1)
            {
                mesh = meshArray[0];

                mesh.CreateVertexBuffer(device);
                mesh.CreateIndexBuffer(device);

                if (mesh is MeshInstanced)
                {
                    ((MeshInstanced)mesh).CreateInstancingBuffer(device);
                }
            }
            else
            {
                if (meshArray[0] is MeshInstanced)
                {
                    MeshInstanced iMesh = MeshInstanced.Merge(
                         device,
                         material,
                         Array.ConvertAll(meshArray, c => c as MeshInstanced),
                         maxInstances);

                    iMesh.CreateVertexBuffer(device);
                    iMesh.CreateIndexBuffer(device);
                    iMesh.CreateInstancingBuffer(device);

                    mesh = iMesh;
                }
                else
                {
                    mesh = Mesh.Merge(
                        device,
                        material,
                        meshArray);

                    mesh.CreateVertexBuffer(device);
                    mesh.CreateIndexBuffer(device);
                }
            }

            return mesh;
        }
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
            Dictionary<string, Mesh[]> meshes;
            string[] skinList;
            this.InitializeGeometry(modelContent, instanced, instances, out meshes, out skinList);

            //Animation
            this.InitializeSkinnedData(modelContent, skinList);

            //Material grouping
            this.Meshes = GroupMeshes(this.Game.Graphics.Device, this.SkinnedData, this.Materials, meshes, instances);
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
        protected virtual void InitializeGeometry(ModelContent modelContent, bool instanced, int instances, out Dictionary<string, Mesh[]> meshes, out string[] skinList)
        {
            meshes = new Dictionary<string, Mesh[]>();
            skinList = null;

            if (modelContent.Controllers != null)
            {
                List<string> list = new List<string>();

                foreach (string controller in modelContent.Controllers.Keys)
                {
                    ControllerContent cInfo = modelContent.Controllers[controller];

                    Matrix bindShapeMatrix = cInfo.BindShapeMatrix;
                    Weight[] weights = cInfo.Weights;
                    SubMeshContent[] geometryList = modelContent.Geometry[cInfo.Skin];

                    list.Add(cInfo.Skin);

                    List<Mesh> meshList = new List<Mesh>();

                    foreach (SubMeshContent g in geometryList)
                    {
                        VertexTypes vertexType = Vertex.GetSkinnedEquivalent(g.VertexType);

                        Mesh nMesh = ReadVertexList(
                            g.Topology,
                            vertexType,
                            g.Vertices,
                            weights,
                            g.Indices,
                            g.BoundingBox,
                            g.BoundingSphere,
                            g.Material,
                            instanced,
                            instances);

                        meshList.Add(nMesh);
                    }

                    meshes.Add(cInfo.Skin, meshList.ToArray());
                }

                skinList = list.ToArray();
            }

            foreach (string mesh in modelContent.Geometry.Keys)
            {
                if (!meshes.ContainsKey(mesh))
                {
                    SubMeshContent[] geometryList = modelContent.Geometry[mesh];

                    List<Mesh> meshList = new List<Mesh>();

                    foreach (SubMeshContent g in geometryList)
                    {
                        Mesh nMesh = ReadVertexList(
                            g.Topology,
                            g.VertexType,
                            g.Vertices,
                            null,
                            g.Indices,
                            g.BoundingBox,
                            g.BoundingSphere,
                            g.Material,
                            instanced,
                            instances);

                        meshList.Add(nMesh);
                    }

                    meshes.Add(mesh, meshList.ToArray());
                }
            }

            BoundingBox bbox = new BoundingBox();
            BoundingSphere bsphere = new BoundingSphere();

            foreach (Mesh[] mList in meshes.Values)
            {
                foreach (Mesh m in mList)
                {
                    bbox = BoundingBox.Merge(bbox, m.BoundingBox);
                    bsphere = BoundingSphere.Merge(bsphere, m.BoundingSphere);
                }
            }

            this.BoundingBox = bbox;
            this.BoundingSphere = bsphere;
        }
        /// <summary>
        /// Initialize skinned data
        /// </summary>
        /// <param name="modelContent">Model content</param>
        /// <param name="skinList">Skins</param>
        protected virtual void InitializeSkinnedData(ModelContent modelContent, string[] skinList)
        {
            if (modelContent.SkinningInfo != null)
            {
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
                    skinList,
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
        /// Compute bounding box against transformed vertices
        /// </summary>
        /// <param name="transform">Transform</param>
        protected BoundingBox ComputeBoundingBox(Matrix transform)
        {
            BoundingBox box = new BoundingBox();

            if (this.Meshes != null)
            {
                foreach (MeshMaterialsDictionary dict in this.Meshes.Values)
                {
                    foreach (Mesh mesh in dict.Values)
                    {
                        box = BoundingBox.Merge(box, mesh.ComputeBoundingBox(transform));
                    }
                }
            }

            return box;
        }
        /// <summary>
        /// Compute bounding sphere against transformed vertices
        /// </summary>
        /// <param name="transform">Transform</param>
        protected BoundingSphere ComputeBoundingSphere(Matrix transform)
        {
            BoundingSphere sphere = new BoundingSphere();

            if (this.Meshes != null)
            {
                foreach (MeshMaterialsDictionary dict in this.Meshes.Values)
                {
                    foreach (Mesh mesh in dict.Values)
                    {
                        sphere = BoundingSphere.Merge(sphere, mesh.ComputeBoundingSphere(transform));
                    }
                }
            }

            return sphere;
        }

        /// <summary>
        /// Read the vertex list creating structuring them with a vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="weights">Weights</param>
        /// <param name="indices">Indices</param>
        /// <param name="material">Material name</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Max instances</param>
        /// <returns>Returns the formatted vertex list into the mesh</returns>
        private Mesh ReadVertexList(
            PrimitiveTopology topology,
            VertexTypes vertexType,
            Vertex[] vertices,
            Weight[] weights,
            uint[] indices,
            BoundingBox bbox,
            BoundingSphere bsphere,
            string material,
            bool instanced,
            int instances)
        {
            IVertex[] vertexList = Vertex.Convert(vertexType, vertices, weights);

            if (instanced)
            {
                return MeshInstanced.Create(
                    this.Game.Graphics.Device,
                    material,
                    topology,
                    vertexList,
                    indices,
                    bbox,
                    bsphere,
                    instances);
            }
            else
            {
                return Mesh.Create(
                    this.Game.Graphics.Device,
                    material,
                    topology,
                    vertexList,
                    indices,
                    bbox,
                    bsphere);
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
