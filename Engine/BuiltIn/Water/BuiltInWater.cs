using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Water
{
    using Engine.Common;

    /// <summary>
    /// Water drawer
    /// </summary>
    public class BuiltInWater : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per water data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 64)]
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
            /// Wave height
            /// </summary>
            [FieldOffset(0)]
            public float WaveHeight;
            /// <summary>
            /// Wave choppy
            /// </summary>
            [FieldOffset(4)]
            public float WaveChoppy;
            /// <summary>
            /// Wave speed
            /// </summary>
            [FieldOffset(8)]
            public float WaveSpeed;
            /// <summary>
            /// Wave frequency
            /// </summary>
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
            public readonly int GetStride()
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
        public BuiltInWater() : base()
        {
            SetVertexShader<WaterVs>();
            SetPixelShader<WaterPs>();

            cbPerWater = BuiltInShaders.GetConstantBuffer<PerWater>();
        }

        /// <summary>
        /// Updates the water drawer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Drawer state</param>
        public void UpdateWater(IEngineDeviceContext dc, BuiltInWaterState state)
        {
            dc.UpdateConstantBuffer(cbPerWater, PerWater.Build(state));

            var pixelShader = GetPixelShader<WaterPs>();
            pixelShader?.SetPerWaterConstantBuffer(cbPerWater);
        }
    }
}
