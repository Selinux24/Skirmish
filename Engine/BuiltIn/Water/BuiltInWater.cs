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
                    WaveHeight = state.WaveHeight,
                    WaveChoppy = state.WaveChoppy,
                    WaveSpeed = state.WaveSpeed,
                    WaveFrequency = state.WaveFrequency,

                    WaterColor = state.WaterColor,

                    BaseColor = state.BaseColor,

                    Steps = state.Steps,
                    GeometryIterations = state.GeometryIterations,
                    ColorIterations = state.ColorIterations,
                };
            }

            /// <summary>
            /// Wave parameters
            /// </summary>
            [FieldOffset(0)]
            public float WaveHeight;
            [FieldOffset(4)]
            public float WaveChoppy;
            [FieldOffset(8)]
            public float WaveSpeed;
            [FieldOffset(12)]
            public float WaveFrequency;

            /// <summary>
            /// Water color
            /// </summary>
            [FieldOffset(16)]
            public Color4 WaterColor;

            /// <summary>
            /// Base color
            /// </summary>
            [FieldOffset(32)]
            public Color3 BaseColor;

            /// <summary>
            /// Steps
            /// </summary>
            [FieldOffset(48)]
            public uint Steps;
            /// <summary>
            /// Geometry iterations
            /// </summary>
            [FieldOffset(52)]
            public uint GeometryIterations;
            /// <summary>
            /// Color iterations
            /// </summary>
            [FieldOffset(56)]
            public uint ColorIterations;

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
        private readonly EngineConstantBuffer<PerWater> cbPerWater;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInWater(Graphics graphics) : base(graphics)
        {
            SetVertexShader<WaterVs>();
            SetPixelShader<WaterPs>();

            cbPerWater = new EngineConstantBuffer<PerWater>(graphics, nameof(BuiltInWater) + "." + nameof(PerWater));
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
                cbPerWater?.Dispose();
            }
        }

        /// <summary>
        /// Updates the water drawer state
        /// </summary>
        /// <param name="state">Drawer state</param>
        public void UpdateWater(BuiltInWaterState state)
        {
            cbPerWater.WriteData(PerWater.Build(state));

            var pixelShader = GetPixelShader<WaterPs>();
            pixelShader?.SetPerWaterConstantBuffer(cbPerWater);
        }
    }
}
