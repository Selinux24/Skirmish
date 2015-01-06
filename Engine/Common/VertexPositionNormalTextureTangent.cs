using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Common
{
    /// <summary>
    /// Position normal texure and tangent vertex format
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexPositionNormalTextureTangent : IVertexData
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
                new InputElement("NORMAL", 0, SharpDX.DXGI.Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, 24, 0, InputClassification.PerVertexData, 0),
                new InputElement("TANGENT", 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 32, 0, InputClassification.PerVertexData, 0),
            };
        }

        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;
        /// <summary>
        /// Tangent
        /// </summary>
        public Vector3 Tangent;
        /// <summary>
        /// Binormal
        /// </summary>
        public Vector3 BiNormal;
        /// <summary>
        /// Vertex type
        /// </summary>
        public VertexTypes VertexType
        {
            get
            {
                return VertexTypes.PositionNormalTextureTangent;
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public int Stride
        {
            get
            {
                return Marshal.SizeOf(typeof(VertexPositionNormalTextureTangent));
            }
        }

        /// <summary>
        /// Text representation of vertex
        /// </summary>
        /// <returns>Returns the text representation of vertex</returns>
        public override string ToString()
        {
            return string.Format("Position: {0}; Normal: {1}; Texture: {2}; Tangent: {3}", this.Position, this.Normal, this.Texture, this.Tangent);
        }
    };
}
