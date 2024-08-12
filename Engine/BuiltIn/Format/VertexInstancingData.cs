using Engine.Common;
using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Format
{
    /// <summary>
    /// Instancing data
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="local">Local transform</param>
    /// <param name="tintColor">Tint color</param>
    /// <param name="textureIndex">Texture index</param>
    /// <param name="materialIndex">Material index</param>
    /// <param name="animationOffset">First animation offset</param>
    /// <param name="animationOffsetB">Second animation offset</param>
    /// <param name="animationInterpolation">Animation interpolation value between the offsets</param>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexInstancingData(Matrix local, Color4 tintColor, uint textureIndex = 0, uint materialIndex = 0, uint animationOffset = 0, uint animationOffsetB = 0, float animationInterpolation = 0f) : IInstacingData
    {
        private const string LocalTransformString = "localTransform";
        private const string TintColorString = "tintColor";
        private const string TextureIndexString = "textureIndex";
        private const string MaterialIndexString = "materialIndex";
        private const string AnimationOffsetString = "animationOffset";
        private const string AnimationOffsetBString = "animationOffsetB";
        private const string AnimationInterpolationString = "animationInterpolation";

        /// <summary>
        /// <summary>
        /// Local transformation for the instance
        /// </summary>
        public Matrix Local = local;
        /// <summary>
        /// Tint color
        /// </summary>
        public Color4 TintColor = tintColor;
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex = textureIndex;
        /// <summary>
        /// Material index
        /// </summary>
        public uint MaterialIndex = materialIndex;
        /// <summary>
        /// First animation offset in current clip
        /// </summary>
        public uint AnimationOffset = animationOffset;
        /// <summary>
        /// Second animation offset in current clip
        /// </summary>
        public uint AnimationOffsetB = animationOffsetB;
        /// <summary>
        /// Animation interpolation between offsets
        /// </summary>
        public float AnimationInterpolation = animationInterpolation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="local">Local transform</param>
        /// <param name="textureIndex">Texture index</param>
        /// <param name="materialIndex">Material index</param>
        /// <param name="animationOffset">First animation offset</param>
        /// <param name="animationOffsetB">Second animation offset</param>
        /// <param name="animationInterpolation">Animation interpolation value between the offsets</param>
        public VertexInstancingData(Matrix local, uint textureIndex = 0, uint materialIndex = 0, uint animationOffset = 0, uint animationOffsetB = 0, float animationInterpolation = 0f)
            : this(local, Color4.White, textureIndex, materialIndex, animationOffset, animationOffsetB, animationInterpolation)
        {

        }

        /// <inheritdoc/>
        public readonly EngineInputElement[] GetInput(int slot)
        {
            return
            [
                new (LocalTransformString, 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 0, slot, EngineInputClassification.PerInstanceData, 1),
                new (LocalTransformString, 1, SharpDX.DXGI.Format.R32G32B32A32_Float, 16, slot, EngineInputClassification.PerInstanceData, 1),
                new (LocalTransformString, 2, SharpDX.DXGI.Format.R32G32B32A32_Float, 32, slot, EngineInputClassification.PerInstanceData, 1),
                new (LocalTransformString, 3, SharpDX.DXGI.Format.R32G32B32A32_Float, 48, slot, EngineInputClassification.PerInstanceData, 1),
                new (TintColorString, 0, SharpDX.DXGI.Format.R32G32B32A32_Float, 64, slot, EngineInputClassification.PerInstanceData, 1),
                new (TextureIndexString, 0, SharpDX.DXGI.Format.R32_UInt, 80, slot, EngineInputClassification.PerInstanceData, 1),
                new (MaterialIndexString, 0, SharpDX.DXGI.Format.R32_UInt, 84, slot, EngineInputClassification.PerInstanceData, 1),
                new (AnimationOffsetString, 0, SharpDX.DXGI.Format.R32_UInt, 88, slot, EngineInputClassification.PerInstanceData, 1),
                new (AnimationOffsetBString, 0, SharpDX.DXGI.Format.R32_UInt, 92, slot, EngineInputClassification.PerInstanceData, 1),
                new (AnimationInterpolationString, 0, SharpDX.DXGI.Format.R32_Float, 96, slot, EngineInputClassification.PerInstanceData, 1),
            ];
        }

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(VertexInstancingData));
        }
    }
}
