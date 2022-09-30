using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.PostProcess
{
    /// <summary>
    /// Per pass data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
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

                EffectIntensity = state.EffectIntensity,

                BlurDirections = state.BlurDirections,
                BlurQuality = state.BlurQuality,
                BlurSize = state.BlurSize,

                VignetteOuter = state.VignetteOuter,
                VignetteInner = state.VignetteInner,

                BloomIntensity = state.BloomIntensity,

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
        public float EffectIntensity;
        [FieldOffset(20)]
        public float BlurDirections;
        [FieldOffset(24)]
        public float BlurQuality;
        [FieldOffset(28)]
        public float BlurSize;

        [FieldOffset(32)]
        public float VignetteOuter;
        [FieldOffset(36)]
        public float VignetteInner;
        [FieldOffset(40)]
        public float BloomIntensity;
        [FieldOffset(44)]
        public uint ToneMappingTone;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerPass));
        }
    }
}
