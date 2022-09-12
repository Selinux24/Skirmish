using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Terrain
{
    using Engine.Common;

    /// <summary>
    /// Terrain drawer
    /// </summary>
    public class BuiltInTerrain : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per terrain data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerTerrain : IBufferData
        {
            public static PerTerrain Build(BuiltInTerrainState state)
            {
                return new PerTerrain
                {
                    TintColor = state.TintColor,
                    MaterialIndex = state.MaterialIndex,
                    Mode = (uint)state.Mode,
                    TextureResolution = state.TextureResolution,
                    Proportion = state.Proportion,
                    Slope1 = state.SlopeRanges.X,
                    Slope2 = state.SlopeRanges.Y,
                };
            }

            /// <summary>
            /// Tint color
            /// </summary>
            [FieldOffset(0)]
            public Color4 TintColor;

            /// <summary>
            /// Scattering coefficients
            /// </summary>
            [FieldOffset(16)]
            public uint MaterialIndex;
            /// <summary>
            /// Render mode
            /// </summary>
            [FieldOffset(20)]
            public uint Mode;

            /// <summary>
            /// Close texture resolution
            /// </summary>
            [FieldOffset(32)]
            public float TextureResolution;
            /// <summary>
            /// Proportion between alpha mapping and sloped terrain
            /// </summary>
            [FieldOffset(36)]
            public float Proportion;
            /// <summary>
            /// Slope 1 height
            /// </summary>
            [FieldOffset(40)]
            public float Slope1;
            /// <summary>
            /// Slope 2 height
            /// </summary>
            [FieldOffset(44)]
            public float Slope2;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerTerrain));
            }
        }

        #endregion

        /// <summary>
        /// Per terrain constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerTerrain> cbPerTerrain;
        /// <summary>
        /// Linear sampler
        /// </summary>
        private readonly EngineSamplerState linear;
        /// <summary>
        /// Anisotropic sampler
        /// </summary>
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInTerrain(Graphics graphics) : base(graphics)
        {
            SetVertexShader<TerrainVs>();
            SetPixelShader<TerrainPs>();

            cbPerTerrain = BuiltInShaders.GetConstantBuffer<PerTerrain>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <summary>
        /// Updates the terrain
        /// </summary>
        /// <param name="state">Terrain state</param>
        public void Update(BuiltInTerrainState state)
        {
            cbPerTerrain.WriteData(PerTerrain.Build(state));

            var vertexShader = GetVertexShader<TerrainVs>();
            vertexShader?.SetPerTerrainConstantBuffer(cbPerTerrain);

            var pixelShader = GetPixelShader<TerrainPs>();
            pixelShader?.SetPerTerrainConstantBuffer(cbPerTerrain);
            pixelShader?.SetAlphaMap(state.AlphaMap);
            pixelShader?.SetNormalMap(state.MormalMap);
            pixelShader?.SetColorTexture(state.ColorTexture);
            pixelShader?.SetLowResolutionTexture(state.LowResolutionTexture);
            pixelShader?.SetHighResolutionTexture(state.HighResolutionTexture);
            pixelShader?.SetDiffuseSampler(state.UseAnisotropic ? anisotropic : linear);
            pixelShader?.SetNormalSampler(linear);
        }
    }
}
