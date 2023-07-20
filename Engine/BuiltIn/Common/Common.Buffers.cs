using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Common
{
    /// <summary>
    /// Per mesh single data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    struct PerMeshSingle : IBufferData
    {
        public static PerMeshSingle Build(BuiltInDrawerMeshState state)
        {
            return new PerMeshSingle
            {
                Local = Matrix.Transpose(state.Local),
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix Local;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMeshSingle));
        }
    }

    /// <summary>
    /// Per mesh skinned data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    struct PerMeshSkinned : IBufferData
    {
        public static PerMeshSkinned Build(BuiltInDrawerMeshState state)
        {
            return new PerMeshSkinned
            {
                Local = Matrix.Transpose(state.Local),
                AnimationOffset = state.AnimationOffset1,
                AnimationOffset2 = state.AnimationOffset2,
                AnimationInterpolation = state.AnimationInterpolationAmount,
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix Local;

        /// <summary>
        /// Animation offset 1
        /// </summary>
        [FieldOffset(64)]
        public uint AnimationOffset;
        /// <summary>
        /// Animation offset 2
        /// </summary>
        [FieldOffset(68)]
        public uint AnimationOffset2;
        /// <summary>
        /// Animation interpolation value
        /// </summary>
        [FieldOffset(72)]
        public float AnimationInterpolation;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMeshSkinned));
        }
    }

    /// <summary>
    /// Per material color data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    struct PerMaterialColor : IBufferData
    {
        public static PerMaterialColor Build(BuiltInDrawerMaterialState state)
        {
            return new PerMaterialColor
            {
                TintColor = state.TintColor,
                MaterialIndex = state.Material?.ResourceIndex ?? 0,
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

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMaterialColor));
        }
    }

    /// <summary>
    /// Per material texture data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    struct PerMaterialTexture : IBufferData
    {
        public static PerMaterialTexture Build(BuiltInDrawerMaterialState state)
        {
            return new PerMaterialTexture
            {
                TintColor = state.TintColor,
                MaterialIndex = state.Material?.ResourceIndex ?? 0,
                TextureIndex = state.TextureIndex,
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
        /// Texture index
        /// </summary>
        [FieldOffset(20)]
        public uint TextureIndex;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMaterialTexture));
        }
    }

    /// <summary>
    /// Per frame position texture data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    struct PerFramePositionTexture : IBufferData
    {
        public static PerFramePositionTexture Build(uint channel)
        {
            return new PerFramePositionTexture
            {
                Channel = channel,
            };
        }

        /// <summary>
        /// Color output channel
        /// </summary>
        [FieldOffset(0)]
        public uint Channel;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerFramePositionTexture));
        }
    }

    /// <summary>
    /// Per terrain data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    struct PerTerrain : IBufferData
    {
        public static PerTerrain Build(BuiltInTerrainState state)
        {
            return new PerTerrain
            {
                TintColor = state.TintColor,
                MaterialIndex = state.MaterialIndex,
                Mode = (uint)state.Mode,
                TextureResolution = state.TextureResolution,
                Proportion = state.Proportion,
                Slope1 = state.SlopeRanges.X,
                Slope2 = state.SlopeRanges.Y,
            };
        }

        /// <summary>
        /// Tint color
        /// </summary>
        [FieldOffset(0)]
        public Color4 TintColor;

        /// <summary>
        /// Scattering coefficients
        /// </summary>
        [FieldOffset(16)]
        public uint MaterialIndex;
        /// <summary>
        /// Render mode
        /// </summary>
        [FieldOffset(20)]
        public uint Mode;

        /// <summary>
        /// Close texture resolution
        /// </summary>
        [FieldOffset(32)]
        public float TextureResolution;
        /// <summary>
        /// Proportion between alpha mapping and sloped terrain
        /// </summary>
        [FieldOffset(36)]
        public float Proportion;
        /// <summary>
        /// Slope 1 height
        /// </summary>
        [FieldOffset(40)]
        public float Slope1;
        /// <summary>
        /// Slope 2 height
        /// </summary>
        [FieldOffset(44)]
        public float Slope2;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerTerrain));
        }
    }
}
