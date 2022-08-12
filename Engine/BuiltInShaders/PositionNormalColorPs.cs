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
            /// <summary>
            /// Hemispheric light
            /// </summary>
            public BufferLightHemispheric HemiLight;
            /// <summary>
            /// Directional lights
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightDirectional.MAX)]
            public BufferLightDirectional[] DirLights;
            /// <summary>
            /// Point lights
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightPoint.MAX)]
            public BufferLightPoint[] PointLights;
            /// <summary>
            /// Spot lights
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = BufferLightSpot.MAX)]
            public BufferLightSpot[] SpotLights;
            /// <summary>
            /// Directional lights count
            /// </summary>
            public uint DirLightsCount;
            /// <summary>
            /// Point lights count
            /// </summary>
            public uint PointLightsCount;
            /// <summary>
            /// Spot lights count
            /// </summary>
            public uint SpotLightsCount;
            /// <summary>
            /// Shadow intensity
            /// </summary>
            public float ShadowIntensity;
            /// <summary>
            /// Level of detail values
            /// </summary>
            public Vector3 LevelOfDetail;
            public float Pad4;

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

            vsPerFrame = new EngineConstantBuffer<VSPerFrame>(graphics, nameof(PositionNormalColorPs) + "." + nameof(VSPerFrame));
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
                vsPerFrame?.Dispose();
            }
        }

        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="lights">Scene lights</param>
        /// <param name="levelOfDetail">Level of detail</param>
        public void SetVSPerFrame(
            Vector3 eyePositionWorld,
            SceneLights lights,
            Vector3 levelOfDetail)
        {
            var hemiLight = BufferLightHemispheric.Build(lights?.GetVisibleHemisphericLight());
            var dirLights = BufferLightDirectional.Build(lights?.GetVisibleDirectionalLights(), out int dirLength);
            var pointLights = BufferLightPoint.Build(lights?.GetVisiblePointLights(), out int pointLength);
            var spotLights = BufferLightSpot.Build(lights?.GetVisibleSpotLights(), out int spotLength);

            var data = new VSPerFrame
            {
                EyePositionWorld = eyePositionWorld,
                FogColor = lights?.FogColor ?? Color.Transparent,
                FogStart = lights?.FogStart ?? 0,
                FogRange = lights?.FogRange ?? 0,
                HemiLight = hemiLight,
                DirLights = dirLights,
                PointLights = pointLights,
                SpotLights = spotLights,
                DirLightsCount = (uint)dirLength,
                PointLightsCount = (uint)pointLength,
                SpotLightsCount = (uint)spotLength,
                ShadowIntensity = lights?.ShadowIntensity ?? 0f,
                LevelOfDetail = levelOfDetail,
            };
            vsPerFrame.WriteData(data);
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
            Graphics.SetPixelShaderConstantBuffer(0, vsPerFrame);

            Graphics.SetPixelShaderResourceViews(0, new[] { shadowMapDir, shadowMapSpot, shadowMapPoint });
        }
    }
}
