using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    /// <summary>
    /// Skinned position vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexSkinnedPosition : IVertexData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        /// <returns></returns>
        public static InputElement[] GetInput()
        {
            return new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElement("WEIGHTS", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, SharpDX.DXGI.Format.R8G8B8A8_UInt, 24, 0, InputClassification.PerVertexData, 0 ),
            };
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Weight 1
        /// </summary>
        public float Weight1;
        /// <summary>
        /// Weight 2
        /// </summary>
        public float Weight2;
        /// <summary>
        /// Weight 3
        /// </summary>
        public float Weight3;
        /// <summary>
        /// Bone 1
        /// </summary>
        public byte BoneIndex1;
        /// <summary>
        /// Bone 2
        /// </summary>
        public byte BoneIndex2;
        /// <summary>
        /// Bone 3
        /// </summary>
        public byte BoneIndex3;
        /// <summary>
        /// Bone 4
        /// </summary>
        public byte BoneIndex4;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionSkinned;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexSkinnedPosition));
            }
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Skinned; Position: {0}", this.Position);
        }
    };
}
