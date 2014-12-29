using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionColor : IVertexData
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
        public static VertexPositionColor Create(VertexData v)
        {
            return new VertexPositionColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionColor;
            }
        }
        public int Stride
        {
            get
            {
                return SizeInBytes;
            }
        }
        public IVertexData Convert(VertexData v)
        {
            return new VertexPositionColor()
            {
                Position = v.Position.Value,
                Color = v.Color.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Color: {1}", this.Position, this.Color);
        }
    };
}
