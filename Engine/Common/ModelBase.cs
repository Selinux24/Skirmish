using System;
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

                this[meshName].Add(materialName, mesh);
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
        /// Datos de animación
        /// </summary>
        protected SkinningData SkinningData = null;
        /// <summary>
        /// Gets triangle list
        /// </summary>
        public Triangle[] Triangles { get; protected set; }
        /// <summary>
        /// Gets static bounding box
        /// </summary>
        public BoundingBox BoundingBox { get; protected set; }
        /// <summary>
        /// Gets static bounding sphere
        /// </summary>
        public BoundingSphere BoundingSphere { get; protected set; }
        /// <summary>
        /// Gets static oriented bounding box
        /// </summary>
        public OrientedBoundingBox OrientedBoundingBox { get; protected set; }

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
        /// <param name="content">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        public ModelBase(Game game, Scene3D scene, ModelContent content, bool instanced = false, int instances = 0)
            : base(game, scene)
        {
            this.Initialize(content, instanced, instances);
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

                animations.Add(SkinningData.DefaultClip, clip);

                this.SkinningData = SkinningData.Create(
                    skins,
                    hierarchy.ToArray(),
                    offsets.ToArray(),
                    animations);
            }
        }
        /// <summary>
        /// Updates model static volumes using per vertex transform
        /// </summary>
        /// <param name="transform">Per vertex transform</param>
        public virtual void ComputeVolumes(Matrix transform)
        {
            List<Triangle> cache = new List<Triangle>();
            BoundingBox bbox = new BoundingBox();
            BoundingSphere bsph = new BoundingSphere();
            OrientedBoundingBox obb = new OrientedBoundingBox(Vector3.Zero, Vector3.Zero);
            obb.Transform(transform);

            foreach (string meshName in this.Meshes.Keys)
            {
                foreach (Mesh mesh in this.Meshes[meshName].Values)
                {
                    mesh.ComputeVolumes(transform);

                    if (mesh.Triangles != null && mesh.Triangles.Length > 0)
                    {
                        cache.AddRange(mesh.Triangles);
                    }

                    bbox = BoundingBox.Merge(bbox, mesh.BoundingBox);
                    bsph = BoundingSphere.Merge(bsph, mesh.BoundingSphere);

                    OrientedBoundingBox meshObb = mesh.OrientedBoundingBox;
                    OrientedBoundingBox.Merge(ref obb, ref meshObb);
                }
            }

            this.Triangles = cache.ToArray();
            this.BoundingBox = bbox;
            this.BoundingSphere = bsph;
            this.OrientedBoundingBox = obb;
        }
        /// <summary>
        /// Get oriented bounding boxes collection
        /// </summary>
        /// <returns>Returns oriented bounding boxes list</returns>
        public virtual BoundingBox[] GetBoundingBoxes()
        {
            List<BoundingBox> bboxList = new List<BoundingBox>();

            foreach (string meshName in this.Meshes.Keys)
            {
                foreach (Mesh mesh in this.Meshes[meshName].Values)
                {
                    bboxList.Add(mesh.BoundingBox);
                }
            }

            return bboxList.ToArray();
        }

        /// <summary>
        /// Update model
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public override void Update(GameTime gameTime)
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
        /// Gets picking position of giving ray
        /// </summary>
        /// <param name="ray">Picking ray</param>
        /// <param name="position">Ground position if exists</param>
        /// <param name="triangle">Triangle found</param>
        /// <returns>Returns true if ground position found</returns>
        public virtual bool Pick(Ray ray, out Vector3 position, out Triangle triangle)
        {
            position = new Vector3();
            triangle = new Triangle();

            bool found = false;

            if (this.BoundingSphere.Intersects(ref ray) || this.BoundingBox.Intersects(ref ray))
            {
                float distance = float.MaxValue;

                foreach (string meshName in this.Meshes.Keys)
                {
                    foreach (Mesh mesh in this.Meshes[meshName].Values)
                    {
                        Vector3 p;
                        Triangle t;
                        if (mesh.Pick(ray, out p, out t))
                        {
                            found = true;

                            float currentDistance = Vector3.Distance(ray.Position, p);

                            if (currentDistance < distance)
                            {
                                position = p;
                                triangle = t;

                                distance = currentDistance;
                            }
                        }
                    }
                }
            }

            return found;
        }
    }
}
