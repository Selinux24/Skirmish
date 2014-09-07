using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalColor : IVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color4 Color;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionNormalColor));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 24, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionNormalColor[] Create(Vector3[] positions, Vector3[] normals, Color4 color)
        {
            List<VertexPositionNormalColor> vertices = new List<VertexPositionNormalColor>();

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.Add(new VertexPositionNormalColor()
                {
                    Position = positions[i],
                    Normal = normals[i],
                    Color = color,
                });
            }

            return vertices.ToArray();
        }
        public static VertexPositionNormalColor[] Create(Vertex[] vertexes)
        {
            List<VertexPositionNormalColor> vertices = new List<VertexPositionNormalColor>();

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices.Add(new VertexPositionNormalColor()
                {
                    Position = vertexes[i].Position.Value,
                    Normal = vertexes[i].Normal.Value,
                    Color = vertexes[i].Color.Value,
                });
            }

            return vertices.ToArray();
        }

        public VertexTypes GetVertexType()
        {
            return VertexTypes.PositionNormalColor;
        }
        public int GetStride()
        {
            return SizeInBytes;
        }
        public IVertex Convert(Vertex vert)
        {
            return new VertexPositionNormalColor()
            {
                Position = vert.Position.Value,
                Normal = vert.Normal.Value,
                Color = vert.Color.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0} Normal: {1}; Color: {2}", this.Position, this.Normal, this.Color);
        }
    };
}
