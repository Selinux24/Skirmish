using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Sprites
{
    /// <summary>
    /// Per sprite data structure
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 192)]
    struct PerSprite : IBufferData
    {
        public static PerSprite Build(BuiltInSpriteState state)
        {
            return new PerSprite
            {
                Local = Matrix.Transpose(state.Local),
                Size = new Vector4(state.RenderArea.X, state.RenderArea.Y, state.RenderArea.Width, state.RenderArea.Height),
                Color1 = state.Color1,
                Color2 = state.Color2,
                Color3 = state.Color3,
                Color4 = state.Color4,
                UsePercentage = state.UsePercentage,
                Percentage = new Vector3(state.Percentage1, state.Percentage2, state.Percentage3),
                Direction = state.Direction,
                TextureIndex = state.TextureIndex,
                Channel = (uint)state.Channel,
                UseRect = state.UseRect,
                ClippingRecangle = new Vector4(state.ClippingRecangle.X, state.ClippingRecangle.Y, state.ClippingRecangle.Width, state.ClippingRecangle.Height),
            };
        }

        /// <summary>
        /// Local transform
        /// </summary>
        [FieldOffset(0)]
        public Matrix Local;

        /// <summary>
        /// Sprite size
        /// </summary>
        [FieldOffset(64)]
        public Vector4 Size;

        /// <summary>
        /// Color 1
        /// </summary>
        [FieldOffset(80)]
        public Color4 Color1;
        /// <summary>
        /// Color 2
        /// </summary>
        [FieldOffset(96)]
        public Color4 Color2;
        /// <summary>
        /// Color 3
        /// </summary>
        [FieldOffset(112)]
        public Color4 Color3;
        /// <summary>
        /// Color 4
        /// </summary>
        [FieldOffset(128)]
        public Color4 Color4;

        /// <summary>
        /// Use percent values
        /// </summary>
        [FieldOffset(144)]
        public bool UsePercentage;
        /// <summary>
        /// Percent values
        /// </summary>
        [FieldOffset(148)]
        public Vector3 Percentage;

        /// <summary>
        /// Percent directio
        /// </summary>
        [FieldOffset(160)]
        public uint Direction;
        /// <summary>
        /// Color channel
        /// </summary>
        [FieldOffset(164)]
        public uint Channel;
        /// <summary>
        /// Texture index
        /// </summary>
        [FieldOffset(168)]
        public uint TextureIndex;
        /// <summary>
        /// Use clipping rectangle
        /// </summary>
        [FieldOffset(172)]
        public bool UseRect;

        /// <summary>
        /// Render area
        /// </summary>
        [FieldOffset(176)]
        public Vector4 ClippingRecangle;

        /// <inheritdoc/>
        public readonly int GetStride()
        {
            return Marshal.SizeOf(typeof(PerSprite));
        }
    }
}
