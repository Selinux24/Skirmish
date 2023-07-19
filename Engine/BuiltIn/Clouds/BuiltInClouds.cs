using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Clouds
{
    using Engine.Common;

    /// <summary>
    /// Clouds drawer
    /// </summary>
    public class BuiltInClouds : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per cloud data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerCloud : IBufferData
        {
            public static PerCloud Build(BuiltInCloudsState state)
            {
                return new PerCloud
                {
                    Perturbed = state.Perturbed,
                    Translation = state.Translation,
                    Scale = state.Scale,
                    FadingDistance = state.FadingDistance,
                    FirstTranslation = state.FirstTranslation,
                    SecondTranslation = state.SecondTranslation,
                    Color = state.Color,
                    Brightness = state.Brightness,
                };
            }

            /// <summary>
            /// Perturbed clouds
            /// </summary>
            [FieldOffset(0)]
            public bool Perturbed;
            /// <summary>
            /// Translation velocity
            /// </summary>
            [FieldOffset(4)]
            public float Translation;
            /// <summary>
            /// Scale
            /// </summary>
            [FieldOffset(8)]
            public float Scale;
            /// <summary>
            /// Fadding distance
            /// </summary>
            [FieldOffset(12)]
            public float FadingDistance;

            /// <summary>
            /// First layer translation velocity
            /// </summary>
            [FieldOffset(16)]
            public Vector2 FirstTranslation;
            /// <summary>
            /// Second layer translation velocity
            /// </summary>
            [FieldOffset(24)]
            public Vector2 SecondTranslation;

            /// <summary>
            /// Color
            /// </summary>
            [FieldOffset(32)]
            public Color3 Color;
            /// <summary>
            /// Brightness
            /// </summary>
            [FieldOffset(44)]
            public float Brightness;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerCloud));
            }
        }

        #endregion

        /// <summary>
        /// Per cloud constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerCloud> cbPerCloud;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInClouds(Graphics graphics) : base(graphics)
        {
            SetVertexShader<CloudsVs>();
            SetPixelShader<CloudsPs>();

            cbPerCloud = BuiltInShaders.GetConstantBuffer<PerCloud>();
        }

        /// <summary>
        /// Updates the cloud drawer state
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="state">Drawer state</param>
        public void UpdateClouds(EngineDeviceContext dc, BuiltInCloudsState state)
        {
            cbPerCloud.WriteData(PerCloud.Build(state));
            dc.UpdateConstantBuffer(cbPerCloud);

            var pixelShader = GetPixelShader<CloudsPs>();
            pixelShader?.SetPerCloudConstantBuffer(cbPerCloud);
            pixelShader?.SetFirstCloudLayer(state.Clouds1);
            pixelShader?.SetSecondCloudLayer(state.Clouds2);
        }
    }
}
