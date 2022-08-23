using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Skinned position color vertex shader
    /// </summary>
    public class ShadowSkinnedPositionColorVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerInstance : IBufferData
        {
            public static PerInstance Build(AnimationDrawInfo animation)
            {
                return new PerInstance
                {
                    AnimationOffset = animation.Offset1,
                    AnimationOffset2 = animation.Offset2,
                    AnimationInterpolation = animation.InterpolationAmount,
                };
            }

            /// <summary>
            /// Animation offset 1
            /// </summary>
            [FieldOffset(0)]
            public uint AnimationOffset;
            /// <summary>
            /// Animation offset 2
            /// </summary>
            [FieldOffset(4)]
            public uint AnimationOffset2;
            /// <summary>
            /// Animation interpolation value
            /// </summary>
            [FieldOffset(8)]
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
        public EngineVertexShader Shader { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public ShadowSkinnedPositionColorVs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_ShadowPositionColor_Skinned_Cso == null;
            var bytes = Resources.Vs_ShadowPositionColor_Skinned_Cso ?? Resources.Vs_ShadowPositionColor_Skinned;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(ShadowSkinnedPositionColorVs), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(ShadowSkinnedPositionColorVs), bytes);
            }

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(ShadowSkinnedPositionColorVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ShadowSkinnedPositionColorVs()
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
                Shader = null;

                cbPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Writes per instance data
        /// </summary>
        /// <param name="animation">Animation</param>
        public void WriteCBPerInstance(AnimationDrawInfo animation)
        {
            cbPerInstance.WriteData(PerInstance.Build(animation));
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
                BuiltInShaders.GetAnimationPalette(),
            };

            Graphics.SetVertexShaderResourceViews(0, rv);
        }
    }
}
