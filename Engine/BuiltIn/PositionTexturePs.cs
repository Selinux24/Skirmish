using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
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
        [StructLayout(LayoutKind.Explicit, Size = 48)]
        public struct PerFrame : IBufferData
        {
            /// <summary>
            /// Eye position world
            /// </summary>
            [FieldOffset(0)]
            public Vector3 EyePositionWorld;

            /// <summary>
            /// Fog color
            /// </summary>
            [FieldOffset(16)]
            public Color4 FogColor;

            /// <summary>
            /// Fog start distance
            /// </summary>
            [FieldOffset(32)]
            public float FogStart;
            /// <summary>
            /// Fog range distance
            /// </summary>
            [FieldOffset(36)]
            public float FogRange;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFrame));
            }
        }

        /// <summary>
        /// Per frame spec data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct PerFrame2 : IBufferData
        {
            /// <summary>
            /// Color output channel
            /// </summary>
            [FieldOffset(0)]
            public uint Channel;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerFrame2));
            }
        }

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

        /// <summary>
        /// Per frame constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFrame> cbPerFrame;
        /// <summary>
        /// Per frame spec constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerFrame2> cbPerFrame2;
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

            cbPerFrame = new EngineConstantBuffer<PerFrame>(graphics, nameof(PositionTexturePs) + "." + nameof(PerFrame));
            cbPerFrame2 = new EngineConstantBuffer<PerFrame2>(graphics, nameof(PositionTexturePs) + "." + nameof(PerFrame2));
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
                cbPerFrame?.Dispose();
                cbPerFrame2?.Dispose();
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
            var data = new PerFrame
            {
                EyePositionWorld = eyePositionWorld,
                FogColor = fogColor,
                FogStart = fogStart,
                FogRange = fogRange,
            };
            cbPerFrame.WriteData(data);
        }
        /// <summary>
        /// Sets per frame spec data
        /// </summary>
        /// <param name="channel">Color output channel</param>
        public void SetVSPerFrame2(uint channel)
        {
            var data = new PerFrame2
            {
                Channel = channel,
            };
            cbPerFrame2.WriteData(data);
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
            Graphics.SetPixelShaderConstantBuffers(0, new IEngineConstantBuffer[] { cbPerFrame, cbPerFrame2 });

            Graphics.SetPixelShaderResourceView(0, diffuseMapArray);
        }
    }
}
