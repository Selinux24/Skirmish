using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionTexture : IVertex
    {
        public Vector3 Position;
        public Vector2 Texture;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionTexture));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
            };
        }
        public static VertexPositionTexture Create(Vertex v)
        {
            return new VertexPositionTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionTexture;
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
            return new VertexPositionTexture()
            {
                Position = v.Position.Value,
                Texture = v.Texture.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Texture: {1}", this.Position, this.Texture);
        }
    };
}
