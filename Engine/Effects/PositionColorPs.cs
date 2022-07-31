using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;
    using SharpDX.Direct3D11;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class PositionColorPs : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VSGlobals
        {
            public uint MaterialPaletteWidth;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame
        {
            public Vector3 EyePositionWorld;
            public float Pad1;
            public Color4 FogColor;
            public float FogStart;
            public float FogRange;
            public float Pad2;
            public float Pad3;
        }

        private readonly EnginePixelShader shader;
        private readonly Buffer vsGlobals;
        private readonly Buffer vsPerFrame;

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
                shader = graphics.CompilePixelShader(nameof(PositionColorPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                shader = graphics.LoadPixelShader(nameof(PositionColorPs), bytes);
            }

            vsGlobals = graphics.CreateConstantBuffer<VSGlobals>(nameof(PositionColorPs) + "." + nameof(VSGlobals));
            vsPerFrame = graphics.CreateConstantBuffer<VSPerFrame>(nameof(PositionColorPs) + "." + nameof(VSPerFrame));
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
                shader?.Dispose();
                vsGlobals?.Dispose();
                vsPerFrame?.Dispose();
            }
        }


        public void SetVSGlobals(uint materialPaletteWidth)
        {
            var data = new VSGlobals
            {
                MaterialPaletteWidth = materialPaletteWidth,
            };

            Graphics.WriteDiscardBuffer(vsGlobals, data);
        }
        public void SetVSPerFrame(Vector3 eyePositionWorld, Color4 fogColor, float fogStart, float fogRange)
        {
            var data = new VSPerFrame
            {
                EyePositionWorld = eyePositionWorld,
                FogColor = fogColor,
                FogStart = fogStart,
                FogRange = fogRange,
            };

            Graphics.WriteDiscardBuffer(vsPerFrame, data);
        }
        public void SetShader()
        {
            Graphics.SetPixelShader(shader);
        }
    }
}
