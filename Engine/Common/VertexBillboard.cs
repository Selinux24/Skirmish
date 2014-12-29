using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexBillboard : IVertexData
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
        public static VertexBillboard Create(VertexData v)
        {
            return new VertexBillboard
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Size = v.Size.HasValue ? v.Size.Value : Vector2.One,
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.Billboard;
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
            return new VertexBillboard()
            {
                Position = v.Position.Value,
                Size = v.Size.Value,
            };
        }
        public override string ToString()
        {
            return string.Format("Position: {0}; Size: {1}", this.Position, this.Size);
        }
    };
}
