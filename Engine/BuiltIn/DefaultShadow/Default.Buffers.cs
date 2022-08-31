using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.DefaultShadow
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
    /// Per material texture data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    struct PerMaterialTexture : IBufferData
    {
        public static PerMaterialTexture Build(BuiltInDrawerMaterialState state)
        {
            return new PerMaterialTexture
            {
                TextureIndex = state.TextureIndex,
            };
        }

        /// <summary>
        /// Texture index
        /// </summary>
        [FieldOffset(0)]
        public uint TextureIndex;

        /// <inheritdoc/>
        public int GetStride()
        {
            return Marshal.SizeOf(typeof(PerMaterialTexture));
        }
    }
}
