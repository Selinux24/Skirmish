using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexBillboard : IVertex
    {
        public Vector3 Position;
        public Vector2 Size;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexBillboard));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("SIZE", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexBillboard[] Create(Vector3[] positions, Vector2 size)
        {
            List<VertexBillboard> vertices = new List<VertexBillboard>();

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.Add(new VertexBillboard()
                {
                    Position = positions[i],
                    Size = size,
                });
            }

            return vertices.ToArray();
        }
        public static VertexBillboard[] Create(Vertex[] vertexes)
        {
            List<VertexBillboard> vertices = new List<VertexBillboard>();

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices.Add(new VertexBillboard()
                {
                    Position = vertexes[i].Position.Value,
                    Size = vertexes[i].Size.Value,
                });
            }

            return vertices.ToArray();
        }

        public VertexTypes GetVertexType()
        {
            return VertexTypes.Billboard;
        }
        public int GetStride()
        {
            return SizeInBytes;
        }
        public IVertex Convert(Vertex vert)
        {
            return new VertexBillboard()
            {
                Position = vert.Position.Value,
                Size = vert.Size.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Size: {1}", this.Position, this.Size);
        }
    };
}
