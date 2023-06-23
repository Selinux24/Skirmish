using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Content
{
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
        public Dictionary<string, LightContent> Lights { get; private set; } = new Dictionary<string, LightContent>();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        public Dictionary<string, IImageContent> Images { get; private set; } = new Dictionary<string, IImageContent>();
        /// <summary>
        /// Material dictionary
        /// </summary>
        public Dictionary<string, IMaterialContent> Materials { get; private set; } = new Dictionary<string, IMaterialContent>();
        /// <summary>
        /// Geometry dictionary
        /// </summary>
        public Dictionary<string, Dictionary<string, SubMeshContent>> Geometry { get; private set; } = new Dictionary<string, Dictionary<string, SubMeshContent>>();
        /// <summary>
        /// Controller dictionary
        /// </summary>
        public Dictionary<string, ControllerContent> Controllers { get; private set; } = new Dictionary<string, ControllerContent>();
        /// <summary>
        /// Animation definition
        /// </summary>
        public AnimationFile AnimationDefinition { get; set; }
        /// <summary>
        /// Animation dictionary
        /// </summary>
        public Dictionary<string, IEnumerable<AnimationContent>> Animations { get; set; } = new Dictionary<string, IEnumerable<AnimationContent>>();
        /// <summary>
        /// Skinning information
        /// </summary>
        public Dictionary<string, SkinningContent> SkinningInfo { get; set; } = new Dictionary<string, SkinningContent>();

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
            return string.Format("_image_{0}_", Images.Count + 1);
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
                Geometry.Add(meshName, new Dictionary<string, SubMeshContent>());
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
                    throw new EngineException($"{materialName} already exists for {meshName}");
                }

                matDict.Add(materialName, meshContent);
            }
        }
        /// <summary>
        /// Skin name list
        /// </summary>
        /// <returns>Returns the skin name list</returns>
        public IEnumerable<string> GetControllerSkins()
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
        public ControllerContent GetControllerForMesh(string meshName)
        {
            foreach (ControllerContent controller in Controllers.Values)
            {
                if (controller.Skin == meshName) return controller;
            }

            return null;
        }
        /// <summary>
        /// Gets whether the specified joint has skinning data attached or not
        /// </summary>
        /// <param name="jointName">Joint name</param>
        public bool SkinHasJointData(string jointName)
        {
            return SkinningInfo.Values.Any(value => value.Skeleton.GetJointNames().Any(j => j == jointName));
        }
        /// <summary>
        /// Gets the animation list for the specified skin content
        /// </summary>
        /// <param name="skInfo">Skin content</param>
        /// <returns>Returns the list of animations for the specified skin content</returns>
        public IEnumerable<string> GetAnimationsForSkin(SkinningContent skInfo)
        {
            List<string> result = new();

            var jointNames = skInfo.Skeleton.GetJointNames();

            foreach (var animation in Animations)
            {
                if (result.Contains(animation.Key))
                {
                    continue;
                }

                if (animation.Value.Any(a => jointNames.Any(j => j == a.JointName)))
                {
                    result.Add(animation.Key);
                }
            }

            return result.ToArray();
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
            List<SubMeshContent> staticM = new();

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

            if (lights.Any())
            {
                foreach (var l in lights)
                {
                    if (res.Lights.ContainsKey(l.Key))
                    {
                        continue;
                    }

                    res.Lights.Add(l.Key, l.Value);
                }
            }
        }

        /// <summary>
        /// Marks hull flag for all the geometry contained into the model
        /// </summary>
        /// <param name="isHull">Hull flag value</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetHullMark(bool isHull)
        {
            int count = 0;

            if (Geometry.Count > 0)
            {
                foreach (var g in Geometry)
                {
                    foreach (var s in g.Value.Values)
                    {
                        s.IsHull = isHull;
                        count++;
                    }
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
            int count = 0;

            var geo = Geometry.Where(g =>
                g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                g.Key.EndsWith("-mesh", StringComparison.OrdinalIgnoreCase));

            if (geo.Any())
            {
                foreach (var g in geo)
                {
                    foreach (var s in g.Value.Values)
                    {
                        s.IsHull = isHull;
                        count++;
                    }
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
}
