using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.SkyScattering
{
    using Engine.Common;

    /// <summary>
    /// Sky scattering drawer
    /// </summary>
    public class BuiltInSkyScattering : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per object data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 112)]
        struct PerObject : IBufferData
        {
            public static PerObject Build(Vector3 lightDirection, BuiltInSkyScatteringState state)
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
            public readonly int GetStride()
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
        public BuiltInSkyScattering() : base()
        {
            SetVertexShader<SkyScatteringVs>();
            SetPixelShader<SkyScatteringPs>();

            cbPerObject = BuiltInShaders.GetConstantBuffer<PerObject>();
        }

        /// <summary>
        /// Updates the texture
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="state">State</param>
        public void Update(IEngineDeviceContext dc, Vector3 lightDirection, BuiltInSkyScatteringState state)
        {
            cbPerObject.WriteData(PerObject.Build(lightDirection, state));
            dc.UpdateConstantBuffer(cbPerObject);

            var vertexShader = GetVertexShader<SkyScatteringVs>();
            vertexShader?.SetPerObjectConstantBuffer(cbPerObject);

            var pixelShader = GetPixelShader<SkyScatteringPs>();
            pixelShader?.SetPerObjectConstantBuffer(cbPerObject);
        }
    }
}
