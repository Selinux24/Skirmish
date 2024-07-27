using Engine.Animation;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using Engine.Content.Persistence;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Content
{
    /// <summary>
    /// Model content
    /// </summary>
    public class ContentData
    {
        /// <summary>
        /// Static mesh name
        /// </summary>
        public const string StaticMesh = "_base_static_mesh_";
        /// <summary>
        /// Unspecified material name
        /// </summary>
        public const string NoMaterial = "_base_material_unspecified_";
        /// <summary>
        /// Default material name
        /// </summary>
        public const string DefaultMaterial = "_base_material_default_";
        /// <summary>
        /// Mesh string
        /// </summary>
        private const string MeshString = "-mesh";

        /// <summary>
        /// Light dictionary
        /// </summary>
        private readonly Dictionary<string, LightContent> lightContent = [];
        /// <summary>
        /// Texture dictionary
        /// </summary>
        private readonly Dictionary<string, IImageContent> imageContent = [];
        /// <summary>
        /// Material dictionary
        /// </summary>
        private readonly Dictionary<string, IMaterialContent> materialContent = [];
        /// <summary>
        /// Geometry dictionary
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, SubMeshContent>> geometryContent = [];
        /// <summary>
        /// Skinning information
        /// </summary>
        private readonly Dictionary<string, SkinningContent> skinningContent = [];
        /// <summary>
        /// Controller dictionary
        /// </summary>
        private readonly Dictionary<string, ControllerContent> controllerContent = [];
        /// <summary>
        /// Animation dictionary
        /// </summary>
        private readonly Dictionary<string, IEnumerable<AnimationContent>> animationContent = [];
        /// <summary>
        /// Animation definition
        /// </summary>
        private AnimationFile animationDefinition;

        /// <summary>
        /// Content name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentData()
        {
            //Adding default material for non material geometry, like hulls
            materialContent.Add(NoMaterial, MaterialBlinnPhongContent.Default);
        }

        /// <summary>
        /// Generates a triangle list model content from scratch
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ContentData GenerateTriangleList(IEnumerable<VertexData> vertices, IMaterialContent material = null)
        {
            var materials = new[] { material ?? MaterialBlinnPhongContent.Default };

            return Generate(Topology.TriangleList, vertices, [], materials);
        }
        /// <summary>
        /// Generates a triangle list model content from scratch
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ContentData GenerateTriangleList(IEnumerable<VertexData> vertices, IEnumerable<uint> indices, IMaterialContent material = null)
        {
            var materials = new[] { material ?? MaterialBlinnPhongContent.Default };

            return Generate(Topology.TriangleList, vertices, indices, materials);
        }
        /// <summary>
        /// Generates a triangle list model content from scratch
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <param name="materials">Material list</param>
        /// <returns>Returns new model content</returns>
        public static ContentData GenerateTriangleList(IEnumerable<VertexData> vertices, IEnumerable<uint> indices, IEnumerable<IMaterialContent> materials)
        {
            return Generate(Topology.TriangleList, vertices, indices, materials);
        }
        /// <summary>
        /// Generates a triangle list model content from geometry descriptor
        /// </summary>
        /// <param name="geometry">Geometry descriptor</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ContentData GenerateTriangleList(GeometryDescriptor geometry, IMaterialContent material = null)
        {
            var vertices = VertexData.FromDescriptor(geometry);
            var materials = new[] { material ?? MaterialBlinnPhongContent.Default };

            return Generate(Topology.TriangleList, vertices, geometry.Indices, materials);
        }
        /// <summary>
        /// Generates a triangle list model content from geometry descriptor
        /// </summary>
        /// <param name="geometry">Geometry descriptor</param>
        /// <param name="materials">Material list</param>
        /// <returns>Returns new model content</returns>
        public static ContentData GenerateTriangleList(GeometryDescriptor geometry, IEnumerable<IMaterialContent> materials)
        {
            var vertices = VertexData.FromDescriptor(geometry);

            return Generate(Topology.TriangleList, vertices, geometry.Indices, materials);
        }
        /// <summary>
        /// Generates a line list model content from scratch
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ContentData GenerateLineList(IEnumerable<VertexData> vertices, IMaterialContent material = null)
        {
            var materials = new[] { material ?? MaterialBlinnPhongContent.Default };

            return Generate(Topology.LineList, vertices, [], materials);
        }
        /// <summary>
        /// Generate model content from scratch
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <param name="materials">Material list</param>
        /// <returns>Returns new model content</returns>
        private static ContentData Generate(Topology topology, IEnumerable<VertexData> vertices, IEnumerable<uint> indices, IEnumerable<IMaterialContent> materials)
        {
            ContentData modelContent = new();
            string materialName;
            bool textured;

            int materialCount = materials.Count();
            if (materialCount == 1)
            {
                var material = materials.First();
                materialName = DefaultMaterial;
                textured = material.Textured;

                modelContent.ImportTextures(ref material);
                modelContent.AddMaterialContent(materialName, material);
            }
            else if (materialCount > 1)
            {
                materialName = DefaultMaterial;
                textured = materials.First().Textured;

                for (int i = 0; i < materialCount; i++)
                {
                    string name = i == 0 ? materialName : $"{materialName}_{i}";
                    var material = materials.ElementAt(i);

                    modelContent.ImportTextures(ref material);
                    modelContent.AddMaterialContent(name, material);
                }

            }
            else
            {
                materialName = NoMaterial;
                textured = false;
            }

            SubMeshContent geo = new(topology, materialName, textured, false, Matrix.Identity);

            geo.SetVertices(vertices);
            geo.SetIndices(indices);

            modelContent.ImportMaterial(StaticMesh, materialName, geo);
            modelContent.Optimize();

            return modelContent;
        }
        /// <summary>
        /// Builds a content dictionary by level of detail
        /// </summary>
        /// <param name="geo">Model content list</param>
        /// <param name="optimize">Sets whether the content must be optimized or not</param>
        /// <returns>Returns the content dictionary by level of detail</returns>
        public static Dictionary<LevelOfDetail, ContentData> BuildLOD(IEnumerable<ContentData> geo, bool optimize)
        {
            Dictionary<LevelOfDetail, ContentData> res = [];

            int lastLod = 1;
            foreach (var iGeo in geo)
            {
                if (optimize) iGeo.Optimize();

                res.Add((LevelOfDetail)lastLod, iGeo);

                lastLod = Helper.NextPowerOfTwo(lastLod + 1);
            }

            return res;
        }

        /// <summary>
        /// Gets texture content
        /// </summary>
        public IEnumerable<(string Name, IImageContent Content)> GetTextureContent()
        {
            if (imageContent.Count == 0)
            {
                yield break;
            }

            foreach (var images in imageContent)
            {
                yield return new(images.Key, images.Value);
            }
        }
        /// <summary>
        /// Gets texture content by name
        /// </summary>
        /// <param name="name">Name</param>
        public IImageContent GetTextureContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!imageContent.TryGetValue(name, out var image))
            {
                return null;
            }

            return image;
        }
        /// <summary>
        /// Gets whether the content data contais the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public bool ContainsTextureContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return imageContent.ContainsKey(name);
        }
        /// <summary>
        /// Adds a new texture content
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="content">Content</param>
        public void AddTextureContent(string name, IImageContent content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!imageContent.TryAdd(name, content))
            {
                imageContent[name] = content;
            }
        }
        /// <summary>
        /// Adds a new texture content list
        /// </summary>
        /// <param name="content">Content list</param>
        public void AddTextureContent(Dictionary<string, IImageContent> content)
        {
            foreach (var texture in content)
            {
                AddTextureContent(texture.Key, texture.Value);
            }
        }

        /// <summary>
        /// Gets material content
        /// </summary>
        public IEnumerable<(string Name, IMaterialContent Content)> GetMaterialContent()
        {
            if (materialContent.Count == 0)
            {
                yield break;
            }

            foreach (var mat in materialContent)
            {
                yield return (mat.Key, mat.Value);
            }
        }
        /// <summary>
        /// Gets material content by name
        /// </summary>
        /// <param name="name">Name</param>
        public IMaterialContent GetMaterialContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!materialContent.TryGetValue(name, out var material))
            {
                return null;
            }

            return material;
        }
        /// <summary>
        /// Gets whether the content data contais the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public bool ContainsMaterialContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return materialContent.ContainsKey(name);
        }
        /// <summary>
        /// Adds a material content to the model content
        /// </summary>
        /// <param name="name">Material name</param>
        /// <param name="content">Material content</param>
        public void AddMaterialContent(string name, IMaterialContent content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!materialContent.TryAdd(name, content))
            {
                materialContent[name] = content;
            }
        }
        /// <summary>
        /// Adds a new material content list
        /// </summary>
        /// <param name="content">Content list</param>
        public void AddMaterialContent(Dictionary<string, IMaterialContent> content)
        {
            foreach (var material in content)
            {
                AddMaterialContent(material.Key, material.Value);
            }
        }

        /// <summary>
        /// Gets geometry content
        /// </summary>
        public IEnumerable<(string Name, Dictionary<string, SubMeshContent> Content)> GetGeometryContent()
        {
            if (geometryContent.Count == 0)
            {
                yield break;
            }

            foreach (var geom in geometryContent)
            {
                yield return (geom.Key, geom.Value);
            }
        }
        /// <summary>
        /// Gets geometry content by name
        /// </summary>
        /// <param name="name">Name</param>
        public Dictionary<string, SubMeshContent> GetGeometryContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return [];
            }

            if (!geometryContent.TryGetValue(name, out var geometry))
            {
                return [];
            }

            return geometry;
        }
        /// <summary>
        /// Gets whether the content data contais the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public bool ContainsGeometryContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return geometryContent.ContainsKey(name);
        }
        /// <summary>
        /// Adds a new geometry content
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="content">Content</param>
        public void AddGeometryContent(string name, Dictionary<string, SubMeshContent> content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!geometryContent.TryAdd(name, content))
            {
                geometryContent[name] = content;
            }
        }
        /// <summary>
        /// Adds a new geometry content list
        /// </summary>
        /// <param name="content">Content list</param>
        public void AddGeometryContent(Dictionary<string, Dictionary<string, SubMeshContent>> content)
        {
            foreach (var geom in content)
            {
                AddGeometryContent(geom.Key, geom.Value);
            }
        }

        /// <summary>
        /// Adds animation content to the model content
        /// </summary>
        /// <param name="animationLib">Animation library name</param>
        /// <param name="content">Animation content</param>
        public void AddAnimationContent(string animationLib, IEnumerable<AnimationContent> content)
        {
            if (content?.Any() != true)
            {
                return;
            }

            if (skinningContent != null)
            {
                //Filter content by existing joints
                animationContent[animationLib] = content.Where(a => SkinHasJointData(a.JointName)).ToArray();
            }
            else
            {
                animationContent[animationLib] = content.ToArray();
            }
        }
        /// <summary>
        /// Adds animation content to the model content
        /// </summary>
        /// <param name="content">Animation content</param>
        public void AddAnimationContent(AnimationLibContentData content)
        {
            if (content?.Animations?.Any() != true)
            {
                return;
            }

            foreach (var animationLib in content.Animations)
            {
                foreach (var animation in animationLib)
                {
                    AddAnimationContent(animation.Key, animation.Value);
                }
            }
        }

        /// <summary>
        /// Gets controller content
        /// </summary>
        public IEnumerable<(string Name, ControllerContent Content)> GetControllerContent()
        {
            if (controllerContent.Count == 0)
            {
                yield break;
            }

            foreach (var controller in controllerContent)
            {
                yield return new(controller.Key, controller.Value);
            }
        }
        /// <summary>
        /// Gets controller content by name
        /// </summary>
        /// <param name="name">Name</param>
        public ControllerContent GetControllerContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!controllerContent.TryGetValue(name, out var controller))
            {
                return null;
            }

            return controller;
        }
        /// <summary>
        /// Gets whether the content data contais the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public bool ContainsControllerContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return controllerContent.ContainsKey(name);
        }
        /// <summary>
        /// Adds a new controller content
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="content">Content</param>
        public void AddControllerContent(string name, ControllerContent content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!controllerContent.TryAdd(name, content))
            {
                controllerContent[name] = content;
            }
        }
        /// <summary>
        /// Adds a new controller content list
        /// </summary>
        /// <param name="content">Content list</param>
        public void AddControllerContent(Dictionary<string, ControllerContent> content)
        {
            foreach (var controller in content)
            {
                AddControllerContent(controller.Key, controller.Value);
            }
        }

        /// <summary>
        /// Gets skinning content
        /// </summary>
        public IEnumerable<(string Name, SkinningContent Content)> GetSkinningContent()
        {
            if (skinningContent.Count == 0)
            {
                yield break;
            }

            foreach (var sk in skinningContent)
            {
                yield return new(sk.Key, sk.Value);
            }
        }
        /// <summary>
        /// Gets skinning content by name
        /// </summary>
        /// <param name="name">Name</param>
        public SkinningContent GetSkinningContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (!skinningContent.TryGetValue(name, out var skinning))
            {
                return null;
            }

            return skinning;
        }
        /// <summary>
        /// Gets whether the content data contais the specified name
        /// </summary>
        /// <param name="name">Name</param>
        public bool ContainsSkinningContent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return skinningContent.ContainsKey(name);
        }
        /// <summary>
        /// Adds a new skinning content
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="content">Content</param>
        public void AddSkinningContent(string name, SkinningContent content)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!skinningContent.TryAdd(name, content))
            {
                skinningContent[name] = content;
            }
        }
        /// <summary>
        /// Adds a new skinning content list
        /// </summary>
        /// <param name="content">Content list</param>
        public void AddSkinningContent(Dictionary<string, SkinningContent> content)
        {
            foreach (var skinning in content)
            {
                AddSkinningContent(skinning.Key, skinning.Value);
            }
        }

        /// <summary>
        /// Adds or replaces a light content by id
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="content">Light content</param>
        public void AddLightContent(string id, LightContent content)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            if (!lightContent.TryAdd(id, content))
            {
                lightContent[id] = content;
            }
        }
        /// <summary>
        /// Gets the light content
        /// </summary>
        /// <param name="id">Id</param>
        public LightContent GetLightContent(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if (!lightContent.TryGetValue(id, out var light))
            {
                return null;
            }

            return light;
        }

        /// <summary>
        /// Imports material texture data to image dictionary
        /// </summary>
        /// <param name="material">Material content</param>
        /// <remarks>Replaces texture path with assigned name</remarks>
        private void ImportTextures(ref IMaterialContent material)
        {
            if (material == null)
            {
                return;
            }

            material.AmbientTexture = ImportImage(material.AmbientTexture);
            material.DiffuseTexture = ImportImage(material.DiffuseTexture);
            material.EmissiveTexture = ImportImage(material.EmissiveTexture);
            material.NormalMapTexture = ImportImage(material.NormalMapTexture);
            material.SpecularTexture = ImportImage(material.SpecularTexture);
        }
        /// <summary>
        /// Gets next image name
        /// </summary>
        /// <returns>Returns next image name</returns>
        private string NextImageName()
        {
            return $"_image_{imageContent.Count + 1}_";
        }
        /// <summary>
        /// Imports a texture
        /// </summary>
        /// <param name="textureName">Texture name</param>
        /// <returns>Returns the assigned texture name in the content data instance</returns>
        private string ImportImage(string textureName)
        {
            if (string.IsNullOrEmpty(textureName))
            {
                return textureName;
            }

            var content = new FileArrayImageContent(textureName);
            var img = imageContent.Where(v => v.Value.Equals(content));
            if (!img.Any())
            {
                string imageName = NextImageName();

                imageContent.Add(imageName, content);

                return imageName;
            }

            return img.First().Key;
        }

        /// <summary>
        /// Adds a submesh to mesh by mesh and material names
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="materialName">Material name</param>
        /// <param name="meshContent">Submesh</param>
        public void ImportMaterial(string meshName, string materialName, SubMeshContent meshContent)
        {
            if (!geometryContent.TryGetValue(meshName, out var matDict))
            {
                matDict = [];
                geometryContent.Add(meshName, matDict);
            }

            if (string.IsNullOrEmpty(materialName) || materialName == NoMaterial)
            {
                matDict.TryAdd(NoMaterial, meshContent);

                return;
            }

            if (!matDict.TryAdd(materialName, meshContent))
            {
                Logger.WriteWarning(this, $"{materialName} already exists for {meshName}");

                return;
            }
        }

        /// <summary>
        /// Gets the skinning data
        /// </summary>
        public SkinningData CreateSkinningData()
        {
            if (skinningContent.Count == 0)
            {
                return null;
            }

            //Use the definition to read animation data into a clip dictionary
            var sInfo = skinningContent.Values.First();
            var jointAnimations = InitializeJoints(sInfo.Skeleton.Root, sInfo.Controllers);

            var skinningData = new SkinningData(sInfo.Skeleton);
            skinningData.Initialize(jointAnimations, animationDefinition);

            return skinningData;
        }
        /// <summary>
        /// Initialize skeleton data
        /// </summary>
        /// <param name="joint">Joint to initialize</param>
        /// <param name="skinController">Skin controller</param>
        private IEnumerable<JointAnimation> InitializeJoints(Joint joint, IEnumerable<string> skinController)
        {
            var animations = new List<JointAnimation>();

            //Find keyframes for current bone
            var boneAnimation = animationContent.Values
                .FirstOrDefault(a => a.Any(ac => ac.JointName == joint.Name))?
                .Select(a => new JointAnimation(a.JointName, a.Keyframes))
                .FirstOrDefault(); //Only one bone animation (for now)
            if (boneAnimation.HasValue)
            {
                animations.Add(boneAnimation.Value);
            }

            foreach (string controllerName in skinController)
            {
                var controller = controllerContent[controllerName];

                if (controller.InverseBindMatrix.TryGetValue(joint.Bone, out var trn))
                {
                    joint.Offset = trn;
                }
                else
                {
                    joint.Offset = Matrix.Identity;
                }
            }

            if (joint.Childs?.Length > 0)
            {
                foreach (var child in joint.Childs)
                {
                    var ja = InitializeJoints(child, skinController);

                    animations.AddRange(ja);
                }
            }

            return [.. animations];
        }
        /// <summary>
        /// Skin name list
        /// </summary>
        /// <returns>Returns the skin name list</returns>
        private string[] GetControllerSkins()
        {
            return controllerContent.Values
                .Select(item => item.Skin)
                .Distinct()
                .ToArray();
        }
        /// <summary>
        /// Get controller for mesh by mesh name
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns the controller attached to the mesh</returns>
        private ControllerContent GetControllerForMesh(string meshName)
        {
            return controllerContent.Values.FirstOrDefault(c => c.Skin == meshName);
        }
        /// <summary>
        /// Gets whether the specified joint has skinning data attached or not
        /// </summary>
        /// <param name="jointName">Joint name</param>
        private bool SkinHasJointData(string jointName)
        {
            return skinningContent.Values.Any(value => value.Skeleton.GetJointNames().Any(j => j == jointName));
        }
        /// <summary>
        /// Reads skinning data
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns the skinnging data</returns>
        private SkinningInfo? GetSkinningInfo(string meshName)
        {
            if (controllerContent.Count == 0)
            {
                return null;
            }

            if (skinningContent.Count == 0)
            {
                return null;
            }

            var cInfo = GetControllerForMesh(meshName);
            if (cInfo == null)
            {
                return null;
            }

            //Apply shape matrix if controller exists but we are not loading animation info
            var bindShapeMatrix = cInfo.BindShapeMatrix;
            var weights = cInfo.Weights;

            //Find skeleton for controller
            if (!skinningContent.TryGetValue(cInfo.Armature, out var sInfo))
            {
                return null;
            }

            var boneNames = sInfo.Skeleton.GetBoneNames();

            return new SkinningInfo
            {
                BindShapeMatrix = bindShapeMatrix,
                Weights = weights,
                BoneNames = boneNames,
            };
        }

        /// <summary>
        /// Initilize geometry
        /// </summary>
        /// <param name="loadAnimation">Load animations</param>
        /// <param name="loadNormalMaps">Load normal maps</param>
        /// <param name="constraint">Use constraint</param>
        public async Task<Dictionary<string, MeshByMaterialCollection>> CreateGeometry(bool loadAnimation, bool loadNormalMaps, BoundingBox? constraint)
        {
            if (geometryContent.Count == 0)
            {
                return [];
            }

            Dictionary<string, MeshByMaterialCollection> meshes = [];

            var meshNames = geometryContent.Keys.ToArray();

            foreach (var meshName in meshNames)
            {
                var mesh = await CreateGeometryMesh(meshName, loadAnimation, loadNormalMaps, constraint);

                meshes.Add(meshName, mesh);
            }

            return meshes;
        }
        /// <summary>
        /// Initialize geometry mesh
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="loadAnimation">Load animations</param>
        /// <param name="loadNormalMaps">Load normal maps</param>
        /// <param name="constraint">Use constraint</param>
        private async Task<MeshByMaterialCollection> CreateGeometryMesh(string meshName, bool loadAnimation, bool loadNormalMaps, BoundingBox? constraint)
        {
            //Extract meshes
            var submeshes = geometryContent[meshName]
                .Where(g => !g.Value.IsHull)
                .ToArray();
            if (submeshes.Length == 0)
            {
                return default;
            }

            var materials = GetMaterialContent();
            var skinningInfo = loadAnimation ? GetSkinningInfo(meshName) : null;
            var isSkinned = skinningInfo.HasValue;

            MeshByMaterialCollection meshes = new();

            foreach (var subMesh in submeshes)
            {
                var geometry = subMesh.Value;
                var meshMaterial = materials.FirstOrDefault(m => m.Name == subMesh.Key).Content;

                var meshInfo = await VertexTypesHelper.CreateMesh(meshName, geometry, isSkinned, loadNormalMaps, meshMaterial, constraint, skinningInfo);
                if (meshInfo == null)
                {
                    continue;
                }

                var nMesh = meshInfo.Value.Mesh;
                var materialName = meshInfo.Value.MaterialName;

                meshes.SetValue(materialName, nMesh);
            }

            return meshes;
        }

        /// <summary>
        /// Model content optimization
        /// </summary>
        public void Optimize()
        {
            if (materialContent.Count <= 1)
            {
                //One material only
                return;
            }

            if (geometryContent.Count <= 1)
            {
                //One geometry group only
                return;
            }

            if (!materialContent.Keys.Any(m => geometryContent.Values.Count(g => g.Keys.Any(k => k == m)) > 1))
            {
                //No materials with more than one geometry group
                return;
            }

            OptimizeGeometry();
        }
        /// <summary>
        /// Optimizes the geometry dictionary, grouping existing sub-meshes below the same material.
        /// </summary>
        private void OptimizeGeometry()
        {
            //Group by material name
            var group = GroupByMaterial(out var skinned);

            //Clear current geometry
            geometryContent.Clear();

            //Add skinning
            foreach (var (mesh, meshDict) in skinned)
            {
                geometryContent.Add(mesh, meshDict);
            }

            //Add grouped submeshes
            int idx = 0;
            foreach (var (mat, meshes) in group)
            {
                if (meshes.Count == 0)
                {
                    continue;
                }

                string firstMesh = null;
                List<SubMeshContent> subMeshList = [];

                foreach (var (meshName, subMesh) in meshes)
                {
                    //Group using the first mesh name
                    firstMesh ??= $"opt_{idx++}.{meshName}";
                    subMeshList.Add(subMesh);
                }

                var gmesh = subMeshList[0];
                if (subMeshList.Count > 1)
                {
                    SubMeshContent.OptimizeMeshes(subMeshList, out gmesh);
                }

                geometryContent.Add(firstMesh, new([new(mat, gmesh)]));
            }
        }
        /// <summary>
        /// Groups the sub-mesh list by material name, excluding the skinned parts
        /// </summary>
        /// <param name="skinned">Excluded skinned parts</param>
        private Dictionary<string, List<(string, SubMeshContent)>> GroupByMaterial(out Dictionary<string, Dictionary<string, SubMeshContent>> skinned)
        {
            skinned = [];

            Dictionary<string, List<(string, SubMeshContent)>> group = [];

            var skins = GetControllerSkins();

            //Group by material name
            foreach (var (mesh, subMeshDict) in geometryContent)
            {
                if (Array.Exists(skins, s => mesh == s))
                {
                    skinned.Add(mesh, subMeshDict);

                    continue;
                }

                foreach (var (mat, subMesh) in subMeshDict)
                {
                    if (!group.TryGetValue(mat, out var meshes))
                    {
                        meshes = [];
                        group.Add(mat, meshes);
                    }

                    meshes.Add((mesh, subMesh));
                }
            }

            return group;
        }

        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list</returns>
        public IEnumerable<Triangle> GetTriangles()
        {
            return geometryContent.Values.SelectMany(m => m.Values.SelectMany(s => s.GetTriangles())).ToArray();
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name
        /// </summary>
        /// <param name="geometryName">Geometry name</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ContentData Filter(string geometryName)
        {
            var geo = geometryContent.Where(g => string.Equals(g.Key, $"{geometryName}{MeshString}", StringComparison.OrdinalIgnoreCase));

            if (!geo.Any())
            {
                return null;
            }

            var res = new ContentData();

            res.AddTextureContent(imageContent);
            res.AddMaterialContent(materialContent);

            foreach (var g in geo)
            {
                res.geometryContent.Add(g.Key, g.Value);
            }

            return res;
        }

        /// <summary>
        /// Creates a new content filtering with the specified geometry names
        /// </summary>
        /// <param name="geometryNames">Geometry names</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ContentData Filter(IEnumerable<string> geometryNames)
        {
            if (geometryNames?.Any() != true)
            {
                return null;
            }

            var geo = geometryContent.Where(g => geometryNames.Any(i => string.Equals(g.Key, $"{i}{MeshString}", StringComparison.OrdinalIgnoreCase)));

            if (!geo.Any())
            {
                return null;
            }

            var res = new ContentData();

            res.AddTextureContent(imageContent);
            res.AddMaterialContent(materialContent);

            foreach (var g in geo)
            {
                res.geometryContent.Add(g.Key, g.Value);
            }

            return res;
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name mask
        /// </summary>
        /// <param name="mask">Name mask</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ContentData FilterMask(string mask)
        {
            return FilterMask([mask]);
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name mask list
        /// </summary>
        /// <param name="masks">Name mask list</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ContentData FilterMask(IEnumerable<string> masks)
        {
            if (masks?.Any() != true)
            {
                return null;
            }

            ContentData res = null;

            foreach (var mask in masks)
            {
                if (string.IsNullOrWhiteSpace(mask))
                {
                    continue;
                }

                if (FilterMaskByController(mask, ref res))
                {
                    continue;
                }

                FilterMaskByMesh(mask, ref res);
            }

            return res;
        }
        /// <summary>
        /// Creates a new content filtering with the specified armature name
        /// </summary>
        /// <param name="armatureName">Armature name</param>
        /// <param name="modelContent">Model content</param>
        /// <returns>Returns true if the new model content was created</returns>
        public bool FilterByArmature(string armatureName, out ContentData modelContent)
        {
            modelContent = null;

            if (string.IsNullOrWhiteSpace(armatureName))
            {
                return false;
            }

            if (!skinningContent.TryGetValue(armatureName, out var content))
            {
                return false;
            }

            modelContent = new();

            foreach (var controller in content.Controllers)
            {
                TryAddController(controller, ref modelContent);
            }

            var meshNames = modelContent.geometryContent.Keys.ToArray();

            foreach (var mesh in meshNames)
            {
                TryAddLights(mesh.Replace(MeshString, ""), ref modelContent);
            }

            return true;
        }
        /// <summary>
        /// Filters the geometry dictionary
        /// </summary>
        /// <param name="mask">Mask</param>
        /// <param name="res">Model content</param>
        private void FilterMaskByMesh(string mask, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(mask))
            {
                return;
            }

            var geo = geometryContent.Where(g =>
                g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                g.Key.EndsWith(MeshString, StringComparison.OrdinalIgnoreCase));

            if (geo.Any())
            {
                res ??= new ContentData();

                foreach (var g in geo)
                {
                    TryAddGeometry(g.Key, ref res);
                }

                TryAddLights(mask, ref res);
            }
        }
        /// <summary>
        /// Filters the controllers dictionary
        /// </summary>
        /// <param name="mask">Mask</param>
        /// <param name="res">Model content</param>
        private bool FilterMaskByController(string mask, ref ContentData res)
        {
            var controllers = this.controllerContent.Where(g =>
                g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                g.Key.EndsWith("-skin", StringComparison.OrdinalIgnoreCase));

            if (!controllers.Any())
            {
                return false;
            }

            res ??= new ContentData();

            foreach (var c in controllers)
            {
                //Add controller
                TryAddController(c.Key, ref res);

                //Add lights
                TryAddLights(mask, ref res);
            }

            return true;
        }
        /// <summary>
        /// Try to add the controller to the result content
        /// </summary>
        /// <param name="controllerName">Controller name</param>
        /// <param name="res">Result content</param>
        private void TryAddController(string controllerName, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                return;
            }

            if (res.skinningContent.ContainsKey(controllerName))
            {
                return;
            }

            var c = controllerContent[controllerName];

            res.controllerContent.Add(controllerName, c);

            //Add skins
            TryAddSkin(c.Armature, ref res);

            //Add meshes
            TryAddGeometry(c.Skin, ref res);
        }
        /// <summary>
        /// Try to add a skin to the result content
        /// </summary>
        /// <param name="controllerName">Controller name</param>
        /// <param name="res">Result content</param>
        private void TryAddSkin(string armatureName, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(armatureName))
            {
                return;
            }

            if (res.skinningContent.ContainsKey(armatureName))
            {
                return;
            }

            var s = skinningContent[armatureName];

            res.skinningContent.Add(armatureName, s);

            //Add animations
            var animations = this.animationContent.Where(g =>
            {
                return s.Skeleton.GetJointNames()
                    .Any(j => g.Key.StartsWith($"{j}_pose_matrix", StringComparison.OrdinalIgnoreCase));
            });

            if (!animations.Any())
            {
                return;
            }

            foreach (var a in animations)
            {
                res.animationContent.Add(a.Key, a.Value);
            }

            var clips = animationDefinition.Clips
                .Where(c => string.Equals(armatureName, c.Skeleton, StringComparison.OrdinalIgnoreCase));

            if (clips.Any())
            {
                res.animationDefinition ??= new AnimationFile();

                res.animationDefinition.Clips.AddRange(clips);
            }
        }
        /// <summary>
        /// Try to add geometry to the result content
        /// </summary>
        /// <param name="controllerName">Controller name</param>
        /// <param name="res">Result content</param>
        private void TryAddGeometry(string meshName, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(meshName))
            {
                return;
            }

            if (res.geometryContent.ContainsKey(meshName))
            {
                return;
            }

            var g = geometryContent[meshName];

            res.geometryContent.Add(meshName, g);

            foreach (var sm in g.Values)
            {
                //Add materials
                TryAddMaterial(sm.Material, ref res);
            }
        }
        /// <summary>
        /// Try to add materials to the result content
        /// </summary>
        /// <param name="controllerName">Controller name</param>
        /// <param name="res">Result content</param>
        private void TryAddMaterial(string materialName, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(materialName))
            {
                return;
            }

            if (res.materialContent.ContainsKey(materialName))
            {
                return;
            }

            var mat = materialContent[materialName];

            res.materialContent.Add(materialName, mat);

            //Add textures
            TryAddImage(mat.AmbientTexture, ref res);
            TryAddImage(mat.DiffuseTexture, ref res);
            TryAddImage(mat.EmissiveTexture, ref res);
            TryAddImage(mat.NormalMapTexture, ref res);
            TryAddImage(mat.SpecularTexture, ref res);
        }
        /// <summary>
        /// Try to add an image to the result content
        /// </summary>
        /// <param name="controllerName">Controller name</param>
        /// <param name="res">Result content</param>
        private void TryAddImage(string textureName, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(textureName))
            {
                return;
            }

            if (res.imageContent.ContainsKey(textureName))
            {
                return;
            }

            res.imageContent.Add(textureName, imageContent[textureName]);
        }
        /// <summary>
        /// Try to add the lights to the result content
        /// </summary>
        /// <param name="controllerName">Controller name</param>
        /// <param name="res">Result content</param>
        private void TryAddLights(string mask, ref ContentData res)
        {
            if (string.IsNullOrWhiteSpace(mask))
            {
                return;
            }

            var lights = this.lightContent.Where(l =>
                l.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                l.Key.EndsWith("-light", StringComparison.OrdinalIgnoreCase));

            if (!lights.Any())
            {
                return;
            }

            foreach (var l in lights)
            {
                if (res.lightContent.ContainsKey(l.Key))
                {
                    continue;
                }

                res.lightContent.Add(l.Key, l.Value);
            }
        }

        /// <summary>
        /// Gets hull meshes
        /// </summary>
        public IEnumerable<Triangle> GetHullMeshes()
        {
            var meshes = new List<Triangle>();

            var meshNames = geometryContent.Keys.ToArray();

            foreach (var meshName in meshNames)
            {
                meshes.AddRange(GetHullMesh(meshName));
            }

            return meshes;
        }
        /// <summary>
        /// Gets hull mesh by mesh name
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        private Triangle[] GetHullMesh(string meshName)
        {
            //Extract hull geometry
            return geometryContent[meshName]
                .Where(g => g.Value.IsHull)
                .SelectMany(material => material.Value.GetTriangles())
                .ToArray();
        }
        /// <summary>
        /// Marks hull flag for all the geometry contained into the model
        /// </summary>
        /// <param name="isHull">Hull flag value</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetHullMark(bool isHull)
        {
            if (geometryContent.Count <= 0)
            {
                return 0;
            }

            int count = 0;

            foreach (var g in geometryContent)
            {
                foreach (var s in g.Value.Values)
                {
                    s.IsHull = isHull;
                    count++;
                }
            }

            return count;
        }
        /// <summary>
        /// Marks hull flag for all the geometry filtered by the specified mask
        /// </summary>
        /// <param name="isHull">Hull flag value</param>
        /// <param name="mask">Asset name flag</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetHullMark(bool isHull, string mask)
        {
            return SetHullMark(isHull, [mask]);
        }
        /// <summary>
        /// Marks hull flag for all the geometry filtered by the specified mask list
        /// </summary>
        /// <param name="isHull">Hull flag value</param>
        /// <param name="masks">Asset name flag list</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetHullMark(bool isHull, IEnumerable<string> masks)
        {
            int count = 0;

            if (masks?.Any() == true)
            {
                foreach (var mask in masks)
                {
                    count += SetHullMarkGeo(mask, isHull);
                }
            }

            return count;
        }
        /// <summary>
        /// Marks the geometry dictionary
        /// </summary>
        /// <param name="mask">Mask name</param>
        /// <param name="isHull">Hull mask value</param>
        /// <returns>Returns the number of meshes setted</returns>
        private int SetHullMarkGeo(string mask, bool isHull)
        {
            var geo = geometryContent.Where(g =>
                g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                g.Key.EndsWith(MeshString, StringComparison.OrdinalIgnoreCase));

            if (!geo.Any())
            {
                return 0;
            }

            int count = 0;

            foreach (var g in geo)
            {
                foreach (var s in g.Value.Values)
                {
                    s.IsHull = isHull;
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Gets the content lights
        /// </summary>
        public IEnumerable<SceneLight> CreateLights()
        {
            foreach (var key in lightContent.Keys)
            {
                var l = lightContent[key];

                if (l.LightType == LightContentTypes.Point)
                {
                    yield return l.CreatePointLight();
                }
                else if (l.LightType == LightContentTypes.Spot)
                {
                    yield return l.CreateSpotLight();
                }
            }
        }

        /// <summary>
        /// Sets the animation definition
        /// </summary>
        /// <param name="animation">Animation definition</param>
        public void SetAnimationDefinition(AnimationFile animation)
        {
            animationDefinition = animation;
        }
    }

    /// <summary>
    /// Skinning information
    /// </summary>
    public struct SkinningInfo
    {
        /// <summary>
        /// Bind shape matrix
        /// </summary>
        public Matrix BindShapeMatrix { get; set; }
        /// <summary>
        /// Weight list
        /// </summary>
        public IEnumerable<Weight> Weights { get; set; }
        /// <summary>
        /// Bone names
        /// </summary>
        public IEnumerable<string> BoneNames { get; set; }
    }
    /// <summary>
    /// Mesh information
    /// </summary>
    public struct MeshInfo
    {
        /// <summary>
        /// Created mesh
        /// </summary>
        public Mesh Mesh { get; set; }
        /// <summary>
        /// Material name
        /// </summary>
        public string MaterialName { get; set; }
    }
}
