using SharpDX;
using System;
using System.Collections.Generic;
using SharpDX.DXGI;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using Buffer = SharpDX.Direct3D11.Buffer;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using Device = SharpDX.Direct3D11.Device;
using Effect = SharpDX.Direct3D11.Effect;
using SharpDX.D3DCompiler;

namespace Engine.Common
{
    using Engine.Animation;
    using Engine.Content;
    using Engine.Helpers;

    /// <summary>
    /// Mesh data
    /// </summary>
    public class DrawingData : IDisposable
    {
        protected bool DynamicBuffers = false;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; protected set; }
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
        /// Model initialization
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="name">Name</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Data description</param>
        /// <returns>Returns the generated drawing data objects</returns>
        public static DrawingData Build(Game game, string name, BufferManager bufferManager, ModelContent modelContent, DrawingDataDescription description)
        {
            DrawingData res = new DrawingData();

            res.Name = name;

            res.DynamicBuffers = description.DynamicBuffers;

            //Animation
            if (description.LoadAnimation)
            {
                InitializeSkinningData(ref res, game, modelContent);
            }

            //Images
            InitializeTextures(ref res, game, modelContent, description.TextureCount);

            //Materials
            InitializeMaterials(ref res, game, modelContent);

            //Skins & Meshes
            InitializeGeometry(ref res, game, modelContent, description);

            //Update meshes into device
            InitializeMeshes(ref res, bufferManager);

            //Lights
            InitializeLights(ref res, game, modelContent);

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
                    ImageContent info = modelContent.Images[images];

                    ShaderResourceView view = game.ResourceManager.CreateResource(info);
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
        /// <param name="game">Game</param>
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
        /// <param name="drw">Drawing data</param>
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        /// <param name="description">Description</param>
        private static void InitializeGeometry(ref DrawingData drw, Game game, ModelContent modelContent, DrawingDataDescription description)
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
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeSkinningData(ref DrawingData drw, Game game, ModelContent modelContent)
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
        private static void InitializeMeshes(ref DrawingData drw, BufferManager bufferManager)
        {
            foreach (MeshMaterialsDictionary dictionary in drw.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    bufferManager.Add(mesh);
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
        /// <param name="game">Game</param>
        /// <param name="modelContent">Model content</param>
        private static void InitializeLights(ref DrawingData drw, Game game, ModelContent modelContent)
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


        public static void UpdateOffsets(ref DrawingData drw, BufferManager bufferManager, int instances)
        {
            foreach (MeshMaterialsDictionary dictionary in drw.Meshes.Values)
            {
                foreach (Mesh mesh in dictionary.Values)
                {
                    bufferManager.Update(mesh, instances);
                }
            }
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
            Helper.Dispose(this.Meshes);
            this.Meshes = null;

            Helper.Dispose(this.Materials);
            this.Materials = null;

            this.Textures.Clear();
            this.Textures = null;

            this.SkinningData = null;
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


    public class BufferManager : IDisposable
    {
        /// <summary>
        /// Vertex buffer
        /// </summary>
        protected Buffer[] VertexBuffers = null;
        /// <summary>
        /// Index buffer
        /// </summary>
        protected Buffer IndexBuffer = null;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        protected VertexBufferBinding[] VertexBufferBindings = null;
        /// <summary>
        /// Input elements
        /// </summary>
        protected InputElement[][] InputElements = null;

        private Dictionary<VertexTypes, List<BufferData>> dict = new Dictionary<VertexTypes, List<BufferData>>();
        private Dictionary<VertexTypes, List<IVertexData>> vList = new Dictionary<VertexTypes, List<IVertexData>>();
        private List<uint> indices = new List<uint>();
        private Dictionary<Mesh, int> vDict = new Dictionary<Mesh, int>();
        private Dictionary<Mesh, int> iDict = new Dictionary<Mesh, int>();

        public string Name;

        /// <summary>
        /// Free resources from memory
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.VertexBuffers);
            this.VertexBuffers = null;

            Helper.Dispose(this.IndexBuffer);
            this.IndexBuffer = null;
        }

        public void Add(Mesh mesh)
        {
            if (mesh.Vertices != null && mesh.Vertices.Length > 0)
            {
                if (!vList.ContainsKey(mesh.VertextType))
                {
                    vList.Add(mesh.VertextType, new List<IVertexData>());
                }

                var vertices = vList[mesh.VertextType];

                vDict.Add(mesh, vertices.Count);

                vertices.AddRange(mesh.Vertices);
                mesh.VertexBufferStride = mesh.Vertices[0].GetStride();
                mesh.VertexCount = mesh.Vertices.Length;
            }

            if (mesh.Indices != null && mesh.Indices.Length > 0)
            {
                iDict.Add(mesh, indices.Count);

                indices.AddRange(mesh.Indices);
                mesh.IndexCount = mesh.Indices.Length;
            }
        }

        public void Update(Mesh mesh, int instances)
        {
            var vTypes = vList.Keys.ToArray();

            if (vDict.ContainsKey(mesh))
            {
                mesh.VertexBufferOffset = vDict[mesh];
                mesh.BufferSlot = Array.IndexOf(vTypes, mesh.VertextType);
            }

            if (iDict.ContainsKey(mesh))
            {
                mesh.IndexBufferOffset = iDict[mesh];
            }

            if (instances > 0)
            {
                mesh.InstancingBufferOffset = vList.Keys.Count + (instances > 0 ? 1 : 0) - 1;
            }
        }

        public void CreateBuffers(Device device, string name, bool dynamic, int instances)
        {
            this.Name = name;

            this.VertexBuffers = new Buffer[vList.Keys.Count + (instances > 0 ? 1 : 0)];
            this.VertexBufferBindings = new VertexBufferBinding[vList.Keys.Count + (instances > 0 ? 1 : 0)];
            this.InputElements = new InputElement[vList.Keys.Count][];

            var vTypes = vList.Keys.ToArray();

            for (int i = 0; i < vTypes.Length; i++)
            {
                var verts = vList[vTypes[i]].ToArray();

                this.VertexBuffers[i] = device.CreateVertexBuffer(name, verts, dynamic);
                this.VertexBufferBindings[i] = new VertexBufferBinding(this.VertexBuffers[i], verts[0].GetStride(), i);
                this.InputElements[i] = verts[0].GetInput(i);
            }

            if (instances > 0)
            {
                int instancingBufferOffset = this.VertexBuffers.Length - 1;

                var instancingData = new VertexInstancingData[instances];
                this.VertexBuffers[instancingBufferOffset] = device.CreateVertexBufferWrite(name, instancingData);
                this.VertexBufferBindings[instancingBufferOffset] = new VertexBufferBinding(this.VertexBuffers[instancingBufferOffset], instancingData[0].GetStride(), instancingBufferOffset);
            }

            if (indices.Count > 0)
            {
                this.IndexBuffer = device.CreateIndexBuffer(name, indices.ToArray(), dynamic);
            }
        }

        /// <summary>
        /// Adds binding to precached buffer bindings for input assembler
        /// </summary>
        /// <param name="binding">Binding to add</param>
        public void AddVertexBufferBinding(VertexBufferBinding binding)
        {
            List<VertexBufferBinding> bindings = new List<VertexBufferBinding>();

            foreach (var bnd in this.VertexBufferBindings)
            {
                bindings.Add(bnd);
                binding.Offset = bnd.Offset;
                bindings.Add(binding);
            }

            this.VertexBufferBindings = bindings.ToArray();
        }
        /// <summary>
        /// Sets input layout to assembler
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        public virtual void SetInputAssembler(DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.SetVertexBuffers(0, this.VertexBufferBindings);
            Counters.IAVertexBuffersSets++;
            deviceContext.InputAssembler.SetIndexBuffer(this.IndexBuffer, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
        }

        /// <summary>
        /// Writes instancing data
        /// </summary>
        /// <param name="deviceContext">Immediate context</param>
        /// <param name="data">Instancig data</param>
        public virtual void WriteInstancingData(DeviceContext deviceContext, VertexInstancingData[] data)
        {
            if (data != null && data.Length > 0)
            {
                var instanceCount = data.Length;
                var instancingBuffer = this.VertexBuffers[this.VertexBuffers.Length - 1];

                deviceContext.WriteDiscardBuffer(instancingBuffer, data);
            }
        }
    }

    public class BufferData
    {

    }
}
