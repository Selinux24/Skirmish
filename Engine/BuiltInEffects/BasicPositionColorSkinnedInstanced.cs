using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Skinned position-color instanced drawer
    /// </summary>
    public class BasicPositionColorSkinnedInstanced : BuiltInDrawer, IDisposable
    {
        #region Buffers

        /// <summary>
        /// Per material data structure
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
        /// Per material constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerMaterial> cbPerMaterial;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorSkinnedVsI>();
            SetPixelShader<PositionColorPs>();

            cbPerMaterial = new EngineConstantBuffer<PerMaterial>(graphics, nameof(BasicPositionColorSkinnedInstanced) + "." + nameof(PerMaterial));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionColorSkinnedInstanced()
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
                cbPerMaterial?.Dispose();
            }
        }

        /// <inheritdoc/>
        public override void UpdateMaterial(BuiltInDrawerMaterialState state)
        {
            cbPerMaterial.WriteData(PerMaterial.Build(state));

            var vertexShader = GetVertexShader<PositionColorSkinnedVsI>();
            vertexShader?.SetPerMaterialConstantBuffer(cbPerMaterial);
        }
    }
}
