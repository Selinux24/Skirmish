using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalColor : IVertexData
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
        public static VertexPositionNormalColor Create(VertexData v)
        {
            return new VertexPositionNormalColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalColor;
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
            return new VertexPositionNormalColor()
            {
                Position = v.Position.Value,
                Normal = v.Normal.Value,
                Color = v.Color.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0} Normal: {1}; Color: {2}", this.Position, this.Normal, this.Color);
        }
    };
}
