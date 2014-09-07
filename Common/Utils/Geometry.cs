using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using MapMode = SharpDX.Direct3D11.MapMode;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Common.Utils
{
    public class Geometry : IDisposable
    {
        private Buffer vertexBuffer = null;
        private Buffer instancingBuffer = null;
        private Buffer indexBuffer = null;

        public VertexTypes VertextType { get; set; }
        public int VertexBufferStride { get; set; }
        public int VertexBufferOffset { get; set; }
        public PrimitiveTopology Topology { get; set; }
        public int VertexCount { get; private set; }
        public int InstancingBufferStride { get; private set; }
        public int InstancingBufferOffset { get; private set; }
        public int InstanceCount { get; private set; }
        public bool Indexed
        {
            get
            {
                return this.indexBuffer != null;
            }
        }
        public Format IndexBufferFormat { get; set; }
        public int IndexCount { get; private set; }
        public Material Material { get; set; }

        public static Vector4[] calculateMeshTangents(Vertex[] vertexes, uint[] triangles)
        {
            Vector3[] vertices = Vertex.GetPositions(vertexes);
            Vector2[] uv = Vertex.GetTextures(vertexes);
            Vector3[] normals = Vertex.GetNormals(vertexes);

            int triangleCount = triangles.Length;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Vector4[] tangents = new Vector4[vertexCount];

            for (long i = 0; i < triangleCount; i += 3)
            {
                uint i1 = triangles[i + 0];
                uint i2 = triangles[i + 1];
                uint i3 = triangles[i + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                float s1 = w2.X - w1.X;
                float s2 = w3.X - w1.X;
                float t1 = w2.Y - w1.Y;
                float t2 = w3.Y - w1.Y;

                float r = 1.0f / (s1 * t2 - s2 * t1);

                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            Vector3.Orthonormalize(normals, tan1);

            for (long i = 0; i < vertexCount; ++i)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];

                tangents[i].X = t.X;
                tangents[i].Y = t.Y;
                tangents[i].Z = t.Z;

                tangents[i].W = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
            }

            return tangents;
        }

        internal Geometry(VertexTypes vertextType, Material material, Buffer vBuffer, int vertexBufferStride, int vertexBufferOffset, PrimitiveTopology topology, int vertexCount)
            : this(vertextType, material, vBuffer, vertexBufferStride, vertexBufferOffset, topology, vertexCount, null, 0, 0, 0, null, Format.Unknown, 0)
        {

        }
        internal Geometry(VertexTypes vertextType, Material material, Buffer vBuffer, int vertexBufferStride, int vertexBufferOffset, PrimitiveTopology topology, int vertexCount, Buffer iBuffer, Format iBufferFormat, int indexCount)
            : this(vertextType, material, vBuffer, vertexBufferStride, vertexBufferOffset, topology, vertexCount, null, 0, 0, 0, iBuffer, iBufferFormat, indexCount)
        {

        }
        internal Geometry(VertexTypes vertextType, Material material, Buffer vBuffer, int vertexBufferStride, int vertexBufferOffset, PrimitiveTopology topology, int vertexCount, Buffer instancingBuffer, int instancingBufferStride, int instancingBufferOffset, int instanceCount)
            : this(vertextType, material, vBuffer, vertexBufferStride, vertexBufferOffset, topology, vertexCount, instancingBuffer, instancingBufferStride, instancingBufferOffset, instanceCount, null, Format.Unknown, 0)
        {

        }
        internal Geometry(VertexTypes vertextType, Material material, Buffer vBuffer, int vertexBufferStride, int vertexBufferOffset, PrimitiveTopology topology, int vertexCount, Buffer instancingBuffer, int instancingBufferStride, int instancingBufferOffset, int instanceCount, Buffer iBuffer, Format iBufferFormat, int indexCount)
        {
            this.Material = material;

            this.VertextType = vertextType;
            this.vertexBuffer = vBuffer;
            this.VertexBufferStride = vertexBufferStride;
            this.VertexBufferOffset = vertexBufferOffset;
            this.Topology = topology;
            this.VertexCount = vertexCount;

            this.instancingBuffer = instancingBuffer;
            this.InstancingBufferStride = instancingBufferStride;
            this.InstancingBufferOffset = instancingBufferOffset;
            this.InstanceCount = instanceCount;

            this.indexBuffer = iBuffer;
            this.IndexBufferFormat = iBufferFormat;
            this.IndexCount = indexCount;
        }

        public void Dispose()
        {
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }

            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }
        }

        public void Set(DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.PrimitiveTopology = this.Topology;

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertexBuffer, this.VertexBufferStride, this.VertexBufferOffset));

            if (this.Indexed)
            {
                deviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, this.IndexBufferFormat, 0);
            }
            else
            {
                deviceContext.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            }
        }
        public void Draw(DeviceContext deviceContext, int count = 0)
        {
            if (this.Indexed)
            {
                deviceContext.DrawIndexed(count == 0 ? this.IndexCount : count, 0, 0);
            }
            else
            {
                deviceContext.Draw(count == 0 ? this.VertexCount : count, 0);
            }
        }
        public void SetInstancing(DeviceContext deviceContext)
        {
            deviceContext.InputAssembler.SetVertexBuffers(
                0,
                new VertexBufferBinding[]
                {
                    new VertexBufferBinding(this.vertexBuffer, this.VertexBufferStride, this.VertexBufferOffset),
                    new VertexBufferBinding(this.instancingBuffer, this.InstancingBufferStride, this.InstancingBufferOffset),
                });

            deviceContext.InputAssembler.PrimitiveTopology = this.Topology;

            if (this.Indexed)
            {
                deviceContext.InputAssembler.SetIndexBuffer(this.indexBuffer, this.IndexBufferFormat, 0);
            }
            else
            {
                deviceContext.InputAssembler.SetIndexBuffer(null, Format.Unknown, 0);
            }
        }
        public void DrawInstancing(DeviceContext deviceContext, int count = 0)
        {
            if (this.Indexed)
            {
                deviceContext.DrawIndexedInstanced(count == 0 ? this.IndexCount : count, this.InstanceCount, 0, 0, 0);
            }
            else
            {
                deviceContext.DrawInstanced(count == 0 ? this.VertexCount : count, this.InstanceCount, 0, 0);
            }
        }

        public void WriteVertexData<T>(DeviceContext deviceContext, T[] vertices) where T : struct, IVertex
        {
            if (vertices != null && vertices.Length > 0)
            {
                if (this.VertextType != vertices[0].GetVertexType())
                {
                    throw new Exception("Tipo de vértice incorrecto para el Buffer");
                }

                DataStream stream;
                deviceContext.MapSubresource(this.vertexBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = 0;
                    stream.WriteRange<T>(vertices);
                }
                deviceContext.UnmapSubresource(this.vertexBuffer, 0);
            }
        }
        public void WriteIndexData(DeviceContext deviceContext, uint[] indices)
        {
            if (indices != null && indices.Length > 0)
            {
                DataStream stream;
                deviceContext.MapSubresource(this.indexBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = 0;
                    stream.WriteRange(indices);
                }
                deviceContext.UnmapSubresource(this.indexBuffer, 0);
            }
        }
        public void WriteInstancingData<T>(DeviceContext deviceContext, T[] data) where T : struct
        {
            if (data != null && data.Length > 0)
            {
                DataStream stream;
                deviceContext.MapSubresource(this.instancingBuffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                using (stream)
                {
                    stream.Position = 0;
                    stream.WriteRange<T>(data);
                }
                deviceContext.UnmapSubresource(this.instancingBuffer, 0);
            }
        }
    }
}
