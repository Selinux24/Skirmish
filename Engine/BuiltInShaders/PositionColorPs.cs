using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInShaders
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position color pixel shader
    /// </summary>
    public class PositionColorPs : IDisposable
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame : IBufferData
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

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSPerFrame));
            }
        }

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

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
                vsPerFrame?.Dispose();
            }
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

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetPixelShaderConstantBuffer(0, vsPerFrame);
        }
    }
}
