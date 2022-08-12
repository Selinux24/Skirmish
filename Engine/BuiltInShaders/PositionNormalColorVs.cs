﻿using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInShaders
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal color vertex shader
    /// </summary>
    public class PositionNormalColorVs : IDisposable
    {
        /// <summary>
        /// Global data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSGlobals : IBufferData
        {
            /// <summary>
            /// Material palette width
            /// </summary>
            public uint MaterialPaletteWidth;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSGlobals));
            }
        }

        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame : IBufferData
        {
            /// <summary>
            /// World matrix
            /// </summary>
            public Matrix World;
            /// <summary>
            /// World view projection matrix
            /// </summary>
            public Matrix WorldViewProjection;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSPerFrame));
            }
        }

        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerInstance : IBufferData
        {
            /// <summary>
            /// Tint color
            /// </summary>
            public Color4 TintColor;
            /// <summary>
            /// Material index
            /// </summary>
            public uint MaterialIndex;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSPerInstance));
            }
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSGlobals> vsGlobals;
        /// <summary>
        /// Material palette resource view
        /// </summary>
        private EngineShaderResourceView materialPalette;
        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerFrame> vsPerFrame;
        /// <summary>
        /// Per instance constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerInstance> vsPerInstance;

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
        public PositionNormalColorVs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionNormalColor_Cso == null;
            var bytes = Resources.Vs_PositionNormalColor_Cso ?? Resources.Vs_PositionNormalColor;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionNormalColorVs), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionNormalColorVs), bytes);
            }

            vsGlobals = new EngineConstantBuffer<VSGlobals>(graphics, nameof(PositionNormalColorVs) + "." + nameof(VSGlobals));
            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionNormalColorVs) + "." + nameof(VSPerFrame));
            vsPerInstance = new EngineConstantBuffer<VSPerInstance>(graphics, nameof(PositionNormalColorVs) + "." + nameof(VSPerInstance));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalColorVs()
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
                vsGlobals?.Dispose();
                vsPerFrame?.Dispose();
                vsPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Sets the globals data
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        public void SetVSGlobals(EngineShaderResourceView materialPalette, uint materialPaletteWidth)
        {
            this.materialPalette = materialPalette;

            var data = new VSGlobals
            {
                MaterialPaletteWidth = materialPaletteWidth,
            };
            vsGlobals.WriteData(data);
        }
        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="worldViewProjection">World view projection matrix</param>
        public void SetVSPerFrame(Matrix world, Matrix worldViewProjection)
        {
            var data = new VSPerFrame
            {
                World = Matrix.Transpose(world),
                WorldViewProjection = Matrix.Transpose(worldViewProjection),
            };
            vsPerFrame.WriteData(data);
        }
        /// <summary>
        /// Sets per instance data
        /// </summary>
        /// <param name="tintColor">Tint color</param>
        /// <param name="materialIndex">Material index</param>
        public void SetVSPerInstance(Color4 tintColor, uint materialIndex)
        {
            var data = new VSPerInstance
            {
                TintColor = tintColor,
                MaterialIndex = materialIndex,
            };
            vsPerInstance.WriteData(data);
        }

        /// <summary>
        /// Sets the vertex shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetVertexShaderConstantBuffers(0, new IEngineConstantBuffer[] { vsGlobals, vsPerFrame, vsPerInstance });

            Graphics.SetVertexShaderResourceView(0, materialPalette);
        }
    }
}
