using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInShaders
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position texture pixel shader
    /// </summary>
    public class PositionTexturePs : IDisposable
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
        /// Per frame spec data structure
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VSPerFrame2 : IBufferData
        {
            /// <summary>
            /// Color output channel
            /// </summary>
            public uint Channel;
            public uint Pad1;
            public uint Pad2;
            public uint Pad3;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(VSPerFrame2));
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
        /// Per frame spec constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<VSPerFrame2> vsPerFrame2;
        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionTexture_Cso == null;
            var bytes = Resources.Ps_PositionTexture_Cso ?? Resources.Ps_PositionTexture;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionTexturePs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionTexturePs), bytes);
            }

            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionTexturePs) + "." + nameof(VSPerFrame));
            vsPerFrame2 = new EngineConstantBuffer<VSPerFrame2>(graphics, nameof(PositionTexturePs) + "." + nameof(VSPerFrame2));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionTexturePs()
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
                vsPerFrame2?.Dispose();
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
        /// Sets per frame spec data
        /// </summary>
        /// <param name="channel">Color output channel</param>
        public void SetVSPerFrame2(uint channel)
        {
            var data = new VSPerFrame2
            {
                Channel = channel,
            };
            vsPerFrame2.WriteData(data);
        }
        /// <summary>
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetPixelShaderConstantBuffers(0, new IEngineConstantBuffer[] { vsPerFrame, vsPerFrame2 });

            Graphics.SetPixelShaderResourceView(0, diffuseMapArray);
        }
    }
}
