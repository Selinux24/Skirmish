﻿using SharpDX;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Billboard
{
    using Engine.Common;

    /// <summary>
    /// Billboards drawer
    /// </summary>
    public class BuiltInBillboards : BuiltInDrawer
    {
        #region Buffers

        /// <summary>
        /// Per billboard data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerBillboard : IBufferData
        {
            public static PerBillboard Build(BuiltInBillboardState state)
            {
                return new PerBillboard
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
                return Marshal.SizeOf(typeof(PerBillboard));
            }
        }

        #endregion

        /// <summary>
        /// Per decal constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerBillboard> cbPerBillboard;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInBillboards(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BillboardVs>();
            SetGeometryShader<BillboardGS>();
            SetPixelShader<BillboardPs>();

            cbPerBillboard = BuiltInShaders.GetConstantBuffer<PerBillboard>();
        }

        /// <summary>
        /// Updates the billboard drawer
        /// </summary>
        /// <param name="state">Billboard state</param>
        public void UpdateBillboard(BuiltInBillboardState state)
        {
            cbPerBillboard.WriteData(PerBillboard.Build(state));

            var vertexShader = GetVertexShader<BillboardVs>();
            vertexShader?.SetPerBillboardConstantBuffer(cbPerBillboard);

            var geometryShader = GetGeometryShader<BillboardGS>();
            geometryShader?.SetPerBillboardConstantBuffer(cbPerBillboard);

            var pixelShader = GetPixelShader<BillboardPs>();
            pixelShader?.SetPerBillboardConstantBuffer(cbPerBillboard);
            pixelShader?.SetRandomTexture(state.RandomTexture);
            pixelShader?.SetTextureArray(state.Texture);
            pixelShader?.SetNormalMapArray(state.NormalMaps);
        }
    }
}