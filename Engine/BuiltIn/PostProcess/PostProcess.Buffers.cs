using System.Runtime.InteropServices;

namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Per pass data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 96)]
    struct PerPass : IBufferData
    {
        public static PerPass Build(BuiltInPostProcessState state)
        {
            return new PerPass
            {
                Effect1 = (uint)state.Effect1,
                Effect2 = (uint)state.Effect2,
                Effect3 = (uint)state.Effect3,
                Effect4 = (uint)state.Effect4,

                Effect1Intensity = state.Effect1Intensity,
                Effect2Intensity = state.Effect2Intensity,
                Effect3Intensity = state.Effect3Intensity,
                Effect4Intensity = state.Effect4Intensity,

                BlurDirections = state.BlurDirections,
                BlurQuality = state.BlurQuality,
                BlurSize = state.BlurSize,

                VignetteOuter = state.VignetteOuter,
                VignetteInner = state.VignetteInner,

                BlurVignetteDirections = state.BlurVignetteDirections,
                BlurVignetteQuality = state.BlurVignetteQuality,
                BlurVignetteSize = state.BlurVignetteSize,
                BlurVignetteOuter = state.BlurVignetteOuter,
                BlurVignetteInner = state.BlurVignetteInner,

                BloomIntensity = state.BloomIntensity,
                BloomDirections = state.BloomDirections,
                BloomQuality = state.BloomQuality,
                BloomSize = state.BloomSize,

                ToneMappingTone = (uint)state.ToneMappingTone,
            };
        }

        [FieldOffset(0)]
        public uint Effect1;
        [FieldOffset(4)]
        public uint Effect2;
        [FieldOffset(8)]
        public uint Effect3;
        [FieldOffset(12)]
        public uint Effect4;

        [FieldOffset(16)]
        public float Effect1Intensity;
        [FieldOffset(20)]
        public float Effect2Intensity;
        [FieldOffset(24)]
        public float Effect3Intensity;
        [FieldOffset(28)]
        public float Effect4Intensity;

        [FieldOffset(32)]
        public float BlurDirections;
        [FieldOffset(36)]
        public float BlurQuality;
        [FieldOffset(40)]
        public float BlurSize;

        [FieldOffset(44)]
        public float VignetteOuter;
        [FieldOffset(48)]
        public float VignetteInner;

        [FieldOffset(52)]
        public float BlurVignetteDirections;
        [FieldOffset(56)]
        public float BlurVignetteQuality;
        [FieldOffset(60)]
        public float BlurVignetteSize;
        [FieldOffset(64)]
        public float BlurVignetteOuter;
        [FieldOffset(68)]
        public float BlurVignetteInner;

        [FieldOffset(72)]
        public float BloomIntensity;
        [FieldOffset(76)]
        public float BloomDirections;
        [FieldOffset(80)]
        public float BloomQuality;
        [FieldOffset(84)]
        public float BloomSize;

        [FieldOffset(88)]
        public uint ToneMappingTone;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerPass));
        }
    }
}
