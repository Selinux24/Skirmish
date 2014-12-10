using System.Runtime.InteropServices;
using SharpDX;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPositionTexture : IVertex
    {
        public Vector3 Position;
        public Vector2 Texture;
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
                return Marshal.SizeOf(typeof(VertexSkinnedPositionTexture));
            }
        }
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 20, 0, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 32, 0, InputClassification.PerVertexData, 0 ),
            };
        }
        public static VertexSkinnedPositionTexture Create(Vertex v, Weight[] vw)
        {
            return new VertexSkinnedPositionTexture
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
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
                return VertexTypes.PositionTextureSkinned;
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
            return new VertexSkinnedPositionTexture()
            {
                Position = v.Position.Value,
                Texture = v.Texture.Value,
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
            return string.Format("Skinned; Position: {0}; Texture: {1}", this.Position, this.Texture);
        }
    }
}
