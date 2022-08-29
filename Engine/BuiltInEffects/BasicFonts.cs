using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Fonts;
    using Engine.Common;

    /// <summary>
    /// Fonts drawer
    /// </summary>
    public class BasicFonts : BuiltInDrawer, IDisposable
    {
        #region Buffers

        /// <summary>
        /// Per font data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerFont : IBufferData
        {
            public static PerFont Build(float alpha, bool useColor, bool useRectangle, bool fineSampling, Rectangle rectangle, Vector2 resolution)
            {
                return new PerFont
                {
                    Alpha = alpha,
                    UseColor = useColor,
                    UseRectangle = useRectangle,
                    FineSampling = fineSampling,

                    Rectangle = new Vector4(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height),

                    Resolution = resolution,
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

            /// <summary>
            /// Screen resolution
            /// </summary>
            [FieldOffset(32)]
            public Vector2 Resolution;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFont));
            }
        }

        #endregion

        /// <summary>
        /// Per font constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFont> cbPerFont;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicFonts(Graphics graphics) : base(graphics)
        {
            SetVertexShader<FontsVs>();
            SetPixelShader<FontsPs>();

            cbPerFont = new EngineConstantBuffer<PerFont>(graphics, nameof(BasicFonts) + "." + nameof(PerFont));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicFonts()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cbPerFont?.Dispose();
            }
        }

        /// <summary>
        /// Updates the font drawer state
        /// </summary>
        public void Update(float alpha, bool useColor, bool useRectangle, bool fineSampling, Rectangle rectangle, Vector2 resolution, EngineShaderResourceView texture)
        {
            cbPerFont.WriteData(PerFont.Build(alpha, useColor, useRectangle, fineSampling, rectangle, resolution));

            var pixelShader = GetPixelShader<FontsPs>();
            pixelShader?.SetPerFontConstantBuffer(cbPerFont);
            pixelShader?.SetTextureArray(texture);
        }
    }
}
