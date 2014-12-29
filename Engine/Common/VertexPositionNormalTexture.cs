using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTexture : IVertexData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionNormalTexture));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionNormalTexture Create(VertexData v)
        {
            return new VertexPositionNormalTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalTexture;
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
            return new VertexPositionNormalTexture()
            {
                Position = v.Position.Value,
                Normal = v.Normal.Value,
                Texture = v.Texture.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Normal: {1}; Texture: {2}", this.Position, this.Normal, this.Texture);
        }
    };
}
