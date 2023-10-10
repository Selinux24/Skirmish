using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Content
{
    using Engine.Animation;
    using Engine.Common;
    using Engine.Content.Persistence;

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
        /// Content name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Light dictionary
        /// </summary>
        public Dictionary<string, LightContent> Lights { get; private set; } = new();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        public Dictionary<string, IImageContent> Images { get; private set; } = new();
        /// <summary>
        /// Material dictionary
        /// </summary>
        public Dictionary<string, IMaterialContent> Materials { get; private set; } = new();
        /// <summary>
        /// Geometry dictionary
        /// </summary>
        public Dictionary<string, Dictionary<string, SubMeshContent>> Geometry { get; private set; } = new();
        /// <summary>
        /// Controller dictionary
        /// </summary>
        public Dictionary<string, ControllerContent> Controllers { get; private set; } = new();
        /// <summary>
        /// Animation definition
        /// </summary>
        public AnimationFile AnimationDefinition { get; set; }
        /// <summary>
        /// Animation dictionary
        /// </summary>
        public Dictionary<string, IEnumerable<AnimationContent>> Animations { get; set; } = new();
        /// <summary>
        /// Skinning information
        /// </summary>
        public Dictionary<string, SkinningContent> SkinningInfo { get; set; } = new();

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

                modelContent.AddMaterial(materialName, material);
            }
            else if (materialCount > 1)
            {
                materialName = DefaultMaterial;
                textured = materials.First().Textured;

                for (int i = 0; i < materialCount; i++)
                {
                    string name = i == 0 ? materialName : $"{materialName}_{i}";

                    modelContent.AddMaterial(name, materials.ElementAt(i));
                }

            }
            else
            {
                materialName = NoMaterial;
                textured = false;
            }

            SubMeshContent geo = new(topology, materialName, textured, false);

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
            Dictionary<LevelOfDetail, ContentData> res = new();

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
        /// Constructor
        /// </summary>
        public ContentData()
        {
            //Adding default material for non material geometry, like hulls
            Materials.Add(NoMaterial, MaterialBlinnPhongContent.Default);
        }

        /// <summary>
        /// Gets texture content
        /// </summary>
        public IEnumerable<(string Name, IImageContent Content)> GetTextures()
        {
            foreach (var images in Images)
            {
                yield return new(images.Key, images.Value);
            }
        }

        /// <summary>
        /// Imports material texture data to image dictionary
        /// </summary>
        /// <param name="material">Material content</param>
        /// <remarks>Replaces texture path with assigned name</remarks>
        public void ImportImage(ref IMaterialContent material)
        {
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
            return $"_image_{Images.Count + 1}_";
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
            var img = Images.Where(v => v.Value.Equals(content));
            if (!img.Any())
            {
                string imageName = NextImageName();

                Images.Add(imageName, content);

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
            if (!Geometry.ContainsKey(meshName))
            {
                Geometry.Add(meshName, new());
            }

            var matDict = Geometry[meshName];

            if (string.IsNullOrEmpty(materialName) || materialName == NoMaterial)
            {
                if (!matDict.ContainsKey(NoMaterial))
                {
                    matDict.Add(NoMaterial, meshContent);
                }
            }
            else
            {
                if (matDict.ContainsKey(materialName))
                {
                    Logger.WriteWarning(this, $"{materialName} already exists for {meshName}");

                    return;
                }

                matDict.Add(materialName, meshContent);
            }
        }
        /// <summary>
        /// Gets material content
        /// </summary>
        public IEnumerable<(string Name, IMaterialContent Content)> GetMaterials()
        {
            if (Materials?.Any() != true)
            {
                yield break;
            }

            foreach (var mat in Materials)
            {
                var matName = mat.Key;
                var matContent = mat.Value;

                yield return (matName, matContent);
            }
        }

        /// <summary>
        /// Gets the skinning data
        /// </summary>
        public SkinningData GetSkinningData()
        {
            if (SkinningInfo?.Any() != true)
            {
                return null;
            }

            //Use the definition to read animation data into a clip dictionary
            var sInfo = SkinningInfo.Values.First();
            var jointAnimations = InitializeJoints(sInfo.Skeleton.Root, sInfo.Controllers);

            var skinningData = new SkinningData(sInfo.Skeleton);
            skinningData.Initialize(jointAnimations, AnimationDefinition);

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

            var boneAnimations = new List<JointAnimation>();

            //Find keyframes for current bone
            var c = Animations.Values.FirstOrDefault(a => a.Any(ac => ac.JointName == joint.Name))?.ToArray();
            if (c?.Any() == true)
            {
                //Set bones
                var ja = c.Select(a => new JointAnimation(a.JointName, a.Keyframes)).ToArray();
                boneAnimations.AddRange(ja);
            }

            if (boneAnimations.Count > 0)
            {
                //Only one bone animation (for now)
                animations.Add(boneAnimations[0]);
            }

            foreach (string controllerName in skinController)
            {
                var controller = Controllers[controllerName];

                Matrix ibm = Matrix.Identity;

                if (controller.InverseBindMatrix.ContainsKey(joint.Bone))
                {
                    ibm = controller.InverseBindMatrix[joint.Bone];
                }

                joint.Offset = ibm;
            }

            if (joint.Childs?.Length > 0)
            {
                foreach (var child in joint.Childs)
                {
                    var ja = InitializeJoints(child, skinController);

                    animations.AddRange(ja);
                }
            }

            return animations.ToArray();
        }
        /// <summary>
        /// Skin name list
        /// </summary>
        /// <returns>Returns the skin name list</returns>
        private IEnumerable<string> GetControllerSkins()
        {
            return Controllers.Values
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
            return Controllers.Values.FirstOrDefault(c => c.Skin == meshName);
        }
        /// <summary>
        /// Gets whether the specified joint has skinning data attached or not
        /// </summary>
        /// <param name="jointName">Joint name</param>
        private bool SkinHasJointData(string jointName)
        {
            return SkinningInfo.Values.Any(value => value.Skeleton.GetJointNames().Any(j => j == jointName));
        }
        /// <summary>
        /// Reads skinning data
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns the skinnging data</returns>
        private SkinningInfo? GetSkinningInfo(string meshName)
        {
            if (Controllers?.Any() != true)
            {
                return null;
            }

            if (SkinningInfo?.Any() != true)
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
            if (!SkinningInfo.ContainsKey(cInfo.Armature))
            {
                return null;
            }

            var sInfo = SkinningInfo[cInfo.Armature];
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
        public async Task<Dictionary<string, Dictionary<string, Mesh>>> GetGeometry(bool loadAnimation, bool loadNormalMaps, BoundingBox? constraint)
        {
            if (Geometry?.Any() != true)
            {
                return default;
            }

            Dictionary<string, Dictionary<string, Mesh>> meshes = new();

            foreach (var meshName in Geometry.Keys)
            {
                var mesh = await GetGeometryMesh(meshName, loadAnimation, loadNormalMaps, constraint);

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
        private async Task<Dictionary<string, Mesh>> GetGeometryMesh(string meshName, bool loadAnimation, bool loadNormalMaps, BoundingBox? constraint)
        {
            //Extract meshes
            var submeshes = Geometry[meshName]
                .Where(g => !g.Value.IsHull)
                .ToArray();
            if (!submeshes.Any())
            {
                return default;
            }

            var materials = GetMaterials();
            var skinningInfo = loadAnimation ? GetSkinningInfo(meshName) : null;
            var isSkinned = skinningInfo.HasValue;

            Dictionary<string, Mesh> meshes = new();

            foreach (var subMesh in submeshes)
            {
                var geometry = subMesh.Value;

                //Get vertex type
                var vertexType = GetVertexType(geometry.VertexType, isSkinned, loadNormalMaps, materials, subMesh.Key);

                var meshInfo = await CreateMesh(meshName, geometry, vertexType, constraint, skinningInfo);
                if (!meshInfo.Any())
                {
                    continue;
                }

                var nMesh = meshInfo.First().Mesh;
                var materialName = meshInfo.First().MaterialName;

                meshes.Add(materialName, nMesh);
            }

            return meshes;
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
        private static VertexTypes GetVertexType(VertexTypes vertexType, bool isSkinned, bool loadNormalMaps, IEnumerable<(string Name, IMaterialContent Content)> materials, string material)
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
                var meshMaterial = materials
                    .Where(m => m.Name == material)
                    .Select(m => m.Content)
                    .FirstOrDefault();

                if (meshMaterial?.NormalMapTexture != null)
                {
                    //Get tangent equivalent
                    res = VertexData.GetTangentEquivalent(res);
                }
            }

            return res;
        }
        /// <summary>
        /// Creates a mesh
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <param name="geometry">Submesh content</param>
        /// <param name="vertexType">Vertext type</param>
        /// <param name="constraint">Geometry constraint</param>
        /// <param name="skinningInfo">Skinning information</param>
        private static async Task<IEnumerable<MeshInfo>> CreateMesh(string meshName, SubMeshContent geometry, VertexTypes vertexType, BoundingBox? constraint, SkinningInfo? skinningInfo)
        {
            //Process the vertex data
            var vertexData = await geometry.ProcessVertexData(vertexType, constraint);
            var vertices = vertexData.vertices;
            var indices = vertexData.indices;

            IEnumerable<IVertexData> vertexList;
            if (skinningInfo.HasValue)
            {
                if (!skinningInfo.Value.BindShapeMatrix.IsIdentity)
                {
                    vertices = VertexData.Transform(vertices, skinningInfo.Value.BindShapeMatrix);
                }

                //Convert the vertex data to final mesh data
                vertexList = await VertexData.Convert(
                    vertexType,
                    vertices,
                    skinningInfo.Value.Weights,
                    skinningInfo.Value.BoneNames);
            }
            else
            {
                vertexList = await VertexData.Convert(
                    vertexType,
                    vertices,
                    Enumerable.Empty<Weight>(),
                    Enumerable.Empty<string>());
            }

            if (!vertexList.Any())
            {
                return Enumerable.Empty<MeshInfo>();
            }

            //Create the mesh
            var nMesh = new Mesh(
                meshName,
                geometry.Topology,
                geometry.Transform,
                vertexList,
                indices);

            //Material name
            string materialName = string.IsNullOrEmpty(geometry.Material) ? NoMaterial : geometry.Material;

            return new[] { new MeshInfo(nMesh, materialName) };
        }

        /// <summary>
        /// Gets hull meshes
        /// </summary>
        public IEnumerable<Triangle> GetHullMeshes()
        {
            var meshes = new List<Triangle>();

            foreach (var meshName in Geometry.Keys)
            {
                var submeshes = GetHullMesh(meshName);

                meshes.AddRange(submeshes);
            }

            return meshes;
        }
        /// <summary>
        /// Gets hull mesh by mesh name
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        private IEnumerable<Triangle> GetHullMesh(string meshName)
        {
            var submeshes = Geometry[meshName];

            //Extract hull geometry
            return submeshes
                .Where(g => g.Value.IsHull)
                .SelectMany(material => material.Value.GetTriangles())
                .ToArray();
        }

        /// <summary>
        /// Model content optimization
        /// </summary>
        public void Optimize()
        {
            if (Materials.Count < 1)
            {
                return;
            }

            //Copy dictionary
            var tmp = new Dictionary<string, Dictionary<string, SubMeshContent>>(Geometry);

            //Clear actual dictionary
            Geometry.Clear();

            var skins = GetControllerSkins();
            if (skins.Any())
            {
                //Skinned
                foreach (string skin in skins)
                {
                    foreach (string material in Materials.Keys)
                    {
                        OptimizeSkinnedMesh(tmp, skin, material);
                    }
                }
            }

            foreach (string material in Materials.Keys)
            {
                OptimizeStaticMesh(tmp, material);
            }
        }
        /// <summary>
        /// Optimizes the skinned mesh
        /// </summary>
        /// <param name="geometry">Current geometry dictionary</param>
        /// <param name="skin">Skin name</param>
        /// <param name="material">Material name</param>
        private void OptimizeSkinnedMesh(Dictionary<string, Dictionary<string, SubMeshContent>> geometry, string skin, string material)
        {
            var skinnedM = ComputeSubmeshContent(geometry, skin, skin, material);
            if (skinnedM != null)
            {
                OptimizeSubmeshContent(skin, material, new[] { skinnedM });
            }
        }
        /// <summary>
        /// Optimizes the static mesh
        /// </summary>
        /// <param name="geometry">Current geometry dictionary</param>
        /// <param name="material">Material name</param>
        private void OptimizeStaticMesh(Dictionary<string, Dictionary<string, SubMeshContent>> geometry, string material)
        {
            var staticM = new List<SubMeshContent>();

            foreach (string mesh in geometry.Keys)
            {
                var skins = GetControllerSkins();
                if (!skins.Any(s => s == mesh))
                {
                    var submesh = ComputeSubmeshContent(geometry, mesh, StaticMesh, material);
                    if (submesh != null)
                    {
                        staticM.Add(submesh);
                    }
                }
            }

            if (staticM.Count > 0)
            {
                OptimizeSubmeshContent(StaticMesh, material, staticM);
            }
        }
        /// <summary>
        /// Computes the specified source mesh
        /// </summary>
        /// <param name="geometry">Current geometry dictionary</param>
        /// <param name="sourceMesh">Source mesh name</param>
        /// <param name="targetMesh">Target mesh name</param>
        /// <param name="material">Material name</param>
        /// <returns>Returns a submesh content if source mesh isn't a hull</returns>
        private SubMeshContent ComputeSubmeshContent(Dictionary<string, Dictionary<string, SubMeshContent>> geometry, string sourceMesh, string targetMesh, string material)
        {
            if (!geometry.ContainsKey(sourceMesh))
            {
                return null;
            }

            var dict = geometry[sourceMesh];

            if (dict.ContainsKey(material))
            {
                if (dict[material].IsHull)
                {
                    //Group into new dictionary
                    ImportMaterial(targetMesh, material, dict[material]);
                }
                else
                {
                    //Return the submesh content
                    return dict[material];
                }
            }

            return null;
        }
        /// <summary>
        /// Optimizes the submesh content list
        /// </summary>
        /// <param name="mesh">Mesh name</param>
        /// <param name="material">Material name</param>
        /// <param name="meshList">Mesh list to optimize</param>
        private void OptimizeSubmeshContent(string mesh, string material, IEnumerable<SubMeshContent> meshList)
        {
            if (SubMeshContent.OptimizeMeshes(meshList, out var gmesh))
            {
                //Mesh grouped
                ImportMaterial(mesh, material, gmesh);
            }
            else
            {
                //Cannot group
                foreach (var m in meshList)
                {
                    ImportMaterial(mesh, material, m);
                }
            }
        }

        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list</returns>
        public IEnumerable<Triangle> GetTriangles()
        {
            return Geometry.Values.SelectMany(m => m.Values.SelectMany(s => s.GetTriangles())).ToArray();
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name
        /// </summary>
        /// <param name="geometryName">Geometry name</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ContentData Filter(string geometryName)
        {
            var geo = Geometry.Where(g => string.Equals(g.Key, geometryName + "-mesh", StringComparison.OrdinalIgnoreCase));

            if (!geo.Any())
            {
                return null;
            }

            var res = new ContentData
            {
                Images = Images,
                Materials = Materials,
            };

            foreach (var g in geo)
            {
                res.Geometry.Add(g.Key, g.Value);
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

            var geo = Geometry.Where(g => geometryNames.Any(i => string.Equals(g.Key, i + "-mesh", StringComparison.OrdinalIgnoreCase)));

            if (!geo.Any())
            {
                return null;
            }

            var res = new ContentData
            {
                Images = Images,
                Materials = Materials,
            };

            foreach (var g in geo)
            {
                res.Geometry.Add(g.Key, g.Value);
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
            return FilterMask(new[] { mask });
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

            if (!SkinningInfo.ContainsKey(armatureName))
            {
                return false;
            }

            modelContent = new ContentData();

            var controllers = SkinningInfo[armatureName].Controllers;

            foreach (var controller in controllers)
            {
                TryAddController(controller, ref modelContent);
            }

            foreach (var mesh in modelContent.Geometry.Keys)
            {
                TryAddLights(mesh.Replace("-mesh", ""), ref modelContent);
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

            var geo = Geometry.Where(g =>
                g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                g.Key.EndsWith("-mesh", StringComparison.OrdinalIgnoreCase));

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
            var controllers = Controllers.Where(g =>
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

            if (res.SkinningInfo.ContainsKey(controllerName))
            {
                return;
            }

            var c = Controllers[controllerName];

            res.Controllers.Add(controllerName, c);

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

            if (res.SkinningInfo.ContainsKey(armatureName))
            {
                return;
            }

            var s = SkinningInfo[armatureName];

            res.SkinningInfo.Add(armatureName, s);

            //Add animations
            var animations = Animations.Where(g =>
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
                res.Animations.Add(a.Key, a.Value);
            }

            var clips = AnimationDefinition.Clips
                .Where(c => string.Equals(armatureName, c.Skeleton, StringComparison.OrdinalIgnoreCase));

            if (clips.Any())
            {
                res.AnimationDefinition ??= new AnimationFile();

                res.AnimationDefinition.Clips.AddRange(clips);
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

            if (res.Geometry.ContainsKey(meshName))
            {
                return;
            }

            var g = Geometry[meshName];

            res.Geometry.Add(meshName, g);

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

            if (res.Materials.ContainsKey(materialName))
            {
                return;
            }

            var mat = Materials[materialName];

            res.Materials.Add(materialName, mat);

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

            if (res.Images.ContainsKey(textureName))
            {
                return;
            }

            res.Images.Add(textureName, Images[textureName]);
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

            var lights = Lights.Where(l =>
                l.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                l.Key.EndsWith("-light", StringComparison.OrdinalIgnoreCase));

            if (!lights.Any())
            {
                return;
            }

            foreach (var l in lights)
            {
                if (res.Lights.ContainsKey(l.Key))
                {
                    continue;
                }

                res.Lights.Add(l.Key, l.Value);
            }
        }

        /// <summary>
        /// Marks hull flag for all the geometry contained into the model
        /// </summary>
        /// <param name="isHull">Hull flag value</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetHullMark(bool isHull)
        {
            if (Geometry.Count <= 0)
            {
                return 0;
            }

            int count = 0;

            foreach (var g in Geometry)
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
            return SetHullMark(isHull, new[] { mask });
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
            var geo = Geometry.Where(g =>
                g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                g.Key.EndsWith("-mesh", StringComparison.OrdinalIgnoreCase));

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
        public IEnumerable<SceneLight> GetLights()
        {
            foreach (var key in Lights.Keys)
            {
                var l = Lights[key];

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
        /// Adds a material content to the model content
        /// </summary>
        /// <param name="name">Material name</param>
        /// <param name="material">Material content</param>
        public void AddMaterial(string name, IMaterialContent material)
        {
            ImportImage(ref material);
            Materials.Add(name, material);
        }
        /// <summary>
        /// Adds animation content to the model content
        /// </summary>
        /// <param name="animationLib">Animation library name</param>
        /// <param name="animationContent">Animation content</param>
        public void AddAnimationContent(string animationLib, IEnumerable<AnimationContent> animationContent)
        {
            if (animationContent?.Any() != true)
            {
                return;
            }

            if (SkinningInfo != null)
            {
                //Filter content by existing joints
                Animations[animationLib] = animationContent.Where(a => SkinHasJointData(a.JointName)).ToArray();
            }
            else
            {
                Animations[animationLib] = animationContent.ToArray();
            }
        }
        /// <summary>
        /// Adds animation content to the model content
        /// </summary>
        /// <param name="animationContent">Animation content</param>
        public void AddAnimationContent(AnimationLibContentData animationContent)
        {
            if (animationContent?.Animations?.Any() != true)
            {
                return;
            }

            foreach (var animationLib in animationContent.Animations)
            {
                foreach (var animation in animationLib)
                {
                    AddAnimationContent(animation.Key, animation.Value);
                }
            }
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

        /// <summary>
        /// Constructor
        /// </summary>
        public MeshInfo(Mesh mesh, string materialName)
        {
            Mesh = mesh;
            MaterialName = materialName;
        }
    }
}
