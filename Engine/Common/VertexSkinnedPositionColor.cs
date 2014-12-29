using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPositionColor : IVertexData
    {
        public Vector3 Position;
        public Color4 Color;
        public float Weight1;
        public float Weight2;
        public float Weight3;
        public byte BoneIndex1;
        public byte BoneIndex2;
        public byte BoneIndex3;
        public byte BoneIndex4;

        public static int SizeInBytes
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexSkinnedPositionColor));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 28, 0, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 40, 0, InputClassification.PerVertexData, 0 ),
           };
        }
        public static VertexSkinnedPositionColor Create(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionColor
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Color = v.Color.HasValue ? v.Color.Value : Color4.Black,
                Weight1 = ((vw != null) && (vw.Length > 0)) ? vw[0].WeightValue : 0f,
                Weight2 = ((vw != null) && (vw.Length > 1)) ? vw[1].WeightValue : 0f,
                Weight3 = ((vw != null) && (vw.Length > 2)) ? vw[2].WeightValue : 0f,
                BoneIndex1 = ((vw != null) && (vw.Length > 0)) ? ((byte)vw[0].BoneIndex) : ((byte)0),
                BoneIndex2 = ((vw != null) && (vw.Length > 1)) ? ((byte)vw[1].BoneIndex) : ((byte)0),
                BoneIndex3 = ((vw != null) && (vw.Length > 2)) ? ((byte)vw[2].BoneIndex) : ((byte)0),
                BoneIndex4 = ((vw != null) && (vw.Length > 3)) ? ((byte)vw[3].BoneIndex) : ((byte)0)
            };
        }

        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionColorSkinned;
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
            return new VertexSkinnedPositionColor()
            {
                Position = v.Position.Value,
                Color = v.Color.Value,
                Weight1 = v.Weights[0],
                Weight2 = v.Weights[1],
                Weight3 = v.Weights[2],
                BoneIndex1 = v.BoneIndices[0],
                BoneIndex2 = v.BoneIndices[1],
                BoneIndex3 = v.BoneIndices[2],
                BoneIndex4 = v.BoneIndices[3],
            };
        }
        public override string ToString()
        {
            return string.Format("Skinned; Position: {0}; Color: {1}", this.Position, this.Color);
        }
    };
}
