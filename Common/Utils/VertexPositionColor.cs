using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor : IVertex
    {
        public Vector3 Position;
        public Color4 Color;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionColor));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionColor[] Create(Vector3[] positions, Color4 color)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.Add(new VertexPositionColor()
                {
                    Position = positions[i],
                    Color = color,
                });
            }

            return vertices.ToArray();
        }
        public static VertexPositionColor[] Create(Vertex[] vertexes)
        {
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices.Add(new VertexPositionColor()
                {
                    Position = vertexes[i].Position.Value,
                    Color = vertexes[i].Color.Value,
                });
            }

            return vertices.ToArray();
        }

        public VertexTypes GetVertexType()
        {
            return VertexTypes.PositionColor;
        }
        public int GetStride()
        {
            return SizeInBytes;
        }
        public IVertex Convert(Vertex vert)
        {
            return new VertexPositionColor()
            {
                Position = vert.Position.Value,
                Color = vert.Color.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Color: {1}", this.Position, this.Color);
        }
    };
}
