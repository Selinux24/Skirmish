using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-color drawer
    /// </summary>
    public class BasicPositionColorSkinned : BuiltInDrawer, IDisposable
    {
        #region Buffers

        /// <summary>
        /// Per mesh data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 80)]
        struct PerMesh : IBufferData
        {
            public static PerMesh Build(BuiltInDrawerMeshState state)
            {
                return new PerMesh
                {
                    Local = state.Local,
                };
            }

            /// <summary>
            /// Local transform
            /// </summary>
            [FieldOffset(0)]
            public Matrix Local;

            /// <summary>
            /// Animation offset 1
            /// </summary>
            [FieldOffset(64)]
            public uint AnimationOffset;
            /// <summary>
            /// Animation offset 2
            /// </summary>
            [FieldOffset(68)]
            public uint AnimationOffset2;
            /// <summary>
            /// Animation interpolation value
            /// </summary>
            [FieldOffset(72)]
            public float AnimationInterpolation;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerMesh));
            }
        }

        /// <summary>
        /// Per mesh data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerMaterial : IBufferData
        {
            public static PerMaterial Build(BuiltInDrawerMaterialState state)
            {
                return new PerMaterial
                {
                    TintColor = state.TintColor,
                    MaterialIndex = state.Material.Material?.ResourceIndex ?? 0,
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

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerMaterial));
            }
        }

        #endregion

        /// <summary>
        /// Per mesh constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMesh> cbPerMesh;
        /// <summary>
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterial> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColorSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorSkinnedVs>();
            SetPixelShader<PositionColorPs>();

            cbPerMesh = new EngineConstantBuffer<PerMesh>(graphics, nameof(BasicPositionColorSkinned) + "." + nameof(PerMesh));
            cbPerMaterial = new EngineConstantBuffer<PerMaterial>(graphics, nameof(BasicPositionColorSkinned) + "." + nameof(PerMaterial));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionColorSkinned()
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
                cbPerMesh?.Dispose();
                cbPerMaterial?.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void UpdateMesh(BuiltInDrawerMeshState state)
        {
            cbPerMesh.WriteData(PerMesh.Build(state));

            var vertexShader = GetVertexShader<PositionColorSkinnedVs>();
            vertexShader?.SetPerMeshConstantBuffer(cbPerMesh);
        }
        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterial.Build(state));

            var vertexShader = GetVertexShader<PositionColorSkinnedVs>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
