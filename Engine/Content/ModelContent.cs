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
        /// Texture dictionary
        /// </summary>
        public Dictionary<string, ImageContent> Images = null;
        /// <summary>
        /// Material dictionary
        /// </summary>
        public Dictionary<string, MaterialContent> Materials = null;
        /// <summary>
        /// Geometry dictionary
        /// </summary>
        public Dictionary<string, SubMeshContent[]> Geometry = null;
        /// <summary>
        /// Controller dictionary
        /// </summary>
        public Dictionary<string, ControllerContent> Controllers = null;
        /// <summary>
        /// Animation dictionary
        /// </summary>
        public Dictionary<string, AnimationContent[]> Animations = null;
        /// <summary>
        /// Skinning information
        /// </summary>
        public SkinningContent SkinningInfo { get; set; }

        public static ModelContent GenerateVegetationBillboard(string contentFolder, string[] textures, Triangle[] triList, float saturation, Vector2 minSize, Vector2 maxSize, int seed = 0)
        {
            Random rnd = new Random(seed);

            List<Vertex> vertices = new List<Vertex>();

            #region Find billboard positions

            foreach (Triangle tri in triList)
            {
                float area = tri.Area;
                float inc = tri.Inclination;

                if (inc < 0.33f) inc = 0;

                int num = rnd.Next((int)(area * saturation * inc));
                for (int b = 0; b < num; b++)
                {
                    //Buscar un punto en el triángulo
                    Vector3 bbpos = Vector3.Zero;
                    bool found = false;
                    while (!found)
                    {
                        Vector3 v = rnd.NextVector3(tri.Min, tri.Max);
                        Ray ray = new Ray(new Vector3(v.X, 1000f, v.Z), Vector3.Down);
                        Vector3? iPoint = null;
                        float? distanceToPoint = null;
                        if (Intersections.RayAndTriangle(ray, tri, out iPoint, out distanceToPoint, false))
                        {
                            bbpos = iPoint.Value;
                            found = true;
                        }
                    }

                    Vector2 bbsize = rnd.NextVector2(minSize, maxSize);

                    bbpos.Y += bbsize.Y * 0.5f;

                    vertices.Add(Vertex.CreateVertexBillboard(bbpos, bbsize));
                }
            }

            #endregion

            Dictionary<string, ImageContent> images = new Dictionary<string, ImageContent>();
            Dictionary<string, MaterialContent> materials = new Dictionary<string, MaterialContent>();
            Dictionary<string, SubMeshContent[]> geometry = new Dictionary<string, SubMeshContent[]>();

            string imageName = "billboard";
            string materialName = "billboardMaterial";
            string geoName = "billboardGeometry";

            ImageContent imageContent = new ImageContent()
            {
                Paths = ContentManager.FindContent(contentFolder, textures),
            };

            images.Add(imageName, imageContent);

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            materials.Add(materialName, material);

            SubMeshContent geo = new SubMeshContent()
            {
                Topology = PrimitiveTopology.TriangleList,
                VertexType = VertexTypes.Billboard,
                Vertices = vertices.ToArray(),
                Indices = null,
                Material = materialName,
            };

            geometry.Add(geoName, new[] { geo });

            return new ModelContent()
            {
                Images = images,
                Materials = materials,
                Geometry = geometry,
            };
        }
        public static ModelContent GenerateSkydom(string contentFolder, string texture, float radius)
        {
            Dictionary<string, ImageContent> images = new Dictionary<string, ImageContent>();
            Dictionary<string, MaterialContent> materials = new Dictionary<string, MaterialContent>();
            Dictionary<string, SubMeshContent[]> geometry = new Dictionary<string, SubMeshContent[]>();

            string imageName = "cubeTexture";
            string materialName = "sphereMaterial";
            string geoName = "sphereGeometry";

            ImageContent image = new ImageContent()
            {
                Path = ContentManager.FindContent(contentFolder, texture),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            Vertex[] verts = null;
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

            images.Add(imageName, image);
            materials.Add(materialName, material);
            geometry.Add(geoName, new[] { geo });

            return new ModelContent()
            {
                Images = images,
                Materials = materials,
                Geometry = geometry,
            };
        }
        public static ModelContent GenerateSprite(string contentFolder, string texture, float width, float height, float formWidth, float formHeight)
        {
            Dictionary<string, ImageContent> images = new Dictionary<string, ImageContent>();
            Dictionary<string, MaterialContent> materials = new Dictionary<string, MaterialContent>();
            Dictionary<string, SubMeshContent[]> geometry = new Dictionary<string, SubMeshContent[]>();

            string imageName = "spriteTexture";
            string materialName = "spriteMaterial";
            string geoName = "spriteGeometry";

            ImageContent image = new ImageContent()
            {
                Path = ContentManager.FindContent(contentFolder, texture),
            };

            MaterialContent material = MaterialContent.Default;
            material.DiffuseTexture = imageName;

            Vertex[] verts = null;
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

            images.Add(imageName, image);
            materials.Add(materialName, material);
            geometry.Add(geoName, new[] { geo });

            return new ModelContent()
            {
                Images = images,
                Materials = materials,
                Geometry = geometry,
            };
        }

        public static void CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, out Vertex[] v, out uint[] i)
        {
            v = new Vertex[4];
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
        public static void CreateBox(float width, float height, float depth, out Vertex[] v, out uint[] i)
        {
            v = new Vertex[24];
            i = new uint[36];

            float w2 = 0.5f * width;
            float h2 = 0.5f * height;
            float d2 = 0.5f * depth;

            // Fill in the front face vertex data.
            v[0] = Vertex.CreateVertexPosition(new Vector3(-w2, -h2, -d2));
            v[1] = Vertex.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[2] = Vertex.CreateVertexPosition(new Vector3(+w2, +h2, -d2));
            v[3] = Vertex.CreateVertexPosition(new Vector3(+w2, -h2, -d2));

            // Fill in the back face vertex data.
            v[4] = Vertex.CreateVertexPosition(new Vector3(-w2, -h2, +d2));
            v[5] = Vertex.CreateVertexPosition(new Vector3(+w2, -h2, +d2));
            v[6] = Vertex.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[7] = Vertex.CreateVertexPosition(new Vector3(-w2, +h2, +d2));

            // Fill in the top face vertex data.
            v[8] = Vertex.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[9] = Vertex.CreateVertexPosition(new Vector3(-w2, +h2, +d2));
            v[10] = Vertex.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[11] = Vertex.CreateVertexPosition(new Vector3(+w2, +h2, -d2));

            // Fill in the bottom face vertex data.
            v[12] = Vertex.CreateVertexPosition(new Vector3(-w2, -h2, -d2));
            v[13] = Vertex.CreateVertexPosition(new Vector3(+w2, -h2, -d2));
            v[14] = Vertex.CreateVertexPosition(new Vector3(+w2, -h2, +d2));
            v[15] = Vertex.CreateVertexPosition(new Vector3(-w2, -h2, +d2));

            // Fill in the left face vertex data.
            v[16] = Vertex.CreateVertexPosition(new Vector3(-w2, -h2, +d2));
            v[17] = Vertex.CreateVertexPosition(new Vector3(-w2, +h2, +d2));
            v[18] = Vertex.CreateVertexPosition(new Vector3(-w2, +h2, -d2));
            v[19] = Vertex.CreateVertexPosition(new Vector3(-w2, -h2, -d2));

            // Fill in the right face vertex data.
            v[20] = Vertex.CreateVertexPosition(new Vector3(+w2, -h2, -d2));
            v[21] = Vertex.CreateVertexPosition(new Vector3(+w2, +h2, -d2));
            v[22] = Vertex.CreateVertexPosition(new Vector3(+w2, +h2, +d2));
            v[23] = Vertex.CreateVertexPosition(new Vector3(+w2, -h2, +d2));

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
        public static void CreateSphere(float radius, uint sliceCount, uint stackCount, out Vertex[] v, out uint[] i)
        {
            List<Vertex> vertList = new List<Vertex>();

            //
            // Compute the vertices stating at the top pole and moving down the stacks.
            //

            // Poles: note that there will be texture coordinate distortion as there is
            // not a unique point on the texture map to assign to the pole when mapping
            // a rectangular texture onto a sphere.

            //TODO: Tangents and Binormals
            vertList.Add(Vertex.CreateVertexPositionNormalTextureTangent(
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

                    vertList.Add(Vertex.CreateVertexPositionNormalTextureTangent(
                        position,
                        normal,
                        tangent,
                        binormal,
                        texture));
                }
            }

            vertList.Add(Vertex.CreateVertexPositionNormalTextureTangent(
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

        public Triangle[] ComputeTriangleList()
        {
            List<Triangle> list = new List<Triangle>();

            foreach (SubMeshContent[] gList in this.Geometry.Values)
            {
                foreach (SubMeshContent g in gList)
                {
                    list.AddRange(g.ComputeTriangleList());
                }
            }

            return list.ToArray();
        }
        public BoundingSphere ComputeBoundingSphere()
        {
            BoundingSphere bsphere = new BoundingSphere();

            foreach (SubMeshContent[] gList in this.Geometry.Values)
            {
                foreach (SubMeshContent g in gList)
                {
                    bsphere = BoundingSphere.Merge(bsphere, g.BoundingSphere);
                }
            }

            return bsphere;
        }
        public BoundingBox ComputeBoundingBox()
        {
            BoundingBox bbox = new BoundingBox();

            foreach (SubMeshContent[] gList in this.Geometry.Values)
            {
                foreach (SubMeshContent g in gList)
                {
                    bbox = BoundingBox.Merge(bbox, g.BoundingBox);
                }
            }

            return bbox;
        }
    }
}
