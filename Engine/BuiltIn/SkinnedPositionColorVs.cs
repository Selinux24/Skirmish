using SharpDX;
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
    public class SkinnedPositionColorVs : IDisposable
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        struct PerInstance : IBufferData
        {
            public static PerInstance Build(MaterialDrawInfo material, Color4 tintColor, AnimationDrawInfo animation)
            {
                return new PerInstance
                {
                    TintColor = tintColor,
                    MaterialIndex = material.Material?.ResourceIndex ?? 0,
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
        public SkinnedPositionColorVs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_Skinned_Cso == null;
            var bytes = Resources.Vs_PositionColor_Skinned_Cso ?? Resources.Vs_PositionColor_Skinned;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(SkinnedPositionColorVs), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(SkinnedPositionColorVs), bytes);
            }

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(SkinnedPositionColorVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~SkinnedPositionColorVs()
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
        /// Writes per instance data
        /// </summary>
        /// <param name="material">Material</param>
        /// <param name="tintColor">Tint color</param>
        /// <param name="animation">Animation</param>
        public void WriteCBPerInstance(MaterialDrawInfo material, Color4 tintColor, AnimationDrawInfo animation)
        {
            cbPerInstance.WriteData(PerInstance.Build(material, tintColor, animation));
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
