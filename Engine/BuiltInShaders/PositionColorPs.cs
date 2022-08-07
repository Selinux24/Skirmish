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
    public class PositionColorPs : IDisposable
    {
        /// <summary>
        /// Global data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSGlobals
        {
            /// <summary>
            /// Material palette width
            /// </summary>
            public uint MaterialPaletteWidth;
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
            /// <summary>
            /// Eye position world
            /// </summary>
            public Vector3 EyePositionWorld;
            public float Pad1;
            /// <summary>
            /// Fog color
            /// </summary>
            public Color4 FogColor;
            /// <summary>
            /// Fog start distance
            /// </summary>
            public float FogStart;
            /// <summary>
            /// Fog range distance
            /// </summary>
            public float FogRange;
            public float Pad2;
            public float Pad3;
        }

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

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
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionColorPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionColor_Cso == null;
            var bytes = Resources.Ps_PositionColor_Cso ?? Resources.Ps_PositionColor;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionColorPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionColorPs), bytes);
            }

            vsGlobals = new EngineConstantBuffer<VSGlobals>(graphics, nameof(PositionColorPs) + "." + nameof(VSGlobals));
            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionColorPs) + "." + nameof(VSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionColorPs()
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
        /// Sets the globals data
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        public void SetVSGlobals(EngineShaderResourceView materialPalette, uint materialPaletteWidth)
        {
            var data = new VSGlobals
            {
                MaterialPaletteWidth = materialPaletteWidth,
            };

            vsGlobals.WriteData(data);

            Graphics.SetPixelShaderResources(0, 1, materialPalette);
        }
        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="fogColor">Fog color</param>
        /// <param name="fogStart">Fog start distance</param>
        /// <param name="fogRange">Fog range distance</param>
        public void SetVSPerFrame(Vector3 eyePositionWorld, Color4 fogColor, float fogStart, float fogRange)
        {
            var data = new VSPerFrame
            {
                EyePositionWorld = eyePositionWorld,
                FogColor = fogColor,
                FogStart = fogStart,
                FogRange = fogRange,
            };

            vsPerFrame.WriteData(data);
        }
    }
}
