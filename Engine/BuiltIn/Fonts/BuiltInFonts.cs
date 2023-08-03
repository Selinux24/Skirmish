using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Fonts
{
    using Engine.BuiltIn.Foliage;
    using Engine.Common;

    /// <summary>
    /// Fonts drawer
    /// </summary>
    public class BuiltInFonts : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per font data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerFont : IBufferData
        {
            public static PerFont Build(BuiltInFontState state)
            {
                return new PerFont
                {
                    Alpha = state.Alpha,
                    UseColor = state.UseColor,
                    UseRectangle = state.UseRectangle,
                    FineSampling = state.FineSampling,

                    Rectangle = new Vector4(state.ClippingRectangle.X, state.ClippingRectangle.Y, state.ClippingRectangle.Width, state.ClippingRectangle.Height),
                };
            }

            /// <summary>
            /// Alpha value
            /// </summary>
            [FieldOffset(0)]
            public float Alpha;
            /// <summary>
            /// Use font color
            /// </summary>
            [FieldOffset(4)]
            public bool UseColor;
            /// <summary>
            /// Use rectangle
            /// </summary>
            [FieldOffset(8)]
            public bool UseRectangle;
            /// <summary>
            /// Fine sampling
            /// </summary>
            [FieldOffset(12)]
            public bool FineSampling;

            /// <summary>
            /// Clipping rectangle
            /// </summary>
            [FieldOffset(16)]
            public Vector4 Rectangle;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFont));
            }
        }
        /// <summary>
        /// Per font data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 64)]
        struct PerText : IBufferData
        {
            public static PerText Build(Matrix local)
            {
                return new PerText
                {
                    Local = Matrix.Transpose(local),
                };
            }

            /// <summary>
            /// Local matrix
            /// </summary>
            [FieldOffset(0)]
            public Matrix Local;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerText));
            }
        }

        #endregion

        /// <summary>
        /// Per font constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFont> cbPerFont;
        /// <summary>
        /// Per text constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerText> cbPerText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInFonts(Graphics graphics) : base(graphics)
        {
            SetVertexShader<FontsVs>();
            SetPixelShader<FontsPs>();

            cbPerFont = BuiltInShaders.GetConstantBuffer<PerFont>();
            cbPerText = BuiltInShaders.GetConstantBuffer<PerText>();
        }

        /// <summary>
        /// Updates the font drawer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Drawer state</param>
        public void UpdateFont(IEngineDeviceContext dc, BuiltInFontState state)
        {
            cbPerFont.WriteData(PerFont.Build(state));
            dc.UpdateConstantBuffer(cbPerFont);

            var pixelShader = GetPixelShader<FontsPs>();
            pixelShader?.SetPerFontConstantBuffer(cbPerFont);
            pixelShader?.SetTextureArray(state.FontTexture);
        }
        /// <summary>
        /// Updates the font drawer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="local">Local transform</param>
        public void UpdateText(IEngineDeviceContext dc, Matrix local)
        {
            cbPerText.WriteData(PerText.Build(local));
            dc.UpdateConstantBuffer(cbPerText);

            var vertexShader = GetVertexShader<FontsVs>();
            vertexShader?.SetPerTextConstantBuffer(cbPerText);
        }
    }
}
