using Engine.Shaders.Properties;
using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Default
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Position color vertex shader
    /// </summary>
    public class BasicPositionColorVs : IBuiltInVertexShader
    {
        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerInstance : IBufferData
        {
            public static PerInstance Build(MaterialDrawInfo material, Color4 tintColor)
            {
                return new PerInstance
                {
                    TintColor = tintColor,
                    MaterialIndex = material.Material?.ResourceIndex ?? 0,
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
        public BasicPositionColorVs(Graphics graphics)
        {
            Graphics = graphics;

            Shader = graphics.CompileVertexShader(nameof(BasicPositionColorVs), "main", ShaderDefaultBasicResources.PositionColor_vs, HelperShaders.VSProfile);

            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(BasicPositionColorVs) + "." + nameof(PerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicPositionColorVs()
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
        public void WriteCBPerInstance(MaterialDrawInfo material, Color4 tintColor)
        {
            cbPerInstance.WriteData(PerInstance.Build(material, tintColor));
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

            Graphics.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPalette());
        }
    }
}
