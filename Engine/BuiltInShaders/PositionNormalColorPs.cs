using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltInShaders
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal color pixel shader
    /// </summary>
    public class PositionNormalColorPs : IDisposable
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 4176)]
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

            /// <summary>
            /// Level of detail values
            /// </summary>
            [FieldOffset(48)]
            public Vector3 LevelOfDetail;

            /// <summary>
            /// Directional lights count
            /// </summary>
            [FieldOffset(64)]
            public uint DirLightsCount;
            /// <summary>
            /// Point lights count
            /// </summary>
            [FieldOffset(68)]
            public uint PointLightsCount;
            /// <summary>
            /// Spot lights count
            /// </summary>
            [FieldOffset(72)]
            public uint SpotLightsCount;
            /// <summary>
            /// Shadow intensity
            /// </summary>
            [FieldOffset(76)]
            public float ShadowIntensity;

            /// <summary>
            /// Hemispheric light
            /// </summary>
            [FieldOffset(80)]
            public BufferLightHemispheric HemiLight;
            /// <summary>
            /// Directional lights
            /// </summary>
            [FieldOffset(112), MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightDirectional.MAX)]
            public BufferLightDirectional[] DirLights;
            /// <summary>
            /// Point lights
            /// </summary>
            [FieldOffset(592), MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightPoint.MAX)]
            public BufferLightPoint[] PointLights;
            /// <summary>
            /// Spot lights
            /// </summary>
            [FieldOffset(1872), MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightSpot.MAX)]
            public BufferLightSpot[] SpotLights;

            /// <inheritdoc/>
            public int GetStride()
            {
#if DEBUG
                int size = Marshal.SizeOf(typeof(PerFrame));
                if (size % 16 != 0) throw new EngineException($"Buffer {nameof(PerFrame)} strides must be divisible by 16 in order to be sent to shaders and effects as arrays");
                return size;
#else
            return Marshal.SizeOf(typeof(BufferLightHemispheric));
#endif
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
        /// Directional shadow map resource view
        /// </summary>
        private EngineShaderResourceView shadowMapDir;
        /// <summary>
        /// Spot shadow map resource view
        /// </summary>
        private EngineShaderResourceView shadowMapSpot;
        /// <summary>
        /// Point shadow map resource view
        /// </summary>
        private EngineShaderResourceView shadowMapPoint;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalColorPs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalColor_Cso == null;
            var bytes = Resources.Ps_PositionNormalColor_Cso ?? Resources.Ps_PositionNormalColor;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionNormalColorPs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionNormalColorPs), bytes);
            }

            cbPerFrame = new EngineConstantBuffer<PerFrame>(graphics, nameof(PositionNormalColorPs) + "." + nameof(PerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalColorPs()
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
            }
        }

        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="lights">Scene lights</param>
        /// <param name="levelOfDetail">Level of detail</param>
        public void SetCBPerFrame(
            Vector3 eyePositionWorld,
            SceneLights lights,
            Vector3 levelOfDetail)
        {
            var hemiLight = BufferLightHemispheric.Build(lights?.GetVisibleHemisphericLight());
            var dirLights = BufferLightDirectional.Build(lights?.GetVisibleDirectionalLights(), out int dirLength);
            var pointLights = BufferLightPoint.Build(lights?.GetVisiblePointLights(), out int pointLength);
            var spotLights = BufferLightSpot.Build(lights?.GetVisibleSpotLights(), out int spotLength);

            var data = new PerFrame
            {
                EyePositionWorld = eyePositionWorld,

                FogColor = lights?.FogColor ?? Color.Transparent,

                FogStart = lights?.FogStart ?? 0,
                FogRange = lights?.FogRange ?? 0,

                LevelOfDetail = levelOfDetail,

                DirLightsCount = (uint)dirLength,
                PointLightsCount = (uint)pointLength,
                SpotLightsCount = (uint)spotLength,
                ShadowIntensity = lights?.ShadowIntensity ?? 0f,

                HemiLight = hemiLight,
                DirLights = dirLights,
                PointLights = pointLights,
                SpotLights = spotLights,
            };
            cbPerFrame.WriteData(data);
        }
        /// <summary>
        /// Sets the directional shadow map array
        /// </summary>
        /// <param name="shadowMapDir">Directional shadow map array</param>
        public void SetDirShadowMap(EngineShaderResourceView shadowMapDir)
        {
            this.shadowMapDir = shadowMapDir;
        }
        /// <summary>
        /// Sets the spot shadow map array
        /// </summary>
        /// <param name="shadowMapSpot">Spot shadow map array</param>
        public void SetSpotShadowMap(EngineShaderResourceView shadowMapSpot)
        {
            this.shadowMapSpot = shadowMapSpot;
        }
        /// <summary>
        /// Sets the point shadow map array
        /// </summary>
        /// <param name="shadowMapPoint">Point shadow map array</param>
        public void SetPointShadowMap(EngineShaderResourceView shadowMapPoint)
        {
            this.shadowMapPoint = shadowMapPoint;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            Graphics.SetPixelShaderConstantBuffer(0, cbPerFrame);

            Graphics.SetPixelShaderResourceViews(0, new[] { shadowMapDir, shadowMapSpot, shadowMapPoint });
        }
    }
}
