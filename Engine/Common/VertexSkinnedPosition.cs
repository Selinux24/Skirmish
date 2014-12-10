using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPosition : IVertex
    {
        public Vector3 Position;
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
                return Marshal.SizeOf(typeof(VertexSkinnedPosition));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 24, 0, InputClassification.PerVertexData, 0 ),
            };
        }
        public static VertexSkinnedPosition Create(Vertex v, Weight[] vw)
        {
            return new VertexSkinnedPosition
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
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
                return VertexTypes.PositionSkinned;
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
            return new VertexSkinnedPosition()
            {
                Position = v.Position.Value,
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
            return string.Format("Skinned; Position: {0}", this.Position);
        }
    };
}
