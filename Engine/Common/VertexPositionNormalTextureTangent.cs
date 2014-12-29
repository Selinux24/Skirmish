using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTextureTangent : IVertexData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public Vector3 Tangent;
        public Vector3 BiNormal;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionNormalTextureTangent));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
                new InputElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 32, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionNormalTextureTangent Create(VertexData v)
        {
            return new VertexPositionNormalTextureTangent
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.Zero,
                BiNormal = v.BiNormal.HasValue ? v.BiNormal.Value : Vector3.Zero,
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalTextureTangent;
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
            return new VertexPositionNormalTextureTangent()
            {
                Position = v.Position.Value,
                Normal = v.Normal.Value,
                Texture = v.Texture.Value,
                Tangent = v.Tangent.Value,
                BiNormal = v.BiNormal.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Normal: {1}; Texture: {2}; Tangent: {3}", this.Position, this.Normal, this.Texture, this.Tangent);
        }
    };
}
