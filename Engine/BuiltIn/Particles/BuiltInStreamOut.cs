﻿using SharpDX;
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
            public int GetStride()
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
        /// <param name="graphics">Graphics</param>
        public BuiltInStreamOut(Graphics graphics) : base(graphics)
        {
            SetVertexShader<StreamOutVs>();
            SetGeometryShader<StreamOutGS>();

            cbPerStreamOut = BuiltInShaders.GetConstantBuffer<PerStreamOut>();
        }

        /// <summary>
        /// Updates the particle drawer
        /// </summary>
        /// <param name="state">Particle state</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture array</param>
        public void Update(BuiltInStreamOutState state)
        {
            cbPerStreamOut.WriteData(PerStreamOut.Build(state));

            var geometryShader = GetGeometryShader<StreamOutGS>();
            geometryShader?.SetPerStreamOutConstantBuffer(cbPerStreamOut);
        }
    }
}
