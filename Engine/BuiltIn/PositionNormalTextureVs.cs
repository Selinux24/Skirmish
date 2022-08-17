using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal texture vertex shader
    /// </summary>
    public class PositionNormalTextureVs : IDisposable
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        public struct PerInstance : IBufferData
        {
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
                return Marshal.SizeOf(typeof(PerInstance));
            }
        }

        /// <summary>
        /// Per instance constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerInstance> cbPerInstance;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EngineVertexShader Shader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalTextureVs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionNormalTexture_Cso == null;
            var bytes = Resources.Vs_PositionNormalTexture_Cso ?? Resources.Vs_PositionNormalTexture;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionNormalTextureVs), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionNormalTextureVs), bytes);
            }

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(PositionNormalTextureVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTextureVs()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
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
                Shader?.Dispose();

                cbPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Sets per instance data
        /// </summary>
        /// <param name="tintColor">Tint color</param>
        /// <param name="materialIndex">Material index</param>
        public void SetCBPerInstance(Color4 tintColor, uint materialIndex)
        {
            var data = new PerInstance
            {
                TintColor = tintColor,
                MaterialIndex = materialIndex,
            };
            cbPerInstance.WriteData(data);
        }

        /// <summary>
        /// Sets the vertex shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var cb = new[]
            {
                BuiltInShaders.GetVSGlobal(),
                BuiltInShaders.GetVSPerFrame(),
                cbPerInstance,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            Graphics.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPalette());
        }
    }
}
