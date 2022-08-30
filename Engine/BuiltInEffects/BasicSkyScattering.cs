using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.SkyScattering;
    using Engine.Common;

    /// <summary>
    /// Sky scattering drawer
    /// </summary>
    public class BasicSkyScattering : BuiltInDrawer, IDisposable
    {
        #region Buffers

        /// <summary>
        /// Per object data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 112)]
        struct PerObject : IBufferData
        {
            public static PerObject Build(Vector3 lightDirection, BasicSkyScatteringState state)
            {
                return new PerObject
                {
                    SphereRadii = new Vector4(
                        state.SphereOuterRadius, state.SphereOuterRadius * state.SphereOuterRadius,
                        state.SphereInnerRadius, state.SphereInnerRadius * state.SphereInnerRadius),

                    ScatteringCoefficients = new Vector4(
                        state.RayleighScattering * state.SkyBrightness, state.RayleighScattering4PI,
                        state.MieScattering * state.SkyBrightness, state.MieScattering4PI),

                    InverseWaveLength = new Vector4(state.InvWaveLength4, 1f),

                    Misc = new Vector4(state.PlanetRadius, state.PlanetAtmosphereRadius, state.Scale, state.Scale / state.RayleighScaleDepth),

                    BackColor = state.BackColor,

                    LightDirection = -lightDirection,
                    HDRExposure = state.HdrExposure,

                    Samples = state.Samples,
                };
            }

            /// <summary>
            /// Sphere radii
            /// </summary>
            [FieldOffset(0)]
            public Vector4 SphereRadii;

            /// <summary>
            /// Scattering coefficients
            /// </summary>
            [FieldOffset(16)]
            public Vector4 ScatteringCoefficients;

            /// <summary>
            /// Inverse wave length
            /// </summary>
            [FieldOffset(32)]
            public Vector4 InverseWaveLength;

            /// <summary>
            /// Miscelanea
            /// </summary>
            [FieldOffset(48)]
            public Vector4 Misc;

            /// <summary>
            /// Backcolor
            /// </summary>
            [FieldOffset(64)]
            public Color4 BackColor;

            /// <summary>
            /// Light direction
            /// </summary>
            [FieldOffset(80)]
            public Vector3 LightDirection;
            /// <summary>
            /// HDR exposure
            /// </summary>
            [FieldOffset(92)]
            public float HDRExposure;

            /// <summary>
            /// Number of samples
            /// </summary>
            [FieldOffset(96)]
            public uint Samples;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerObject));
            }
        }

        #endregion

        /// <summary>
        /// Per object constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerObject> cbPerObject;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicSkyScattering(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkyScatteringVs>();
            SetPixelShader<SkyScatteringPs>();

            cbPerObject = new EngineConstantBuffer<PerObject>(graphics, nameof(BasicSkyScattering) + "." + nameof(PerObject));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicSkyScattering()
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
                cbPerObject?.Dispose();
            }
        }

        /// <summary>
        /// Updates the texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void Update(Vector3 lightDirection, BasicSkyScatteringState state)
        {
            cbPerObject.WriteData(PerObject.Build(lightDirection, state));

            var vertexShader = GetVertexShader<SkyScatteringVs>();
            vertexShader?.SetPerObjectConstantBuffer(cbPerObject);

            var pixelShader = GetPixelShader<SkyScatteringPs>();
            pixelShader?.SetPerObjectConstantBuffer(cbPerObject);
        }
    }
}
