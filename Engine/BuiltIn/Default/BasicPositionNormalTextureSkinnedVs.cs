using Engine.Shaders.Properties;
using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Skinned position normal texture vertex shader
    /// </summary>
    public class BasicPositionNormalTextureSkinnedVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerInstance : IBufferData
        {
            public static PerInstance Build(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
            {
                return new PerInstance
                {
                    TintColor = tintColor,
                    MaterialIndex = material.Material?.ResourceIndex ?? 0,
                    TextureIndex = textureIndex,
                    AnimationOffset = animation.Offset1,
                    AnimationOffset2 = animation.Offset2,
                    AnimationInterpolation = animation.InterpolationAmount,
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
            /// Texture index
            /// </summary>
            [FieldOffset(20)]
            public uint TextureIndex;

            /// <summary>
            /// Animation offset 1
            /// </summary>
            [FieldOffset(32)]
            public uint AnimationOffset;
            /// <summary>
            /// Animation offset 2
            /// </summary>
            [FieldOffset(36)]
            public uint AnimationOffset2;
            /// <summary>
            /// Animation interpolation value
            /// </summary>
            [FieldOffset(40)]
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
        public BasicPositionNormalTextureSkinnedVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(BasicPositionNormalTextureSkinnedVs), "main", ShaderDefaultBasicResources.PositionNormalTextureSkinned_vs, HelperShaders.VSProfile);

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(BasicPositionNormalTextureSkinnedVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionNormalTextureSkinnedVs()
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
        /// <param name="animation">Animation</param>
        public void WriteCBPerInstance(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            cbPerInstance.WriteData(PerInstance.Build(material, tintColor, textureIndex, animation));
        }

        /// <inheritdoc/>
        public void SetShaderResources()
        {
            var cb = new[]
            {
                BuiltInShaders.GetVSGlobal(),
                BuiltInShaders.GetVSPerFrame(),
                cbPerInstance,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetMaterialPalette(),
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
