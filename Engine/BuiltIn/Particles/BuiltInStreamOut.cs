using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Particles
{
    using Engine.Common;

    /// <summary>
    /// Stream-out drawer
    /// </summary>
    public class BuiltInStreamOut : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per stream-out data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerStreamOut : IBufferData
        {
            public static PerStreamOut Build(BuiltInStreamOutState state)
            {
                return new PerStreamOut
                {
                    EmissionRate = state.EmissionRate,
                    VelocitySensitivity = state.VelocitySensitivity,
                    TotalTime = state.TotalTime,
                    ElapsedTime = state.ElapsedTime,
                    HorizontalVelocity = state.HorizontalVelocity,
                    VerticalVelocity = state.VerticalVelocity,
                    RandomValues = state.RandomValues,
                };
            }

            /// <summary>
            /// Emission rate
            /// </summary>
            [FieldOffset(0)]
            public float EmissionRate;
            /// <summary>
            /// Velocity sensitivity
            /// </summary>
            [FieldOffset(4)]
            public float VelocitySensitivity;
            /// <summary>
            /// Total particle time (not game time)
            /// </summary>
            [FieldOffset(8)]
            public float TotalTime;
            /// <summary>
            /// Elapsed particle time (not game time)
            /// </summary>
            [FieldOffset(12)]
            public float ElapsedTime;

            /// <summary>
            /// Horizontal velocity
            /// </summary>
            [FieldOffset(16)]
            public Vector2 HorizontalVelocity;
            /// <summary>
            /// Vertical velocity
            /// </summary>
            [FieldOffset(24)]
            public Vector2 VerticalVelocity;

            /// <summary>
            /// Random values
            /// </summary>
            [FieldOffset(32)]
            public Vector4 RandomValues;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerStreamOut));
            }
        }

        #endregion

        /// <summary>
        /// Per stream-out pass constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerStreamOut> cbPerStreamOut;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInStreamOut(Game game) : base(game)
        {
            SetVertexShader<StreamOutVs>();
            SetGeometryShader<StreamOutGS>();

            cbPerStreamOut = BuiltInShaders.GetConstantBuffer<PerStreamOut>();
        }

        /// <summary>
        /// Updates the particle drawer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Particle state</param>
        public void Update(IEngineDeviceContext dc, BuiltInStreamOutState state)
        {
            dc.UpdateConstantBuffer(cbPerStreamOut, PerStreamOut.Build(state));

            var geometryShader = GetGeometryShader<StreamOutGS>();
            geometryShader?.SetPerStreamOutConstantBuffer(cbPerStreamOut);
        }
    }
}
