﻿using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal color instanced vertex shader
    /// </summary>
    public class PositionNormalColorVsI : IBuiltInVertexShader
    {
        /// <summary>
        /// Per object data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 32)]
        struct PerObject : IBufferData
        {
            public static PerObject Build(MaterialDrawInfo material, Color4 tintColor)
            {
                return new PerObject
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
                return Marshal.SizeOf(typeof(PerObject));
            }
        }

        /// <summary>
        /// Per object constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerObject> cbPerObject;

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
        public PositionNormalColorVsI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionNormalColor_I_Cso == null;
            var bytes = Resources.Vs_PositionNormalColor_I_Cso ?? Resources.Vs_PositionNormalColor_I;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionNormalColorVsI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionNormalColorVsI), bytes);
            }

            cbPerObject = new EngineConstantBuffer<PerObject>(graphics, nameof(PositionNormalColorVsI) + "." + nameof(PerObject));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalColorVsI()
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

                cbPerObject?.Dispose();
            }
        }

        /// <summary>
        /// Writes per object data
        /// </summary>
        /// <param name="material">Material</param>
        /// <param name="tintColor">Tint color</param>
        public void WriteCBPerObject(MaterialDrawInfo material, Color4 tintColor)
        {
            cbPerObject.WriteData(PerObject.Build(material, tintColor));
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
                cbPerObject,
            };

            Graphics.SetVertexShaderConstantBuffers(0, cb);

            Graphics.SetVertexShaderResourceView(0, BuiltInShaders.GetMaterialPalette());
        }
    }
}