using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Drawers.PostProcess
{
    /// <summary>
    /// Per pass data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    struct PerEffect : IBufferData
    {
        public static PerEffect Build(BuiltInPostProcessEffects effect)
        {
            return new PerEffect
            {
                Effect = (uint)effect,
            };
        }

        [FieldOffset(0)]
        public uint Effect;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerEffect));
        }
    }

    /// <summary>
    /// Per pass structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 96)]
    struct PerPass : IBufferData
    {
        public const int MaxEffects = 8;

        public static PerPass Build(BuiltInPostProcessState state)
        {
            return new PerPass
            {
                GrayscaleIntensity = state.GrayscaleIntensity,
                SepiaIntensity = state.SepiaIntensity,
                GrainIntensity = state.GrainIntensity,

                BlurIntensity = state.BlurIntensity,
                BlurDirections = state.BlurDirections,
                BlurQuality = state.BlurQuality,
                BlurSize = state.BlurSize,

                VignetteIntensity = state.VignetteIntensity,
                VignetteOuter = state.VignetteOuter,
                VignetteInner = state.VignetteInner,

                BlurVignetteIntensity = state.BlurVignetteIntensity,
                BlurVignetteDirections = state.BlurVignetteDirections,
                BlurVignetteQuality = state.BlurVignetteQuality,
                BlurVignetteSize = state.BlurVignetteSize,
                BlurVignetteOuter = state.BlurVignetteOuter,
                BlurVignetteInner = state.BlurVignetteInner,

                BloomIntensity = state.BloomIntensity,
                BloomForce = state.BloomForce,
                BloomDirections = state.BloomDirections,
                BloomQuality = state.BloomQuality,
                BloomSize = state.BloomSize,

                ToneMappingIntensity = state.ToneMappingIntensity,
                ToneMappingTone = (uint)state.ToneMappingTone,
            };
        }

        [FieldOffset(0)]
        public float GrayscaleIntensity;
        [FieldOffset(4)]
        public float SepiaIntensity;
        [FieldOffset(8)]
        public float GrainIntensity;

        [FieldOffset(12)]
        public float BlurIntensity;
        [FieldOffset(16)]
        public float BlurDirections;
        [FieldOffset(20)]
        public float BlurQuality;
        [FieldOffset(24)]
        public float BlurSize;

        [FieldOffset(28)]
        public float VignetteIntensity;
        [FieldOffset(32)]
        public float VignetteOuter;
        [FieldOffset(36)]
        public float VignetteInner;

        [FieldOffset(40)]
        public float BlurVignetteIntensity;
        [FieldOffset(44)]
        public float BlurVignetteDirections;
        [FieldOffset(48)]
        public float BlurVignetteQuality;
        [FieldOffset(52)]
        public float BlurVignetteSize;
        [FieldOffset(56)]
        public float BlurVignetteOuter;
        [FieldOffset(60)]
        public float BlurVignetteInner;

        [FieldOffset(64)]
        public float BloomIntensity;
        [FieldOffset(68)]
        public float BloomForce;
        [FieldOffset(72)]
        public float BloomDirections;
        [FieldOffset(76)]
        public float BloomQuality;
        [FieldOffset(80)]
        public float BloomSize;

        [FieldOffset(84)]
        public float ToneMappingIntensity;
        [FieldOffset(88)]
        public uint ToneMappingTone;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerPass));
        }
    }
}
