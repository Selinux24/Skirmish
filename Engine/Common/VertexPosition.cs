using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
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
        public static VertexPosition Create(Vertex v)
        {
            return new VertexPosition
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.Position;
            }
        }
        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
        public IVertex Convert(Vertex v)
        {
            return new VertexPosition()
            {
                Position = v.Position.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}", this.Position);
        }
    };
}
