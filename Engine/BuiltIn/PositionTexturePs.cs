using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position texture pixel shader
    /// </summary>
    public class PositionTexturePs : IDisposable
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerFrame : IBufferData
        {
            public static PerFrame Build(uint channel)
            {
                return new PerFrame
                {
                    Channel = channel,
                };
            }

            /// <summary>
            /// Color output channel
            /// </summary>
            [FieldOffset(0)]
            public uint Channel;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFrame));
            }
        }

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFrame> cbPerFrame;
        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionTexture_Cso == null;
            var bytes = Resources.Ps_PositionTexture_Cso ?? Resources.Ps_PositionTexture;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionTexturePs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionTexturePs), bytes);
            }

            cbPerFrame = new EngineConstantBuffer<PerFrame>(graphics, nameof(PositionTexturePs) + "." + nameof(PerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionTexturePs()
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

                cbPerFrame?.Dispose();
            }
        }

        /// <summary>
        /// Writes per frame data
        /// </summary>
        /// <param name="channel">Color output channel</param>
        public void WriteCBPerFrame(uint channel)
        {
            cbPerFrame.WriteData(PerFrame.Build(channel));
        }
        /// <summary>
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPSPerFrameNoLit(),
                cbPerFrame,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            Graphics.SetPixelShaderResourceView(0, diffuseMapArray);
        }
    }
}
