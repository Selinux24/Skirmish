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

        #region Classes

        /// <summary>
        /// Images dictionary by image name
        /// </summary>
        public class ImageDictionary : Dictionary<string, ImageContent>
        {

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

                this[meshName].Add(materialName, meshContent);
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

        private static bool OptimizeMeshes(SubMeshContent[] meshArray, out SubMeshContent optimizedMesh)
        {
            if (meshArray == null || meshArray.Length == 0)
            {
                optimizedMesh = null;

                return true;
            }
            else if (meshArray.Length == 1)
            {
                optimizedMesh = meshArray[0];

                return true;
            }
            else
            {
                string material = meshArray[0].Material;
                PrimitiveTopology topology = meshArray[0].Topology;
                VertexTypes vertexType = meshArray[0].VertexType;

                List<VertexData> vertices = new List<VertexData>();
                List<uint> indices = new List<uint>();

                uint indexOffset = 0;

                foreach (SubMeshContent mesh in meshArray)
                {
                    if (mesh.VertexType != vertexType || mesh.Topology != topology)
                    {
                        optimizedMesh = null;

                        return false;
                    }

                    if (mesh.Vertices != null && mesh.Vertices.Length > 0)
                    {
                        foreach (VertexData v in mesh.Vertices)
                        {
                            vertices.Add(v);
                        }
                    }

                    if (mesh.Indices != null && mesh.Indices.Length > 0)
                    {
                        foreach (uint i in mesh.Indices)
                        {
                            indices.Add(indexOffset + i);
                        }
                    }

                    indexOffset = (uint)vertices.Count;
                }

                optimizedMesh = new SubMeshContent()
                {
                    Material = material,
                    Topology = topology,
                    VertexType = vertexType,
                    Indices = indices.ToArray(),
                    Vertices = vertices.ToArray(),
                };

                return true;
            }
        }

        public static ModelContent GenerateLineList(Line[] lines, Color color)
        {
            ModelContent modelContent = new ModelContent();

            VertexData[] verts = null;
            CreateLineList(lines, color, out verts);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.LineList,
                VertexType = VertexTypes.PositionColor,
                Vertices = verts,
                Material = ModelContent.NoMaterial,
            };

            modelContent.Materials.Add(ModelContent.NoMaterial, MaterialContent.Default);
            modelContent.Geometry.Add(ModelContent.StaticMesh, ModelContent.NoMaterial, geo);
            modelContent.Optimize();

            return modelContent;
        }
        public static ModelContent GenerateTriangleList(Triangle[] triangles, Color color)
        {
            ModelContent modelContent = new ModelContent();

            VertexData[] verts = null;
            CreateTriangleList(triangles, color, out verts);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.LineList,
                VertexType = VertexTypes.PositionColor,
                Vertices = verts,
                Material = ModelContent.NoMaterial,
            };

            modelContent.Materials.Add(ModelContent.NoMaterial, MaterialContent.Default);
            modelContent.Geometry.Add(ModelContent.StaticMesh, ModelContent.NoMaterial, geo);
            modelContent.Optimize();

            return modelContent;
        }
        public static ModelContent GenerateSprite(string contentFolder, string texture)
        {
            return GenerateSprite(contentFolder, texture, 1, 1, 0, 0);
        }
        public static ModelContent GenerateSprite(string contentFolder, string texture, float width, float height, float formWidth, float formHeight)
        {
            ModelContent modelContent = new ModelContent();

            string imageName = "spriteTexture";
            string materialName = "spriteMaterial";
            string geoName = "spriteGeometry";

            ImageContent image = new ImageContent()
            {
                Path = ContentManager.FindContent(contentFolder, texture),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            VertexData[] verts = null;
            uint[] indices = null;
            CreateSprite(Vector2.Zero, width, height, formWidth, formHeight, out verts, out indices);

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
        public static ModelContent GenerateVegetationBillboard(string contentFolder, string[] textures, Triangle[] triList, float saturation, Vector2 minSize, Vector2 maxSize, int seed = 0)
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

                int num = (int)(area * saturation * inc * 0.1f);
                for (int b = 0; b < num; b++)
                {
                    //Buscar un punto en el triángulo
                    Vector3 v = rnd.NextVector3(tri.Min, tri.Max);
                    Ray ray = new Ray(new Vector3(v.X, 1000f, v.Z), Vector3.Down);
                    Vector3? iPoint = null;
                    float? distanceToPoint = null;
                    if (Intersections.RayAndTriangle(ray, tri, out iPoint, out distanceToPoint, false))
                    {
                        Vector2 bbsize = rnd.NextVector2(minSize, maxSize);

                        Vector3 bbpos = iPoint.Value;
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
        public static ModelContent GenerateSkydom(string contentFolder, string texture, float radius)
        {
            ModelContent modelContent = new ModelContent();

            string imageName = "cubeTexture";
            string materialName = "sphereMaterial";
            string geoName = "sphereGeometry";

            ImageContent image = new ImageContent()
            {
                Path = ContentManager.FindContent(contentFolder, texture),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            VertexData[] verts = null;
            uint[] indices = null;
            CreateSphere(radius, 20, 20, out verts, out indices);

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

        public static void CreateLineList(Line[] lines, Color color, out VertexData[] v)
        {
            List<VertexData> data = new List<VertexData>();

            for (int i = 0; i < lines.Length; i++)
            {
                data.Add(VertexData.CreateVertexPositionColor(lines[i].Point1, color));
                data.Add(VertexData.CreateVertexPositionColor(lines[i].Point2, color));
            }

            v = data.ToArray();
        }
        public static void CreateTriangleList(Triangle[] triangles, Color color, out VertexData[] v)
        {
            List<VertexData> vList = new List<VertexData>();

            for (int i = 0; i < triangles.Length; i++)
            {
                vList.Add(VertexData.CreateVertexPositionColor(triangles[i].Point1, color));
                vList.Add(VertexData.CreateVertexPositionColor(triangles[i].Point2, color));
                vList.Add(VertexData.CreateVertexPositionColor(triangles[i].Point3, color));
            }

            v = vList.ToArray();
        }
        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, out VertexData[] v, out uint[] i)
        {
            v = new VertexData[4];
            i = new uint[6];

            float left = (formWidth * 0.5f * -1f) + position.X;
            float right = left + width;
            float top = (formHeight * 0.5f) - position.Y;
            float bottom = top - height;

            v[0].Position = new Vector3(left, top, 0.0f);
            v[0].Normal = Vector3.UnitZ;
            v[0].Texture = Vector2.Zero;

            v[1].Position = new Vector3(right, bottom, 0.0f);
            v[1].Normal = Vector3.UnitZ;
            v[1].Texture = Vector2.One;

            v[2].Position = new Vector3(left, bottom, 0.0f);
            v[2].Normal = Vector3.UnitZ;
            v[2].Texture = Vector2.UnitY;

            v[3].Position = new Vector3(right, top, 0.0f);
            v[3].Normal = Vector3.UnitZ;
            v[3].Texture = Vector2.UnitX;

            i[0] = 0;
            i[1] = 1;
            i[2] = 2;

            i[3] = 0;
            i[4] = 3;
            i[5] = 1;
        }
        public static void CreateBox(float width, float height, float depth, out VertexData[] v, out uint[] i)
        {
            v = new VertexData[24];
            i = new uint[36];

            float w2 = 0.5f * width;
            float h2 = 0.5f * height;
            float d2 = 0.5f * depth;

            // Fill in the front face vertex data.
            v[0] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, -d2));
            v[1] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[2] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, -d2));
            v[3] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, -d2));

            // Fill in the back face vertex data.
            v[4] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, +d2));
            v[5] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, +d2));
            v[6] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[7] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, +d2));

            // Fill in the top face vertex data.
            v[8] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[9] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, +d2));
            v[10] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[11] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, -d2));

            // Fill in the bottom face vertex data.
            v[12] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, -d2));
            v[13] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, -d2));
            v[14] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, +d2));
            v[15] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, +d2));

            // Fill in the left face vertex data.
            v[16] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, +d2));
            v[17] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, +d2));
            v[18] = VertexData.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[19] = VertexData.CreateVertexPosition(new Vector3(-w2, -h2, -d2));

            // Fill in the right face vertex data.
            v[20] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, -d2));
            v[21] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, -d2));
            v[22] = VertexData.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[23] = VertexData.CreateVertexPosition(new Vector3(+w2, -h2, +d2));

            // Fill in the front face index data
            i[0] = 0; i[1] = 1; i[2] = 2;
            i[3] = 0; i[4] = 2; i[5] = 3;

            // Fill in the back face index data
            i[6] = 4; i[7] = 5; i[8] = 6;
            i[9] = 4; i[10] = 6; i[11] = 7;

            // Fill in the top face index data
            i[12] = 8; i[13] = 9; i[14] = 10;
            i[15] = 8; i[16] = 10; i[17] = 11;

            // Fill in the bottom face index data
            i[18] = 12; i[19] = 13; i[20] = 14;
            i[21] = 12; i[22] = 14; i[23] = 15;

            // Fill in the left face index data
            i[24] = 16; i[25] = 17; i[26] = 18;
            i[27] = 16; i[28] = 18; i[29] = 19;

            // Fill in the right face index data
            i[30] = 20; i[31] = 21; i[32] = 22;
            i[33] = 20; i[34] = 22; i[35] = 23;
        }
        public static void CreateSphere(float radius, uint sliceCount, uint stackCount, out VertexData[] v, out uint[] i)
        {
            List<VertexData> vertList = new List<VertexData>();

            //
            // Compute the vertices stating at the top pole and moving down the stacks.
            //

            // Poles: note that there will be texture coordinate distortion as there is
            // not a unique point on the texture map to assign to the pole when mapping
            // a rectangular texture onto a sphere.

            //TODO: Tangents and Binormals
            vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                new Vector3(0.0f, +radius, 0.0f),
                new Vector3(0.0f, +1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 0.0f)));

            float phiStep = MathUtil.Pi / stackCount;
            float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

            // Compute vertices for each stack ring (do not count the poles as rings).
            for (int st = 1; st <= stackCount - 1; ++st)
            {
                float phi = st * phiStep;

                // Vertices of ring.
                for (int sl = 0; sl <= sliceCount; ++sl)
                {
                    float theta = sl * thetaStep;

                    Vector3 position;
                    Vector3 normal;
                    Vector3 tangent;
                    Vector3 binormal;
                    Vector2 texture;

                    // spherical to cartesian
                    position.X = radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    position.Y = radius * (float)Math.Cos(phi);
                    position.Z = radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);

                    normal = position;
                    normal.Normalize();

                    // Partial derivative of P with respect to theta
                    tangent.X = -radius * (float)Math.Sin(phi) * (float)Math.Sin(theta);
                    tangent.Y = 0.0f;
                    tangent.Z = +radius * (float)Math.Sin(phi) * (float)Math.Cos(theta);
                    //tangent.W = 0.0f;
                    tangent.Normalize();

                    binormal = tangent;

                    texture.X = theta / MathUtil.Pi * 2f;
                    texture.Y = phi / MathUtil.Pi;

                    vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                        position,
                        normal,
                        tangent,
                        binormal,
                        texture));
                }
            }

            vertList.Add(VertexData.CreateVertexPositionNormalTextureTangent(
                new Vector3(0.0f, -radius, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector2(0.0f, 1.0f)));

            List<uint> indexList = new List<uint>();

            for (uint index = 1; index <= sliceCount; ++index)
            {
                indexList.Add(0);
                indexList.Add(index + 1);
                indexList.Add(index);
            }

            //
            // Compute indices for inner stacks (not connected to poles).
            //

            // Offset the indices to the index of the first vertex in the first ring.
            // This is just skipping the top pole vertex.
            uint baseIndex = 1;
            uint ringVertexCount = sliceCount + 1;
            for (uint st = 0; st < stackCount - 2; ++st)
            {
                for (uint sl = 0; sl < sliceCount; ++sl)
                {
                    indexList.Add(baseIndex + st * ringVertexCount + sl);
                    indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);

                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);
                    indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                    indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl + 1);
                }
            }

            //
            // Compute indices for bottom stack.  The bottom stack was written last to the vertex buffer
            // and connects the bottom pole to the bottom ring.
            //

            // South pole vertex was added last.
            uint southPoleIndex = (uint)vertList.Count - 1;

            // Offset the indices to the index of the first vertex in the last ring.
            baseIndex = southPoleIndex - ringVertexCount;

            for (uint index = 0; index < sliceCount; ++index)
            {
                indexList.Add(southPoleIndex);
                indexList.Add(baseIndex + index);
                indexList.Add(baseIndex + index + 1);
            }

            v = vertList.ToArray();
            i = indexList.ToArray();
        }

        public ModelContent()
        {

        }

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
                                if (OptimizeMeshes(skinnedM.ToArray(), out gmesh))
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
                        if (OptimizeMeshes(staticM.ToArray(), out gmesh))
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
