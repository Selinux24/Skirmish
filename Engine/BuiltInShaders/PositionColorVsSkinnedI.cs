using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInShaders
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class PositionColorVsSkinnedI : IDisposable
    {
        /// <summary>
        /// Global data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSGlobals
        {
            public uint AnimationPaletteWidth;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;
        }

        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame
        {
            public Matrix World;
            public Matrix WorldViewProjection;
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSGlobals> vsGlobals;
        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerFrame> vsPerFrame;

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
        public PositionColorVsSkinnedI(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Vs_PositionColor_Skinned_I_Cso == null;
            var bytes = Resources.Vs_PositionColor_Skinned_I_Cso ?? Resources.Vs_PositionColor_Skinned_I;
            if (compile)
            {
                Shader = graphics.CompileVertexShader(nameof(PositionColorVsSkinnedI), "main", bytes, HelperShaders.VSProfile);
            }
            else
            {
                Shader = graphics.LoadVertexShader(nameof(PositionColorVsSkinnedI), bytes);
            }

            vsGlobals = new EngineConstantBuffer<VSGlobals>(graphics, nameof(PositionColorVsSkinnedI) + "." + nameof(VSGlobals));
            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionColorVsSkinnedI) + "." + nameof(VSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorVsSkinnedI()
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
            }
        }

        /// <summary>
        /// Sets global data
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWidth">Animation palette texture width</param>
        public void SetVSGlobals(EngineShaderResourceView animationPalette, uint animationPaletteWidth)
        {
            var data = new VSGlobals
            {
                AnimationPaletteWidth = animationPaletteWidth,
            };

            vsGlobals.WriteData(data);

            Graphics.SetVertexShaderResources(0, 1, animationPalette);
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
    }
}
