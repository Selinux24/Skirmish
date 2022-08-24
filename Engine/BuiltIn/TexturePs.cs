using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Cubemap pixel shader
    /// </summary>
    public class TexturePs : IBuiltInPixelShader
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerFrame : IBufferData
        {
            public static PerFrame Build(float textureIndex)
            {
                return new PerFrame
                {
                    TextureIndex = textureIndex,
                };
            }

            /// <summary>
            /// Texture index
            /// </summary>
            [FieldOffset(0)]
            public float TextureIndex;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFrame));
            }
        }

        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFrame> cbPerFrame;
        /// <summary>
        /// Texture resource view
        /// </summary>
        private EngineShaderResourceView texture;
        /// <summary>
        /// Texture sampler
        /// </summary>
        private EngineSamplerState sampler;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public TexturePs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_Texture_Cso == null;
            var bytes = Resources.Ps_Texture_Cso ?? Resources.Ps_Texture;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(TexturePs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(TexturePs), bytes);
            }

            cbPerFrame = new EngineConstantBuffer<PerFrame>(graphics, nameof(TexturePs) + "." + nameof(PerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TexturePs()
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
                Shader?.Dispose();
                Shader = null;

                cbPerFrame?.Dispose();
            }
        }

        /// <summary>
        /// Writes per frame data
        /// </summary>
        /// <param name="textureIndex">Texture index</param>
        public void WriteCBPerFrame(uint textureIndex)
        {
            cbPerFrame.WriteData(PerFrame.Build(textureIndex));
        }
        /// <summary>
        /// Sets the texture
        /// </summary>
        /// <param name="texture">Texture</param>
        public void SetTexture(EngineShaderResourceView texture)
        {
            this.texture = texture;
        }
        /// <summary>
        /// Sets the texture sampler state
        /// </summary>
        /// <param name="sampler">Sampler</param>
        public void SetSampler(EngineSamplerState sampler)
        {
            this.sampler = sampler;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            Graphics.SetPixelShaderConstantBuffer(0, cbPerFrame);

            Graphics.SetPixelShaderResourceView(0, texture);

            Graphics.SetPixelShaderSampler(0, sampler);
        }
    }
}
