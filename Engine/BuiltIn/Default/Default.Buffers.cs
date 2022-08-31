using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Default
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
        public int GetStride()
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
        public int GetStride()
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
        public int GetStride()
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
        public int GetStride()
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
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerFramePositionTexture));
        }
    }
}
