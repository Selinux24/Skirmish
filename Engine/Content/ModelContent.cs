using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;

namespace Engine.Content
{
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
        /// Images dictionary by image name
        /// </summary>
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
            /// Imports material texture data to image dictionary
            /// </summary>
            /// <param name="material">Material content</param>
            /// <remarks>Replaces texture path with assigned name</remarks>
            public void Import(ref MaterialContent material)
            {
                if (!string.IsNullOrEmpty(material.AmbientTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture(material.AmbientTexture));

                    material.AmbientTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.DiffuseTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture(material.DiffuseTexture));

                    material.DiffuseTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.EmissionTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture(material.EmissionTexture));

                    material.EmissionTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.NormalMapTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture(material.NormalMapTexture));

                    material.NormalMapTexture = imageName;
                }

                if (!string.IsNullOrEmpty(material.SpecularTexture))
                {
                    string imageName = this.NextImageName;

                    this.Add(imageName, ImageContent.Texture(material.SpecularTexture));

                    material.SpecularTexture = imageName;
                }
            }
        }
        /// <summary>
        /// Materials dictionary by material name
        /// </summary>
        public class MaterialDictionary : Dictionary<string, MaterialContent>
        {

        }
        /// <summary>
        /// Geometry dictionary by mesh name
        /// </summary>
        /// <remarks>Each mesh has one or more submeshes in a dictionary by material name</remarks>
        public class GeometryDictionary : Dictionary<string, Dictionary<string, SubMeshContent>>
        {
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
        public class AnimationDictionary : Dictionary<string, AnimationContent[]>
        {

        }

        #endregion

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
        public static ModelContent Generate(PrimitiveTopology topology, VertexTypes vertexType, VertexData[] vertices, MaterialContent material)
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
        public static ModelContent Generate(PrimitiveTopology topology, VertexTypes vertexType, VertexData[] vertices, uint[] indices, MaterialContent material)
        {
            ModelContent modelContent = new ModelContent();

            modelContent.Images.Import(ref material);

            modelContent.Materials.Add(ModelContent.DefaultMaterial, material);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = topology,
                VertexType = vertexType,
                Vertices = vertices,
                Indices = indices,
                Material = ModelContent.DefaultMaterial,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, ModelContent.DefaultMaterial, geo);
            modelContent.Optimize();

            return modelContent;
        }
        /// <summary>
        /// Generate model content for line list
        /// </summary>
        /// <param name="lines">Lines</param>
        /// <param name="color">Color</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateLineList(Line[] lines, Color4 color)
        {
            ModelContent modelContent = new ModelContent();

            VertexData[] verts = null;
            VertexData.CreateLineList(lines, color, out verts);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.LineList,
                VertexType = VertexTypes.PositionColor,
                Vertices = verts,
                Material = ModelContent.NoMaterial,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, ModelContent.NoMaterial, geo);
            modelContent.Optimize();

            return modelContent;
        }
        /// <summary>
        /// Generate model content for triangle list
        /// </summary>
        /// <param name="triangles">Triangles</param>
        /// <param name="color">Color</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateTriangleList(Triangle[] triangles, Color4 color)
        {
            ModelContent modelContent = new ModelContent();

            VertexData[] verts = null;
            VertexData.CreateTriangleList(triangles, color, out verts);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.LineList,
                VertexType = VertexTypes.PositionColor,
                Vertices = verts,
                Material = ModelContent.NoMaterial,
            };

            modelContent.Geometry.Add(ModelContent.StaticMesh, ModelContent.NoMaterial, geo);
            modelContent.Optimize();

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

            ImageContent image = ImageContent.Array(ContentManager.FindContent(contentFolder, textures));

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
            modelContent.Optimize();

            return modelContent;
        }
        /// <summary>
        /// Generate model content for vegetarion billboard
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="textures">Texture list</param>
        /// <param name="bbox">Bounding box</param>
        /// <param name="triList">Triangle list to populate</param>
        /// <param name="saturation">Per triangle saturation</param>
        /// <param name="minSize">Billboard min size</param>
        /// <param name="maxSize">Billboard max size</param>
        /// <param name="seed">Random seed</param>
        /// <returns>Returns new model content</returns>
        public static ModelContent GenerateVegetationBillboard(string contentFolder, BoundingBox bbox, Triangle[] triList, string[] textures, float saturation, Vector2 minSize, Vector2 maxSize, int seed = 0)
        {
            ModelContent modelContent = new ModelContent();

            Random rnd = new Random(seed);

            float maxAngle = 60f;

            List<VertexData> vertices = new List<VertexData>();

            #region Find billboard positions

            foreach (Triangle tri in triList)
            {
                float area = tri.Area;
                float inc = MathUtil.RadiansToDegrees(tri.Inclination);

                if (inc > maxAngle)
                {
                    inc = 0;
                }
                else
                {
                    inc = (inc + maxAngle) / maxAngle;
                }

                int num = (int)(area * saturation * inc);
                for (int b = 0; b < num; b++)
                {
                    //Buscar un punto en el triángulo
                    Vector3 v = rnd.NextVector3(tri.Min, tri.Max);
                    Ray ray = new Ray(new Vector3(v.X, bbox.Maximum.Y + 0.1f, v.Z), Vector3.Down);
                    Vector3 iPoint;
                    if (tri.Intersects(ref ray, out iPoint))
                    {
                        Vector2 bbsize = rnd.NextVector2(minSize, maxSize);

                        Vector3 bbpos = iPoint;
                        bbpos.Y += bbsize.Y * 0.5f;

                        vertices.Add(VertexData.CreateVertexBillboard(bbpos, bbsize));
                    }
                }
            }

            #endregion

            string imageName = "billboard";
            string materialName = "billboardMaterial";
            string geoName = "billboardGeometry";

            ImageContent imageContent = new ImageContent()
            {
                Paths = ContentManager.FindContent(contentFolder, textures),
            };

            modelContent.Images.Add(imageName, imageContent);

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            modelContent.Materials.Add(materialName, material);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.PointList,
                VertexType = VertexTypes.Billboard,
                Vertices = vertices.ToArray(),
                Indices = null,
                Material = materialName,
            };

            modelContent.Geometry.Add(geoName, materialName, geo);
            modelContent.Optimize();

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
                Paths = ContentManager.FindContent(contentFolder, texture),
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
            modelContent.Optimize();

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
    }
}
