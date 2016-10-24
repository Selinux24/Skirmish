using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Content
{
    using Engine.Animation;
    using Engine.Common;

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
        /// Generate model content from scratch
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="vertices">Vertex list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent Generate(PrimitiveTopology topology, VertexTypes vertexType, VertexData[] vertices, MaterialContent material = null)
        {
            return Generate(topology, vertexType, vertices, null, material);
        }
        /// <summary>
        /// Generate model content from scratch
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="vertexType">Vertex type</param>
        /// <param name="vertices">Vertex list</param>
        /// <param name="indices">Index list</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent Generate(PrimitiveTopology topology, VertexTypes vertexType, VertexData[] vertices, uint[] indices, MaterialContent material = null)
        {
            ModelContent modelContent = new ModelContent();

            if (material != null)
            {
                modelContent.Images.Import(ref material);

                modelContent.Materials.Add(ModelContent.DefaultMaterial, material);
            }

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = topology,
                VertexType = vertexType,
                Vertices = vertices,
                Indices = indices,
                Material = material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial, geo);
            modelContent.Optimize();

            return modelContent;
        }
        /// <summary>
        /// Generate model content for line list
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <param name="color">Color</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateLineList(Line3D[] lines, Color4 color, MaterialContent material = null)
        {
            ModelContent modelContent = new ModelContent();

            if (material != null)
            {
                modelContent.Images.Import(ref material);

                modelContent.Materials.Add(ModelContent.DefaultMaterial, material);
            }

            VertexData[] verts = null;
            VertexData.CreateLineList(lines, color, out verts);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.LineList,
                VertexType = VertexTypes.PositionColor,
                Vertices = verts,
                Material = material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial, geo);

            return modelContent;
        }
        /// <summary>
        /// Generate model content for triangle list
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="color">Color</param>
        /// <param name="material">Material</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateTriangleList(Triangle[] triangles, Color4 color, MaterialContent material = null)
        {
            ModelContent modelContent = new ModelContent();

            if (material != null)
            {
                modelContent.Images.Import(ref material);

                modelContent.Materials.Add(ModelContent.DefaultMaterial, material);
            }

            VertexData[] verts = null;
            VertexData.CreateTriangleList(triangles, color, out verts);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.TriangleList,
                VertexType = VertexTypes.PositionColor,
                Vertices = verts,
                Material = material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, material != null ? ModelContent.DefaultMaterial : ModelContent.NoMaterial, geo);

            return modelContent;
        }
        /// <summary>
        /// Generate model content for sprite
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="textures">Texture list</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateSprite(string contentFolder, string[] textures)
        {
            return GenerateSprite(contentFolder, textures, 1, 1, 0, 0);
        }
        /// <summary>
        /// Generate model content for sprite
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="textures">Texture list</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateSprite(string contentFolder, string[] textures, float width, float height, float formWidth, float formHeight)
        {
            ModelContent modelContent = new ModelContent();

            string imageName = "spriteTexture";
            string materialName = "spriteMaterial";
            string geoName = "spriteGeometry";

            ImageContent image = ImageContent.Array(contentFolder, textures);

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            VertexData[] verts = null;
            uint[] indices = null;
            VertexData.CreateSprite(Vector2.Zero, width, height, formWidth, formHeight, out verts, out indices);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.TriangleList,
                VertexType = VertexTypes.PositionTexture,
                Vertices = verts,
                Indices = indices,
                Material = materialName,
            };

            modelContent.Images.Add(imageName, image);
            modelContent.Materials.Add(materialName, material);
            modelContent.Geometry.Add(geoName, materialName, geo);

            return modelContent;
        }
        /// <summary>
        /// Generate model content for cubemap
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="texture">Texture</param>
        /// <param name="radius">cubemap radius</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateSphere(string contentFolder, string texture, float radius)
        {
            ModelContent modelContent = new ModelContent();

            string imageName = "cubeTexture";
            string materialName = "sphereMaterial";
            string geoName = "sphereGeometry";

            ImageContent image = new ImageContent()
            {
                Streams = ContentManager.FindContent(contentFolder, texture),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            VertexData[] verts = null;
            uint[] indices = null;
            VertexData.CreateSphere(radius, 20, 20, out verts, out indices);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.TriangleList,
                VertexType = VertexTypes.Position,
                Vertices = verts,
                Indices = indices,
                Material = materialName,
            };

            modelContent.Images.Add(imageName, image);
            modelContent.Materials.Add(materialName, material);
            modelContent.Geometry.Add(geoName, materialName, geo);

            return modelContent;
        }
        /// <summary>
        /// Generate model content for sky dom
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="texture">Texture</param>
        /// <param name="radius">Sky dom radius</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateSkydom(string contentFolder, string texture, float radius)
        {
            ModelContent modelContent = new ModelContent();

            string imageName = "cubeTexture";
            string materialName = "sphereMaterial";
            string geoName = "sphereGeometry";

            ImageContent image = new ImageContent()
            {
                Streams = ContentManager.FindContent(contentFolder, texture),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            VertexData[] verts = null;
            uint[] indices = null;
            VertexData.CreateSphere(radius, 20, 20, out verts, out indices);

            indices = Helper.ChangeCoordinate(indices);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.TriangleList,
                VertexType = VertexTypes.Position,
                Vertices = verts,
                Indices = indices,
                Material = materialName,
            };

            modelContent.Images.Add(imageName, image);
            modelContent.Materials.Add(materialName, material);
            modelContent.Geometry.Add(geoName, materialName, geo);

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

            VertexData[] vertices;
            uint[] indices;
            hm.BuildGeometry(cellSize, cellHeight, out vertices, out indices);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.TriangleList,
                VertexType = VertexTypes.PositionNormalTexture,
                Material = materialName,
                Vertices = vertices,
                Indices = indices,
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

                            if (this.Geometry[skin].ContainsKey(material))
                            {
                                skinnedM.Add(this.Geometry[skin][material]);
                            }

                            if (skinnedM.Count > 0)
                            {
                                SubMeshContent gmesh;
                                if (SubMeshContent.OptimizeMeshes(skinnedM.ToArray(), out gmesh))
                                {
                                    //Mesh grouped
                                    newDict.Add(skin, material, gmesh);
                                }
                                else
                                {
                                    //Cannot group
                                    foreach (SubMeshContent m in skinnedM)
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
                            if (this.Geometry[mesh].ContainsKey(material))
                            {
                                staticM.Add(this.Geometry[mesh][material]);
                            }
                        }
                    }

                    if (staticM.Count > 0)
                    {
                        SubMeshContent gmesh;
                        if (SubMeshContent.OptimizeMeshes(staticM.ToArray(), out gmesh))
                        {
                            //Mesh grouped
                            newDict.Add(StaticMesh, material, gmesh);
                        }
                        else
                        {
                            //Cannot group
                            foreach (SubMeshContent m in staticM)
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
    }
}
