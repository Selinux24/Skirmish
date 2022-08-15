using SharpDX;
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
        public struct Globals : IBufferData
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
                return Marshal.SizeOf(typeof(Globals));
            }
        }

        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrame : IBufferData
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
                return Marshal.SizeOf(typeof(PerFrame));
            }
        }

        /// <summary>
        /// Per instance data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerInstance : IBufferData
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
                return Marshal.SizeOf(typeof(PerInstance));
            }
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<Globals> cbGlobals;
        /// <summary>
        /// Material palette resource view
        /// </summary>
        private EngineShaderResourceView materialPalette;
        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFrame> cbPerFrame;
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

            cbGlobals = new EngineConstantBuffer<Globals>(graphics, nameof(PositionNormalColorVs) + "." + nameof(Globals));
            cbPerFrame = new EngineConstantBuffer<PerFrame>(graphics, nameof(PositionNormalColorVs) + "." + nameof(PerFrame));
            cbPerInstance = new EngineConstantBuffer<PerInstance>(graphics, nameof(PositionNormalColorVs) + "." + nameof(PerInstance));
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
                cbGlobals?.Dispose();
                cbPerFrame?.Dispose();
                cbPerInstance?.Dispose();
            }
        }

        /// <summary>
        /// Sets the globals data
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        public void SetCBGlobals(EngineShaderResourceView materialPalette, uint materialPaletteWidth)
        {
            this.materialPalette = materialPalette;

            var data = new Globals
            {
                MaterialPaletteWidth = materialPaletteWidth,

                Pad1 = 9999,
                Pad2 = 9999,
                Pad3 = 9999,
            };
            cbGlobals.WriteData(data);
        }
        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="worldViewProjection">World view projection matrix</param>
        public void SetCBPerFrame(Matrix world, Matrix worldViewProjection)
        {
            var data = new PerFrame
            {
                World = Matrix.Transpose(world),
                WorldViewProjection = Matrix.Transpose(worldViewProjection),
            };
            cbPerFrame.WriteData(data);
        }
        /// <summary>
        /// Sets per instance data
        /// </summary>
        /// <param name="tintColor">Tint color</param>
        /// <param name="materialIndex">Material index</param>
        public void SetCBPerInstance(Color4 tintColor, uint materialIndex)
        {
            var data = new PerInstance
            {
                TintColor = tintColor,
                MaterialIndex = materialIndex,

                Pad1 = 9999,
                Pad2 = 9999,
                Pad3 = 9999,
            };
            cbPerInstance.WriteData(data);
        }

        /// <summary>
        /// Sets the vertex shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetVertexShaderConstantBuffers(0, new IEngineConstantBuffer[] { cbGlobals, cbPerFrame, cbPerInstance });

            Graphics.SetVertexShaderResourceView(0, materialPalette);
        }
    }
}
