using Engine.Shaders.Properties;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position normal texture vertex shader
    /// </summary>
    public class ShadowSkinnedPositionNormalTextureVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerInstance : IBufferData
        {
            public static PerInstance Build(uint textureIndex, AnimationDrawInfo animation)
            {
                return new PerInstance
                {
                    TextureIndex = textureIndex,
                    AnimationOffset = animation.Offset1,
                    AnimationOffset2 = animation.Offset2,
                    AnimationInterpolation = animation.InterpolationAmount,
                };
            }

            /// <summary>
            /// Texture index
            /// </summary>
            [FieldOffset(0)]
            public uint TextureIndex;
            /// <summary>
            /// Animation offset 1
            /// </summary>
            [FieldOffset(4)]
            public uint AnimationOffset;
            /// <summary>
            /// Animation offset 2
            /// </summary>
            [FieldOffset(8)]
            public uint AnimationOffset2;
            /// <summary>
            /// Animation interpolation value
            /// </summary>
            [FieldOffset(12)]
            public float AnimationInterpolation;

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
        public ShadowSkinnedPositionNormalTextureVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(ShadowSkinnedPositionNormalTextureVs), "main", ShaderShadowBasicResources.PositionNormalTextureSkinned_vs, HelperShaders.VSProfile);

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(ShadowSkinnedPositionNormalTextureVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowSkinnedPositionNormalTextureVs()
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
        /// <param name="textureIndex">Texture index</param>
        /// <param name="animation">Animation</param>
        public void WriteCBPerInstance(uint textureIndex, AnimationDrawInfo animation)
        {
            cbPerInstance.WriteData(PerInstance.Build(textureIndex, animation));
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetGlobal(),
                BuiltInShaders.GetVSPerFrame(),
                cbPerInstance,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
