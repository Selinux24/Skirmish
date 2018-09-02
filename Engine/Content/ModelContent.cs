using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Engine.Content
{
    using Engine.Common;
    using System.Security.Permissions;

    /// <summary>
    /// Model content
    /// </summary>
    public class ModelContent
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

        #region Classes

        /// <summary>
        /// Lights dictionary by light name
        /// </summary>
        [Serializable]
        public class LightDictionary : Dictionary<string, LightContent>
        {
            /// <summary>
            /// Gets next light name
            /// </summary>
            private string NextLightName
            {
                get
                {
                    return string.Format("_light_{0}_", this.Count + 1);
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public LightDictionary()
                : base()
            {

            }
            /// <summary>
            /// Constructor de serialización
            /// </summary>
            /// <param name="info">Info</param>
            /// <param name="context">Context</param>
            protected LightDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
        }
        /// <summary>
        /// Images dictionary by image name
        /// </summary>
        [Serializable]
        public class ImageDictionary : Dictionary<string, ImageContent>
        {
            /// <summary>
            /// Gets next image name
            /// </summary>
            private string NextImageName
            {
                get
                {
                    return string.Format("_image_{0}_", this.Count + 1);
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public ImageDictionary()
                : base()
            {

            }
            /// <summary>
            /// Constructor de serialización
            /// </summary>
            /// <param name="info">Info</param>
            /// <param name="context">Context</param>
            protected ImageDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }

            /// <summary>
            /// Imports material texture data to image dictionary
            /// </summary>
            /// <param name="material">Material content</param>
            /// <remarks>Replaces texture path with assigned name</remarks>
            public void Import(ref MaterialContent material)
            {
                if (!string.IsNullOrEmpty(material.AmbientTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture("", material.AmbientTexture));

                    material.AmbientTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.DiffuseTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture("", material.DiffuseTexture));

                    material.DiffuseTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.EmissionTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture("", material.EmissionTexture));

                    material.EmissionTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.NormalMapTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture("", material.NormalMapTexture));

                    material.NormalMapTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.SpecularTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture("", material.SpecularTexture));

                    material.SpecularTexture = imageName;
                }
            }
        }
        /// <summary>
        /// Materials dictionary by material name
        /// </summary>
        [Serializable]
        public class MaterialDictionary : Dictionary<string, MaterialContent>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public MaterialDictionary()
                : base()
            {

            }
            /// <summary>
            /// Constructor de serialización
            /// </summary>
            /// <param name="info">Info</param>
            /// <param name="context">Context</param>
            protected MaterialDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
        }
        /// <summary>
        /// Geometry dictionary by mesh name
        /// </summary>
        /// <remarks>Each mesh has one or more submeshes in a dictionary by material name</remarks>
        [Serializable]
        public class GeometryDictionary : Dictionary<string, Dictionary<string, SubMeshContent>>
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public GeometryDictionary()
                : base()
            {

            }
            /// <summary>
            /// Constructor de serialización
            /// </summary>
            /// <param name="info">Info</param>
            /// <param name="context">Context</param>
            protected GeometryDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }

            /// <summary>
            /// Adds a submesh to mesh by mesh and material names
            /// </summary>
            /// <param name="meshName">Mesh name</param>
            /// <param name="materialName">Material name</param>
            /// <param name="meshContent">Submesh</param>
            public void Add(string meshName, string materialName, SubMeshContent meshContent)
            {
                if (!this.ContainsKey(meshName))
                {
                    this.Add(meshName, new Dictionary<string, SubMeshContent>());
                }

                this[meshName].Add(string.IsNullOrEmpty(materialName) ? ModelContent.NoMaterial : materialName, meshContent);
            }
            /// <summary>
            /// Gets all meshes with the specified material
            /// </summary>
            /// <param name="material">Materia name</param>
            /// <returns>Returns a submesh collection</returns>
            /// <remarks>The submesh collection returned may have differents vertex types</remarks>
            public SubMeshContent[] GetMeshesByMaterial(string material)
            {
                List<SubMeshContent> res = new List<SubMeshContent>();

                foreach (string mesh in this.Keys)
                {
                    SubMeshContent meshContent = this[mesh][material];
                    if (meshContent != null)
                    {
                        res.Add(meshContent);
                    }
                }

                return res.ToArray();
            }
        }
        /// <summary>
        /// Controller dictionary by controller name
        /// </summary>
        [Serializable]
        public class ControllerDictionary : Dictionary<string, ControllerContent>
        {
            /// <summary>
            /// Skin name list
            /// </summary>
            public string[] Skins
            {
                get
                {
                    List<string> skins = new List<string>();

                    foreach (ControllerContent controller in this.Values)
                    {
                        if (!skins.Contains(controller.Skin)) skins.Add(controller.Skin);
                    }

                    return skins.ToArray();
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public ControllerDictionary()
                : base()
            {

            }
            /// <summary>
            /// Constructor de serialización
            /// </summary>
            /// <param name="info">Info</param>
            /// <param name="context">Context</param>
            protected ControllerDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                
            }

            /// <summary>
            /// Get controller for mesh by mesh name
            /// </summary>
            /// <param name="meshName">Mesh name</param>
            /// <returns>Returns the controller attached to the mesh</returns>
            public ControllerContent GetControllerForMesh(string meshName)
            {
                foreach (ControllerContent controller in this.Values)
                {
                    if (controller.Skin == meshName) return controller;
                }

                return null;
            }
        }
        /// <summary>
        /// Animation dictionary by animation name
        /// </summary>
        [Serializable]
        public class AnimationDictionary : Dictionary<string, AnimationContent[]>
        {
            /// <summary>
            /// Animation definition
            /// </summary>
            public AnimationDescription Definition { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public AnimationDictionary()
                : base()
            {

            }
            /// <summary>
            /// Constructor de serialización
            /// </summary>
            /// <param name="info">Info</param>
            /// <param name="context">Context</param>
            protected AnimationDictionary(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                Definition = info.GetValue<AnimationDescription>("Definition");
            }
            /// <summary>
            /// Populates a SerializationInfo with the data needed to serialize the target object.
            /// </summary>
            /// <param name="info">The SerializationInfo to populate with data.</param>
            /// <param name="context">The destination for this serialization.</param>
            [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Definition", Definition);

                base.GetObjectData(info, context);
            }
        }

        #endregion

        /// <summary>
        /// Light dictionary
        /// </summary>
        public LightDictionary Lights = new LightDictionary();
        /// <summary>
        /// Texture dictionary
        /// </summary>
        public ImageDictionary Images = new ImageDictionary();
        /// <summary>
        /// Material dictionary
        /// </summary>
        public MaterialDictionary Materials = new MaterialDictionary();
        /// <summary>
        /// Geometry dictionary
        /// </summary>
        public GeometryDictionary Geometry = new GeometryDictionary();
        /// <summary>
        /// Controller dictionary
        /// </summary>
        public ControllerDictionary Controllers = new ControllerDictionary();
        /// <summary>
        /// Animation dictionary
        /// </summary>
        public AnimationDictionary Animations = new AnimationDictionary();
        /// <summary>
        /// Skinning information
        /// </summary>
        public SkinningContent SkinningInfo { get; set; }
        /// <summary>
        /// Generate triangle list model content from scratch
        /// </summary>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateTriangleList(VertexData[] vertices, uint[] indices, MaterialContent material = null)
        {
            return Generate(Topology.TriangleList, vertices, indices, material);
        }
        /// <summary>
        /// Generate model content from scratch
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        private static ModelContent Generate(Topology topology, VertexData[] vertices, uint[] indices, MaterialContent material = null)
        {
            ModelContent modelContent = new ModelContent();

            if (material != null)
            {
                modelContent.Images.Import(ref material);

                modelContent.Materials.Add(ModelContent.DefaultMaterial, material);
            }

            var materialName = material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial;
            var textured = modelContent.Materials[materialName].DiffuseTexture != null;
            var transparent = modelContent.Materials[materialName].Transparent != Color.Transparent;

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = topology,
                Vertices = vertices,
                Indices = indices,
                Material = materialName,
                Textured = textured,
                Transparent = transparent,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial, geo);
            modelContent.Optimize();

            return modelContent;
        }
        /// <summary>
        /// Generates a new model content from an height map
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="heightMap">Height map</param>
        /// <param name="textures">Texture list</param>
        /// <param name="cellSize">Cell size</param>
        /// <param name="cellHeight">Cell height</param>
        /// <returns>Returns a new model content</returns>
        public static ModelContent FromHeightmap(string contentFolder, string heightMap, string[] textures, float cellSize, float cellHeight)
        {
            ModelContent modelContent = new ModelContent();

            string texureName = "texture";
            string materialName = "material";
            string geoName = "geometry";

            ImageContent heightMapImage = new ImageContent()
            {
                Streams = ContentManager.FindContent(contentFolder, heightMap),
            };

            ImageContent textureImage = new ImageContent()
            {
                Streams = ContentManager.FindContent(contentFolder, textures),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = texureName;

            HeightMap hm = HeightMap.FromStream(heightMapImage.Stream, null);

            hm.BuildGeometry(cellSize, cellHeight, out VertexData[] vertices, out uint[] indices);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = Topology.TriangleList,
                Material = materialName,
                Vertices = vertices,
                Indices = indices,
                Textured = true,
                Transparent = false,
            };

            modelContent.Images.Add(texureName, textureImage);
            modelContent.Materials.Add(materialName, material);
            modelContent.Geometry.Add(geoName, materialName, geo);

            return modelContent;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelContent()
        {
            //Adding default material for non material geometry, like volumes
            this.Materials.Add(NoMaterial, MaterialContent.Default);
        }

        /// <summary>
        /// Model content optimization
        /// </summary>
        public void Optimize()
        {
            GeometryDictionary newDict = new GeometryDictionary();

            if (this.Materials.Count > 0)
            {
                if (this.Controllers.Skins.Length > 0)
                {
                    //Skinned
                    foreach (string skin in this.Controllers.Skins)
                    {
                        foreach (string material in this.Materials.Keys)
                        {
                            List<SubMeshContent> skinnedM = new List<SubMeshContent>();

                            var dict = this.Geometry[skin];

                            if (dict.ContainsKey(material))
                            {
                                if (dict[material].IsVolume)
                                {
                                    newDict.Add(skin, material, dict[material]);
                                }
                                else
                                {
                                    skinnedM.Add(dict[material]);
                                }
                            }

                            if (skinnedM.Count > 0)
                            {
                                if (SubMeshContent.OptimizeMeshes(skinnedM.ToArray(), out SubMeshContent gmesh))
                                {
                                    //Mesh grouped
                                    newDict.Add(skin, material, gmesh);
                                }
                                else
                                {
                                    //Cannot group
                                    foreach (var m in skinnedM)
                                    {
                                        newDict.Add(skin, material, m);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (string material in this.Materials.Keys)
                {
                    List<SubMeshContent> staticM = new List<SubMeshContent>();

                    foreach (string mesh in this.Geometry.Keys)
                    {
                        if (!Array.Exists(this.Controllers.Skins, s => s == mesh))
                        {
                            var dict = this.Geometry[mesh];

                            if (dict.ContainsKey(material))
                            {
                                if (dict[material].IsVolume)
                                {
                                    newDict.Add(StaticMesh, material, dict[material]);
                                }
                                else
                                {
                                    staticM.Add(dict[material]);
                                }
                            }
                        }
                    }

                    if (staticM.Count > 0)
                    {
                        if (SubMeshContent.OptimizeMeshes(staticM.ToArray(), out SubMeshContent gmesh))
                        {
                            //Mesh grouped
                            newDict.Add(StaticMesh, material, gmesh);
                        }
                        else
                        {
                            //Cannot group
                            foreach (var m in staticM)
                            {
                                newDict.Add(StaticMesh, material, m);
                            }
                        }
                    }
                }
            }

            this.Geometry = newDict;
        }
        /// <summary>
        /// Gets triangle list
        /// </summary>
        /// <returns>Returns triangle list</returns>
        public Triangle[] GetTriangles()
        {
            List<Triangle> triangles = new List<Triangle>();

            foreach (var meshDict in this.Geometry.Values)
            {
                foreach (var mesh in meshDict.Values)
                {
                    triangles.AddRange(mesh.GetTriangles());
                }
            }

            return triangles.ToArray();
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name
        /// </summary>
        /// <param name="geometryName">Geometry name</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ModelContent Filter(string geometryName)
        {
            var geo = this.Geometry.Where(g => string.Equals(g.Key, geometryName + "-mesh", StringComparison.OrdinalIgnoreCase));

            if (geo.Count() > 0)
            {
                var res = new ModelContent
                {
                    Images = this.Images,
                    Materials = this.Materials,
                };

                foreach (var g in geo)
                {
                    res.Geometry.Add(g.Key, g.Value);
                }

                return res;
            }

            return null;
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry names
        /// </summary>
        /// <param name="geometryNames">Geometry names</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ModelContent Filter(IEnumerable<string> geometryNames)
        {
            if (geometryNames != null && geometryNames.Count() > 0)
            {
                var geo = this.Geometry.Where(g => geometryNames.Count(i => string.Equals(g.Key, i + "-mesh", StringComparison.OrdinalIgnoreCase)) > 0);

                if (geo.Count() > 0)
                {
                    var res = new ModelContent
                    {
                        Images = this.Images,
                        Materials = this.Materials,
                    };

                    foreach (var g in geo)
                    {
                        res.Geometry.Add(g.Key, g.Value);
                    }

                    return res;
                }
            }

            return null;
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name mask
        /// </summary>
        /// <param name="mask">Name mask</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ModelContent FilterMask(string mask)
        {
            return FilterMask(new[] { mask });
        }
        /// <summary>
        /// Creates a new content filtering with the specified geometry name mask list
        /// </summary>
        /// <param name="masks">Name mask list</param>
        /// <returns>Returns a new content instance with the referenced geometry, materials, images, ...</returns>
        public ModelContent FilterMask(IEnumerable<string> masks)
        {
            ModelContent res = null;

            if (masks != null && masks.Count() > 0)
            {
                foreach (var mask in masks)
                {
                    var geo = this.Geometry.Where(g =>
                        g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                        g.Key.EndsWith("-mesh", StringComparison.OrdinalIgnoreCase));

                    if (geo.Count() > 0)
                    {
                        if (res == null)
                        {
                            res = new ModelContent();
                        }

                        res.Images = this.Images;
                        res.Materials = this.Materials;

                        foreach (var g in geo)
                        {
                            res.Geometry.Add(g.Key, g.Value);
                        }

                        var lights = this.Lights.Where(l =>
                            l.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                            l.Key.EndsWith("-light", StringComparison.OrdinalIgnoreCase));

                        if (lights.Count() > 0)
                        {
                            foreach (var l in lights)
                            {
                                res.Lights.Add(l.Key, l.Value);
                            }
                        }
                    }
                }
            }

            return res;
        }
        /// <summary>
        /// Marks volume flag for all the geometry contained into the model
        /// </summary>
        /// <param name="isVolume">Flag value</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetVolumeMark(bool isVolume)
        {
            int count = 0;

            if (this.Geometry.Count > 0)
            {
                foreach (var g in this.Geometry)
                {
                    foreach (var s in g.Value.Values)
                    {
                        s.IsVolume = isVolume;
                        count++;
                    }
                }
            }

            return count;
        }
        /// <summary>
        /// Marks volume flag for all the geometry filtered by the specified mask
        /// </summary>
        /// <param name="isVolume">Flag value</param>
        /// <param name="mask">Asset name flag</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetVolumeMark(bool isVolume, string mask)
        {
            return SetVolumeMark(isVolume, new[] { mask });
        }
        /// <summary>
        /// Marks volume flag for all the geometry filtered by the specified mask list
        /// </summary>
        /// <param name="isVolume">Flag value</param>
        /// <param name="masks">Asset name flag list</param>
        /// <returns>Returns the number of meshes setted</returns>
        public int SetVolumeMark(bool isVolume, IEnumerable<string> masks)
        {
            int count = 0;

            if (masks != null && masks.Count() > 0)
            {
                foreach (var mask in masks)
                {
                    var geo = this.Geometry.Where(g =>
                        g.Key.StartsWith(mask, StringComparison.OrdinalIgnoreCase) &&
                        g.Key.EndsWith("-mesh", StringComparison.OrdinalIgnoreCase));

                    if (geo.Count() > 0)
                    {
                        foreach (var g in geo)
                        {
                            foreach (var s in g.Value.Values)
                            {
                                s.IsVolume = isVolume;
                                count++;
                            }
                        }
                    }
                }
            }

            return count;
        }
    }
}
