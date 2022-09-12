using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Foliage
{
    using Engine.Common;

    /// <summary>
    /// Foliage drawer
    /// </summary>
    public class BuiltInFoliage : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per material data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerMaterial : IBufferData
        {
            public static PerMaterial Build(BuiltInFoliageState state)
            {
                return new PerMaterial
                {
                    TintColor = state.TintColor,

                    MaterialIndex = state.MaterialIndex,
                    TextureCount = state.TextureCount,
                    NormalMapCount = state.NormalMapCount,

                    StartRadius = state.StartRadius,
                    EndRadius = state.EndRadius,
                };
            }

            /// <summary>
            /// Tint color
            /// </summary>
            [FieldOffset(0)]
            public Color4 TintColor;

            /// <summary>
            /// Material index
            /// </summary>
            [FieldOffset(16)]
            public uint MaterialIndex;
            /// <summary>
            /// Texture count
            /// </summary>
            [FieldOffset(20)]
            public uint TextureCount;
            /// <summary>
            /// Normal map count
            /// </summary>
            [FieldOffset(24)]
            public uint NormalMapCount;

            /// <summary>
            /// Rotation
            /// </summary>
            [FieldOffset(32)]
            public float StartRadius;
            /// <summary>
            /// Texture count
            /// </summary>
            [FieldOffset(36)]
            public float EndRadius;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerMaterial));
            }
        }
        /// <summary>
        /// Per patch data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerPatch : IBufferData
        {
            public static PerPatch Build(BuiltInFoliageState state)
            {
                return new PerPatch
                {
                    WindDirection = state.WindDirection,
                    WindStrength = state.WindStrength,

                    Delta = state.Delta,
                    WindEffect = state.WindEffect,
                };
            }

            /// <summary>
            /// Wind direction
            /// </summary>
            [FieldOffset(0)]
            public Vector3 WindDirection;
            /// <summary>
            /// Wind strength
            /// </summary>
            [FieldOffset(12)]
            public float WindStrength;

            /// <summary>
            /// Delta
            /// </summary>
            [FieldOffset(16)]
            public Vector3 Delta;
            /// <summary>
            /// Wind effect
            /// </summary>
            [FieldOffset(28)]
            public float WindEffect;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerPatch));
            }
        }

        #endregion

        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterial> cbPerMaterial;
        /// <summary>
        /// Per patch constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerPatch> cbPerPatch;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInFoliage(Graphics graphics) : base(graphics)
        {
            SetVertexShader<FoliageVs>();
            SetGeometryShader<FoliageGS>();
            SetPixelShader<FoliagePs>();

            cbPerMaterial = BuiltInShaders.GetConstantBuffer<PerMaterial>();
            cbPerPatch = BuiltInShaders.GetConstantBuffer<PerPatch>();
        }

        /// <summary>
        /// Updates the foliage drawer
        /// </summary>
        /// <param name="state">Billboard state</param>
        public void UpdateFoliage(BuiltInFoliageState state)
        {
            cbPerMaterial.WriteData(PerMaterial.Build(state));
            cbPerPatch.WriteData(PerPatch.Build(state));

            var vertexShader = GetVertexShader<FoliageVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);

            var geometryShader = GetGeometryShader<FoliageGS>();
            geometryShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
            geometryShader?.SetPerPatchConstantBuffer(cbPerPatch);
            geometryShader?.SetRandomTexture(state.RandomTexture);

            var pixelShader = GetPixelShader<FoliagePs>();
            pixelShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
            pixelShader?.SetTextureArray(state.Texture);
            pixelShader?.SetNormalMapArray(state.NormalMaps);
        }
    }
}
