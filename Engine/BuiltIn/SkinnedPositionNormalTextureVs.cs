using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Skinned position normal texture vertex shader
    /// </summary>
    public class SkinnedPositionNormalTextureVs : IDisposable
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
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

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EngineVertexShader Shader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public SkinnedPositionNormalTextureVs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionNormalTexture_Skinned_Cso == null;
            var bytes = Resources.Vs_PositionNormalTexture_Skinned_Cso ?? Resources.Vs_PositionNormalTexture_Skinned;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(SkinnedPositionNormalTextureVs), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(SkinnedPositionNormalTextureVs), bytes);
            }

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(SkinnedPositionNormalTextureVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SkinnedPositionNormalTextureVs()
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
        /// <param name="animationOffset">Animation offset 1</param>
        /// <param name="animationOffset2">Animation offset 2</param>
        /// <param name="animationInterpolation">Animation interpolation value</param>
        public void SetVSPerInstance(Color4 tintColor, uint materialIndex, uint animationOffset, uint animationOffset2, float animationInterpolation)
        {
            var data = new PerInstance
            {
                TintColor = tintColor,
                MaterialIndex = materialIndex,
                AnimationOffset = animationOffset,
                AnimationOffset2 = animationOffset2,
                AnimationInterpolation = animationInterpolation,
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

            var rv = new[]
            {
                BuiltInShaders.GetMaterialPalette(),
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
