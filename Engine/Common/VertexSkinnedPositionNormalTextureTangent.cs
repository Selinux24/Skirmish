using System.Runtime.InteropServices;
using SharpDX;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using InputElement = SharpDX.Direct3D11.InputElement;

namespace Engine.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPositionNormalTextureTangent : IVertexData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;
        public Vector3 Tangent;
        public Vector3 BiNormal;
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
                return Marshal.SizeOf(typeof(VertexSkinnedPositionNormalTextureTangent));
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
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 48, 0, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 60, 0, InputClassification.PerVertexData, 0 ),
            };
        }
        public static VertexSkinnedPositionNormalTextureTangent Create(VertexData v, Weight[] vw)
        {
            return new VertexSkinnedPositionNormalTextureTangent
            {
                Position = v.Position.HasValue ? v.Position.Value : Vector3.Zero,
                Normal = v.Normal.HasValue ? v.Normal.Value : Vector3.Zero,
                Texture = v.Texture.HasValue ? v.Texture.Value : Vector2.Zero,
                Tangent = v.Tangent.HasValue ? v.Tangent.Value : Vector3.Zero,
                BiNormal = v.BiNormal.HasValue ? v.BiNormal.Value : Vector3.Zero,
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
                return VertexTypes.PositionNormalTextureTangentSkinned;
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
            return new VertexSkinnedPositionNormalTextureTangent()
            {
                Position = v.Position.Value,
                Normal = v.Normal.Value,
                Texture = v.Texture.Value,
                Tangent = v.Tangent.Value,
                BiNormal = v.BiNormal.Value,
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
            return string.Format("Skinned; Position: {0}; Normal: {1}; Texture: {2}; Tangent: {3}", this.Position, this.Normal, this.Texture, this.Tangent);
        }
    }
}
