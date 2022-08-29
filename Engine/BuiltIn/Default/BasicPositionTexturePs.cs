using Engine.Shaders.Properties;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position texture pixel shader
    /// </summary>
    public class BasicPositionTexturePs : IBuiltInPixelShader
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

        /// <inheritdoc/>
        public EnginePixelShader Shader { get; private set; }

        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFrame> cbPerFrame;
        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;
        /// <summary>
        /// Diffuse sampler
        /// </summary>
        private EngineSamplerState samplerDiffuse;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public BasicPositionTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompilePixelShader(nameof(BasicPositionTexturePs), "main", ShaderDefaultBasicResources.PositionTexture_ps, HelperShaders.PSProfile);

            cbPerFrame = new EngineConstantBuffer<PerFrame>(graphics, nameof(BasicPositionTexturePs) + "." + nameof(PerFrame));

            samplerDiffuse = BuiltInShaders.GetSamplerLinear();
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionTexturePs()
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
        /// Sets the diffuse sampler state
        /// </summary>
        /// <param name="samplerDiffuse">Diffuse sampler</param>
        public void SetDiffseSampler(EngineSamplerState samplerDiffuse)
        {
            this.samplerDiffuse = samplerDiffuse;
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetPSPerFrame(),
                cbPerFrame,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            Graphics.SetPixelShaderResourceView(0, diffuseMapArray);

            Graphics.SetPixelShaderSampler(0, samplerDiffuse);
        }
    }
}
