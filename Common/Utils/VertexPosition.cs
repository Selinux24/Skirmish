using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Common.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPosition : IVertex
    {
        public Vector3 Position;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPosition));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPosition[] Create(Vector3[] positions)
        {
            List<VertexPosition> vertices = new List<VertexPosition>();

            for (int i = 0; i < positions.Length; i++)
            {
                vertices.Add(new VertexPosition()
                {
                    Position = positions[i],
                });
            }

            return vertices.ToArray();
        }
        public static VertexPosition[] Create(Vertex[] vertexes)
        {
            List<VertexPosition> vertices = new List<VertexPosition>();

            for (int i = 0; i < vertexes.Length; i++)
            {
                vertices.Add(new VertexPosition()
                {
                    Position = vertexes[i].Position.Value,
                });
            }

            return vertices.ToArray();
        }

        public VertexTypes GetVertexType()
        {
            return VertexTypes.Position;
        }
        public int GetStride()
        {
            return SizeInBytes;
        }
        public IVertex Convert(Vertex vert)
        {
            return new VertexPosition()
            {
                Position = vert.Position.Value,    
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}", this.Position);
        }
    };
}
