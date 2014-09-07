using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using BufferDescription = SharpDX.Direct3D11.BufferDescription;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using Device = SharpDX.Direct3D11.Device;
using FilterFlags = SharpDX.Direct3D11.FilterFlags;
using ImageLoadInformation = SharpDX.Direct3D11.ImageLoadInformation;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using MapMode = SharpDX.Direct3D11.MapMode;
using Resource = SharpDX.Direct3D11.Resource;
using ResourceOptionFlags = SharpDX.Direct3D11.ResourceOptionFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using ShaderResourceViewDescription = SharpDX.Direct3D11.ShaderResourceViewDescription;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using Texture2DDescription = SharpDX.Direct3D11.Texture2DDescription;

namespace Common.Utils
{
    using Common.Collada;
    using Common.Collada.Types;

    public static class ResourceUtils
    {
        private static Buffer CreateVertexBufferImmutable<T>(this Device device, IList<T> data)
           where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Immutable,
                BindFlags.VertexBuffer,
                CpuAccessFlags.None);
        }
        private static Buffer CreateIndexBufferImmutable<T>(this Device device, IList<T> data)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Immutable,
                BindFlags.IndexBuffer,
                CpuAccessFlags.None);
        }
        private static Buffer CreateVertexBufferWrite<T>(this Device device, IList<T> data)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Dynamic,
                BindFlags.VertexBuffer,
                CpuAccessFlags.Write);
        }
        private static Buffer CreateIndexBufferWrite<T>(this Device device, IList<T> data)
            where T : struct
        {
            return CreateBuffer<T>(
                device,
                data,
                ResourceUsage.Dynamic,
                BindFlags.IndexBuffer,
                CpuAccessFlags.Write);
        }
        private static Buffer CreateBuffer<T>(this Device device, IList<T> data, ResourceUsage usage, BindFlags binding, CpuAccessFlags access)
            where T : struct
        {
            int sizeInBytes = Marshal.SizeOf(typeof(T)) * data.Count;

            using (DataStream d = new DataStream(sizeInBytes, true, true))
            {
                foreach (T v in data)
                {
                    d.Write<T>(v);
                }
                d.Position = 0;

                return new Buffer(
                    device,
                    d,
                    new BufferDescription()
                    {
                        Usage = usage,
                        SizeInBytes = sizeInBytes,
                        BindFlags = binding,
                        CpuAccessFlags = access,
                        OptionFlags = ResourceOptionFlags.None,
                        StructureByteStride = 0,
                    });
            }
        }

        public static ShaderResourceView LoadTexture(this Device device, string filename)
        {
            return ShaderResourceView.FromFile(device, filename);
        }
        public static ShaderResourceView LoadTextureArray(this Device device, string[] filenames)
        {
            List<Texture2D> textureList = new List<Texture2D>();

            for (int i = 0; i < filenames.Length; i++)
            {
                textureList.Add(Texture2D.FromFile<Texture2D>(
                    device,
                    filenames[i],
                    new ImageLoadInformation()
                    {
                        FirstMipLevel = 0,
                        Usage = ResourceUsage.Staging,
                        BindFlags = BindFlags.None,
                        CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                        OptionFlags = ResourceOptionFlags.None,
                        Format = Format.R8G8B8A8_UNorm,
                        Filter = FilterFlags.None,
                        MipFilter = FilterFlags.Linear,
                    }));
            }

            Texture2DDescription textureDescription = textureList[0].Description;

            using (Texture2D textureArray = new Texture2D(
                device,
                new Texture2DDescription()
                {
                    Width = textureDescription.Width,
                    Height = textureDescription.Height,
                    MipLevels = textureDescription.MipLevels,
                    ArraySize = filenames.Length,
                    Format = textureDescription.Format,
                    SampleDescription = new SampleDescription()
                    {
                        Count = 1,
                        Quality = 0,
                    },
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                }))
            {

                for (int i = 0; i < textureList.Count; i++)
                {
                    for (int mipLevel = 0; mipLevel < textureDescription.MipLevels; mipLevel++)
                    {
                        DataBox mappedTex2D = device.ImmediateContext.MapSubresource(
                            textureList[i],
                            mipLevel,
                            MapMode.Read,
                            MapFlags.None);

                        int subIndex = Resource.CalculateSubResourceIndex(
                            mipLevel,
                            i,
                            textureDescription.MipLevels);

                        device.ImmediateContext.UpdateSubresource(
                            textureArray,
                            subIndex,
                            null,
                            mappedTex2D.DataPointer,
                            mappedTex2D.RowPitch,
                            mappedTex2D.SlicePitch);

                        device.ImmediateContext.UnmapSubresource(
                            textureList[i],
                            mipLevel);
                    }

                    textureList[i].Dispose();
                }

                ShaderResourceView result = new ShaderResourceView(
                    device,
                    textureArray,
                    new ShaderResourceViewDescription()
                    {
                        Format = textureDescription.Format,
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource()
                        {
                            MostDetailedMip = 0,
                            MipLevels = textureDescription.MipLevels,
                            FirstArraySlice = 0,
                            ArraySize = filenames.Length,
                        },
                    });

                return result;
            }
        }
        public static ShaderResourceView LoadTextureCube(this Device device, string filename, int faceSize)
        {
            Format format = Format.R8G8B8A8_UNorm;

            using (Texture2D cubeTex = new Texture2D(
                device,
                new Texture2DDescription()
                {
                    Width = faceSize,
                    Height = faceSize,
                    MipLevels = 0,
                    ArraySize = 6,
                    SampleDescription = new SampleDescription()
                    {
                        Count = 1,
                        Quality = 0,
                    },
                    Format = format,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.GenerateMipMaps | ResourceOptionFlags.TextureCube,
                }))
            {
                return new ShaderResourceView(
                    device,
                    cubeTex,
                    new ShaderResourceViewDescription()
                    {
                        Format = format,
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource()
                        {
                            MostDetailedMip = 0,
                            MipLevels = -1,
                        },
                    });
            }
        }

        public static Geometry CreateGeometry<T>(this Device device, Material material, IList<T> vertices, PrimitiveTopology topology)
            where T : struct, IVertex
        {
            VertexTypes vertexType = vertices[0].GetVertexType();
            int stride = vertices[0].GetStride();

            Buffer vBuffer = CreateVertexBufferWrite(device, vertices);

            return new Geometry(
                vertexType,
                material,
                vBuffer, stride, 0, topology, vertices.Count);
        }
        public static Geometry CreateGeometry<T>(this Device device, Material material, IList<T> vertices, PrimitiveTopology topology, IList<uint> indices)
            where T : struct, IVertex
        {
            VertexTypes vertexType = vertices[0].GetVertexType();
            int stride = vertices[0].GetStride();

            Buffer vBuffer = CreateVertexBufferWrite(device, vertices);
            Buffer iBuffer = CreateIndexBufferWrite(device, indices);

            return new Geometry(
                vertexType,
                material, vBuffer, stride, 0, topology, vertices.Count,
                iBuffer, Format.R32_UInt, indices.Count);
        }
        public static Geometry CreateInstancedGeometry<T, Y>(this Device device, Material material, IList<T> vertices, PrimitiveTopology topology, IList<Y> instancingData)
            where T : struct, IVertex
            where Y : struct
        {
            VertexTypes vertexType = vertices[0].GetVertexType();
            int stride = vertices[0].GetStride();

            Buffer vBuffer = CreateVertexBufferWrite(device, vertices);
            Buffer insBuffer = CreateVertexBufferWrite(device, instancingData);

            return new Geometry(
                vertexType,
                material,
                vBuffer, stride, 0, topology, vertices.Count,
                insBuffer, Marshal.SizeOf(typeof(Y)), 0, instancingData.Count);
        }
        public static Geometry CreateInstancedGeometry<T, Y>(this Device device, Material material, IList<T> vertices, PrimitiveTopology topology, IList<uint> indices, IList<Y> instancingData)
            where T : struct, IVertex
            where Y : struct
        {
            VertexTypes vertexType = vertices[0].GetVertexType();
            int stride = vertices[0].GetStride();

            Buffer vBuffer = CreateVertexBufferWrite(device, vertices);
            Buffer insBuffer = CreateVertexBufferWrite(device, instancingData);
            Buffer iBuffer = CreateIndexBufferWrite(device, indices);

            return new Geometry(
                vertexType,
                material,
                vBuffer, stride, 0, topology, vertices.Count,
                insBuffer, Marshal.SizeOf(typeof(Y)), 0, instancingData.Count,
                iBuffer, Format.R32_UInt, indices.Count);
        }

        public static Geometry[] LoadCollada(this Device device, string filename)
        {
            return LoadCollada(device, filename, Matrix.Identity, Matrix.Identity, Matrix.Identity);
        }
        public static Geometry[] LoadCollada(this Device device, string filename, Matrix translation, Matrix rotation, Matrix scale)
        {
            List<Geometry> res = new List<Geometry>();

            Dae dae = Dae.Load(filename);

            if (dae.Asset.UpAxisType == UpAxisType.X)
            {
                rotation = Matrix.RotationZ(MathUtil.DegreesToRadians(-90f)) * rotation;
            }
            else if (dae.Asset.UpAxisType == UpAxisType.Z)
            {
                rotation = Matrix.RotationX(MathUtil.DegreesToRadians(-90f)) * rotation;
            }

            ColladaGeometryInfo[] geo = dae.MapScene(dae.VisualScenes[0], translation, rotation, scale, true, Path.GetDirectoryName(filename));
            if (geo != null && geo.Length > 0)
            {
                for (int i = 0; i < geo.Length; i++)
                {
                    if (geo[i].VertexType == VertexTypes.Position)
                        res.Add(device.CreateGeometry(geo[i].Material, geo[i].Position, geo[i].Topology));
                    if (geo[i].VertexType == VertexTypes.PositionColor)
                        res.Add(device.CreateGeometry(geo[i].Material, geo[i].PositionColor, geo[i].Topology));
                    if (geo[i].VertexType == VertexTypes.PositionNormalColor)
                        res.Add(device.CreateGeometry(geo[i].Material, geo[i].PositionNormalColor, geo[i].Topology));
                    if (geo[i].VertexType == VertexTypes.PositionTexture)
                        res.Add(device.CreateGeometry(geo[i].Material, geo[i].PositionTexture, geo[i].Topology));
                    if (geo[i].VertexType == VertexTypes.PositionNormalTexture)
                        res.Add(device.CreateGeometry(geo[i].Material, geo[i].PositionNormalTexture, geo[i].Topology));
                }
            }

            return res.ToArray();
        }
        public static Geometry[] LoadColladaInstanced(this Device device, string filename, int count)
        {
            return LoadColladaInstanced(device, filename, Matrix.Identity, Matrix.Identity, Matrix.Identity, count);
        }
        public static Geometry[] LoadColladaInstanced(this Device device, string filename, Matrix translation, Matrix rotation, Matrix scale, int count)
        {
            List<Geometry> res = new List<Geometry>();

            Dae dae = Dae.Load(filename);

            if (dae.Asset.UpAxisType == UpAxisType.X)
            {
                rotation = Matrix.RotationZ(MathUtil.DegreesToRadians(-90f)) * rotation;
            }
            else if (dae.Asset.UpAxisType == UpAxisType.Z)
            {
                rotation = Matrix.RotationX(MathUtil.DegreesToRadians(-90f)) * rotation;
            }

            ColladaGeometryInfo[] geo = dae.MapScene(dae.VisualScenes[0], translation, rotation, scale, true, Path.GetDirectoryName(filename));
            if (geo != null && geo.Length > 0)
            {
                for (int i = 0; i < geo.Length; i++)
                {
                    if (geo[i].VertexType == VertexTypes.Position)
                        res.Add(device.CreateInstancedGeometry(geo[i].Material, geo[i].Position, geo[i].Topology, new BufferInstancingData[count]));
                    if (geo[i].VertexType == VertexTypes.PositionColor)
                        res.Add(device.CreateInstancedGeometry(geo[i].Material, geo[i].PositionColor, geo[i].Topology, new BufferInstancingData[count]));
                    if (geo[i].VertexType == VertexTypes.PositionNormalColor)
                        res.Add(device.CreateInstancedGeometry(geo[i].Material, geo[i].PositionNormalColor, geo[i].Topology, new BufferInstancingData[count]));
                    if (geo[i].VertexType == VertexTypes.PositionTexture)
                        res.Add(device.CreateInstancedGeometry(geo[i].Material, geo[i].PositionTexture, geo[i].Topology, new BufferInstancingData[count]));
                    if (geo[i].VertexType == VertexTypes.PositionNormalTexture)
                        res.Add(device.CreateInstancedGeometry(geo[i].Material, geo[i].PositionNormalTexture, geo[i].Topology, new BufferInstancingData[count]));
                }
            }

            return res.ToArray();
        }
        public static Geometry[] PopulateBillboard(this Device device, string filename, Matrix translation, Matrix rotation, Matrix scale, string[] textures, float saturation, int seed = 0)
        {
            List<Geometry> geoBillboard = new List<Geometry>();

            Dae dae = Dae.Load(filename);

            if (dae.Asset.UpAxisType == UpAxisType.X)
            {
                rotation = Matrix.RotationZ(MathUtil.DegreesToRadians(-90f)) * rotation;
            }
            else if (dae.Asset.UpAxisType == UpAxisType.Z)
            {
                rotation = Matrix.RotationX(MathUtil.DegreesToRadians(-90f)) * rotation;
            }

            Material mat = Material.CreateTextured("array", textures);

            Random rnd = new Random(seed);

            ColladaGeometryInfo[] geo = dae.MapScene(dae.VisualScenes[0], translation, rotation, scale, true, Path.GetDirectoryName(filename));
            if (geo != null && geo.Length > 0)
            {
                List<VertexBillboard> vertexBillboard = new List<VertexBillboard>();

                for (int i = 0; i < geo.Length; i++)
                {
                    Triangle[] triangleList = geo[i].ComputeTriangleList();

                    foreach (Triangle tri in triangleList)
                    {
                        float area = tri.Area;
                        float inc = tri.Inclination;

                        //Obtener número de billboardas en este triángulo
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

                            float s = 1f + rnd.NextFloat(0f, 0.5f);

                            Vector2 bbsize = new Vector2(s, s);
                            bbpos.Y += bbsize.Y * 0.5f;

                            vertexBillboard.Add(new VertexBillboard() { Position = bbpos, Size = bbsize, });
                        }
                    }
                }

                geoBillboard.Add(device.CreateGeometry(mat, vertexBillboard.ToArray(), PrimitiveTopology.PointList));
            }

            return geoBillboard.ToArray();
        }
        public static Geometry GenerateSkydom(this Device device, Material material, float radius)
        {
            Vertex[] verts;
            uint[] indices;
            ResourceUtils.CreateSphere(radius, 30, 30, out verts, out indices);

            return CreateGeometry<VertexPosition>(
                device,
                material,
                Vertex.Convert<VertexPosition>(verts),
                PrimitiveTopology.TriangleList,
                indices);
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

            vertList.Add(Vertex.CreateVertexPositionNormalTangentTexture(new Vector3(0.0f, +radius, 0.0f), new Vector3(0.0f, +1.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f)));

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
                    tangent.Normalize();

                    texture.X = theta / MathUtil.Pi * 2f;
                    texture.Y = phi / MathUtil.Pi;

                    vertList.Add(Vertex.CreateVertexPositionNormalTangentTexture(position, normal, tangent, texture));
                }
            }

            vertList.Add(Vertex.CreateVertexPositionNormalTangentTexture(new Vector3(0.0f, -radius, 0.0f), new Vector3(0.0f, -1.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), new Vector2(0.0f, 1.0f)));

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
        public static void CreateGeoSphere(float radius, uint numSubdivisions, out Vertex[] v, out uint[] i)
        {
            // Put a cap on the number of subdivisions.
            numSubdivisions = Math.Min(numSubdivisions, 5u);

            // Approximate a sphere by tessellating an icosahedron.
            const float X = 0.525731f;
            const float Z = 0.850651f;

            Vector3[] pos = new Vector3[]
	        {
		        new Vector3(-X, 0.0f, Z),  new Vector3(X, 0.0f, Z),  
		        new Vector3(-X, 0.0f, -Z), new Vector3(X, 0.0f, -Z),    
		        new Vector3(0.0f, Z, X),   new Vector3(0.0f, Z, -X), 
		        new Vector3(0.0f, -Z, X),  new Vector3(0.0f, -Z, -X),    
		        new Vector3(Z, X, 0.0f),   new Vector3(-Z, X, 0.0f), 
		        new Vector3(Z, -X, 0.0f),  new Vector3(-Z, -X, 0.0f)
	        };

            uint[] k = new uint[]
            {
		        1,4,0,  4,9,0,  4,5,9,  8,5,4,  1,8,4,    
		        1,10,8, 10,3,8, 8,3,5,  3,2,5,  3,7,2,    
		        3,10,7, 10,6,7, 6,11,7, 6,0,11, 6,1,0, 
		        10,1,6, 11,0,9, 2,11,9, 5,2,9,  11,2,7 
	        };

            v = new Vertex[12];
            i = new uint[60];

            for (uint p = 0; p < 12; ++p)
                v[p].Position = pos[p];

            for (uint p = 0; p < 60; ++p)
                i[p] = k[p];

            for (uint p = 0; p < numSubdivisions; ++p)
                Subdivide(ref v, ref i);

            // Project vertices onto sphere and scale.
            for (uint vert = 0; vert < v.Length; ++vert)
            {
                // Project onto unit sphere.
                Vector3 normal = Vector3.Normalize(v[vert].Position.Value);

                // Project onto sphere.
                Vector3 position = radius * normal;

                v[vert].Position = position;
                v[vert].Normal = normal;

                // Derive texture coordinates from spherical coordinates.
                float theta = AngleFromXY(
                    v[vert].Position.Value.X,
                    v[vert].Position.Value.Z);

                float phi = (float)Math.Acos(v[vert].Position.Value.Y / radius);

                v[vert].Texture = new Vector2(theta / MathUtil.Pi * 2f, phi / MathUtil.Pi);

                // Partial derivative of P with respect to theta
                v[vert].Tangent = Vector3.Normalize(new Vector3(-radius * (float)Math.Sin(phi) * (float)Math.Sin(theta), 0.0f, +radius * (float)Math.Sin(phi) * (float)Math.Cos(theta)));
            }
        }
        public static void Subdivide(ref Vertex[] v, ref uint[] i)
        {
            List<Vertex> vertlist = new List<Vertex>();
            List<uint> indexList = new List<uint>();

            //       v1
            //       *
            //      / \
            //     /   \
            //  m0*-----*m1
            //   / \   / \
            //  /   \ /   \
            // *-----*-----*
            // v0    m2     v2

            int numTris = v.Length / 3;
            for (uint t = 0; t < numTris; ++t)
            {
                Vector3 v0 = v[i[t * 3 + 0]].Position.Value;
                Vector3 v1 = v[i[t * 3 + 1]].Position.Value;
                Vector3 v2 = v[i[t * 3 + 2]].Position.Value;

                //
                // Generate the midpoints.
                //
                Vector3 m0, m1, m2;

                // For subdivision, we just care about the position component.  We derive the other
                // vertex components in CreateGeosphere.
                m0 = new Vector3(
                    0.5f * (v0.X + v1.X),
                    0.5f * (v0.Y + v1.Y),
                    0.5f * (v0.Z + v1.Z));

                m1 = new Vector3(
                    0.5f * (v1.X + v2.X),
                    0.5f * (v1.Y + v2.Y),
                    0.5f * (v1.Z + v2.Z));

                m2 = new Vector3(
                    0.5f * (v0.X + v2.X),
                    0.5f * (v0.Y + v2.Y),
                    0.5f * (v0.Z + v2.Z));

                //
                // Add new geometry.
                //

                vertlist.Add(Vertex.CreateVertexPosition(v0)); // 0
                vertlist.Add(Vertex.CreateVertexPosition(v1)); // 1
                vertlist.Add(Vertex.CreateVertexPosition(v2)); // 2
                vertlist.Add(Vertex.CreateVertexPosition(m0)); // 3
                vertlist.Add(Vertex.CreateVertexPosition(m1)); // 4
                vertlist.Add(Vertex.CreateVertexPosition(m2)); // 5

                indexList.Add(t * 6 + 0);
                indexList.Add(t * 6 + 3);
                indexList.Add(t * 6 + 5);

                indexList.Add(t * 6 + 3);
                indexList.Add(t * 6 + 4);
                indexList.Add(t * 6 + 5);

                indexList.Add(t * 6 + 5);
                indexList.Add(t * 6 + 4);
                indexList.Add(t * 6 + 2);

                indexList.Add(t * 6 + 3);
                indexList.Add(t * 6 + 1);
                indexList.Add(t * 6 + 4);
            }

            v = vertlist.ToArray();
            i = indexList.ToArray();
        }
        public static float AngleFromXY(float x, float y)
        {
            float theta = 0.0f;

            // Quadrant I or IV
            if (x >= 0.0f)
            {
                // If x = 0, then atanf(y/x) = +pi/2 if y > 0
                //                atanf(y/x) = -pi/2 if y < 0
                theta = (float)Math.Atan(y / x); // in [-pi/2, +pi/2]

                if (theta < 0.0f)
                {
                    theta += 2.0f * MathUtil.Pi; // in [0, 2*pi).
                }
            }
            else
            {
                // Quadrant II or III
                theta = (float)Math.Atan(y / x) + MathUtil.Pi; // in [0, 2*pi).
            }

            return theta;
        }
    }
}
