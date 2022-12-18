using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Foliage
{
    /// <summary>
    /// Per material data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    struct PerMaterial : IBufferData
    {
        public static PerMaterial Build(BuiltInFoliageState state)
        {
            return new PerMaterial
            {
                TintColor = state.TintColor,

                MaterialIndex = state.MaterialIndex,
                TextureCount = state.TextureCount,
                NormalMapCount = state.NormalMapCount,
            };
        }

        /// <summary>
        /// Tint color
        /// </summary>
        [FieldOffset(0)]
        public Color4 TintColor;

        /// <summary>
        /// Material index
        /// </summary>
        [FieldOffset(16)]
        public uint MaterialIndex;
        /// <summary>
        /// Texture count
        /// </summary>
        [FieldOffset(20)]
        public uint TextureCount;
        /// <summary>
        /// Normal map count
        /// </summary>
        [FieldOffset(24)]
        public uint NormalMapCount;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMaterial));
        }
    }

    /// <summary>
    /// Per patch data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    struct PerPatch : IBufferData
    {
        public static PerPatch Build(BuiltInFoliageState state)
        {
            return new PerPatch
            {
                WindDirection = state.WindDirection,
                WindStrength = state.WindStrength,

                StartRadius = state.StartRadius,
                EndRadius = state.EndRadius,
                Instances = (uint)state.Instances,

                Delta = state.Delta,
                WindEffect = state.WindEffect,
            };
        }

        /// <summary>
        /// Wind direction
        /// </summary>
        [FieldOffset(0)]
        public Vector3 WindDirection;
        /// <summary>
        /// Wind strength
        /// </summary>
        [FieldOffset(12)]
        public float WindStrength;

        /// <summary>
        /// Rotation
        /// </summary>
        [FieldOffset(16)]
        public float StartRadius;
        /// <summary>
        /// Texture count
        /// </summary>
        [FieldOffset(20)]
        public float EndRadius;
        /// <summary>
        /// Instance count
        /// </summary>
        [FieldOffset(24)]
        public uint Instances;

        /// <summary>
        /// Position delta for additional instances
        /// </summary>
        [FieldOffset(32)]
        public Vector3 Delta;
        /// <summary>
        /// Wind effect
        /// </summary>
        [FieldOffset(44)]
        public float WindEffect;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerPatch));
        }
    }
}
