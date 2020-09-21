using SharpDX;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Instancing data
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexInstancingData : IInstacingData
    {
        /// <summary>
        /// Defined input colection
        /// </summary>
        public static InputElement[] Input(int slot)
        {
            return new InputElement[]
            {
                new InputElement("localTransform", 0, Format.R32G32B32A32_Float, 0, slot, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 1, Format.R32G32B32A32_Float, 16, slot, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 2, Format.R32G32B32A32_Float, 32, slot, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 3, Format.R32G32B32A32_Float, 48, slot, InputClassification.PerInstanceData, 1),
                new InputElement("textureIndex", 0, Format.R32_UInt, 64, slot, InputClassification.PerInstanceData, 1),
                new InputElement("materialIndex", 0, Format.R32_UInt, 68, slot, InputClassification.PerInstanceData, 1),
                new InputElement("animationOffset", 0, Format.R32_UInt, 72, slot, InputClassification.PerInstanceData, 1),
            };
        }

        /// <summary>
        /// Local transformation for the instance
        /// </summary>
        public Matrix Local;
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex;
        /// <summary>
        /// Material index
        /// </summary>
        public uint MaterialIndex;
        /// <summary>
        /// Animation offset in current clip
        /// </summary>
        public uint AnimationOffset;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="local">Local transform</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="materialIndex">Material index</param>
        /// <param name="animationOffset">Animation offset</param>
        public VertexInstancingData(Matrix local, uint textureIndex = 0, uint materialIndex = 0, uint animationOffset = 0)
        {
            Local = local;
            TextureIndex = textureIndex;
            MaterialIndex = materialIndex;
            AnimationOffset = animationOffset;
        }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexInstancingData));
        }
    };

    /// <summary>
    /// Instancing data interface
    /// </summary>
    public interface IInstacingData : IBufferData
    {

    }
}
