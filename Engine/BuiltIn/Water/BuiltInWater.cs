using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Water
{
    using Engine.Common;

    /// <summary>
    /// Water drawer
    /// </summary>
    public class BuiltInWater : BuiltInDrawer, IDisposable
    {
        #region Buffers

        /// <summary>
        /// Per water data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerWater : IBufferData
        {
            public static PerWater Build(BuiltInWaterState state)
            {
                return new PerWater
                {
                    BaseColor = state.BaseColor,
                    WaterColor = state.WaterColor.RGB(),
                    WaterAlpha = state.WaterColor.Alpha,
                    WaveParams = new Vector4(state.WaveHeight, state.WaveChoppy, state.WaveSpeed, state.WaveFrequency),
                    IterParams = new Int3(state.Steps, state.GeometryIterations, state.ColorIterations),
                };
            }

            /// <summary>
            /// Wave parameters
            /// </summary>
            [FieldOffset(0)]
            public Vector4 WaveParams;

            /// <summary>
            /// Water color
            /// </summary>
            [FieldOffset(16)]
            public Color3 WaterColor;
            /// <summary>
            /// Water alpha
            /// </summary>
            [FieldOffset(28)]
            public float WaterAlpha;

            /// <summary>
            /// Base color
            /// </summary>
            [FieldOffset(32)]
            public Color3 BaseColor;

            /// <summary>
            /// Iterator
            /// </summary>
            [FieldOffset(48)]
            public Int3 IterParams;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerWater));
            }
        }

        #endregion

        /// <summary>
        /// Per water constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerWater> cbPerFont;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInWater(Graphics graphics) : base(graphics)
        {
            SetVertexShader<WaterVs>();
            SetPixelShader<WaterPs>();

            cbPerFont = new EngineConstantBuffer<PerWater>(graphics, nameof(BuiltInWater) + "." + nameof(PerWater));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BuiltInWater()
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
        /// Updates the water drawer state
        /// </summary>
        /// <param name="state">Drawer state</param>
        public void UpdateWater(BuiltInWaterState state)
        {
            cbPerFont.WriteData(PerWater.Build(state));

            var pixelShader = GetPixelShader<WaterPs>();
            pixelShader?.SetPerWaterConstantBuffer(cbPerFont);
        }
    }
}
