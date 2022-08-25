using Shaders.Properties;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position normal texture tangent vertex shader
    /// </summary>
    public class ShadowPositionNormalTextureTangentVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerInstance : IBufferData
        {
            public static PerInstance Build(uint textureIndex)
            {
                return new PerInstance
                {
                    TextureIndex = textureIndex,
                };
            }

            /// <summary>
            /// Texture index
            /// </summary>
            [FieldOffset(0)]
            public uint TextureIndex;

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

        /// <inheritdoc/>
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public ShadowPositionNormalTextureTangentVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(ShadowPositionNormalTextureTangentVs), "main", ShaderShadowBasicResources.PositionNormalTextureTangent_vs, HelperShaders.VSProfile);

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(ShadowPositionNormalTextureTangentVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowPositionNormalTextureTangentVs()
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

                cbPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Writes per instance data
        /// </summary>
        /// <param name="material">Material</param>
        /// <param name="tintColor">Tint color</param>
        /// <param name="textureIndex">Texture index</param>
        public void WriteCBPerInstance(uint textureIndex)
        {
            cbPerInstance.WriteData(PerInstance.Build(textureIndex));
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetVSPerFrame(),
                cbPerInstance,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);
        }
    }
}
