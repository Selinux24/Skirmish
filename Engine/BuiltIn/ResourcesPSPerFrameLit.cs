using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Per-frame resources
    /// </summary>
    public class ResourcesPSPerFrameLit : IDisposable
    {
        /// <summary>
        /// Per frame data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 4176)]
        public struct PSPerFrame : IBufferData
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
                return Marshal.SizeOf(typeof(PSPerFrame));
            }
        }

        /// <summary>
        /// Globals constant buffer
        /// </summary>
        public EngineConstantBuffer<PSPerFrame> PerFrame { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ResourcesPSPerFrameLit(Graphics graphics)
        {
            PerFrame = new EngineConstantBuffer<PSPerFrame>(graphics, nameof(ResourcesPSPerFrameLit) + "." + nameof(PSPerFrame));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ResourcesPSPerFrameLit()
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
                PerFrame?.Dispose();
            }
        }

        /// <summary>
        /// Sets per frame data
        /// </summary>
        /// <param name="eyePositionWorld">Eye position world</param>
        /// <param name="lights">Scene lights</param>
        /// <param name="levelOfDetail">Level of detail</param>
        public void SetCBPerFrame(Vector3 eyePositionWorld, SceneLights lights, Vector3 levelOfDetail)
        {
            var hemiLight = BufferLightHemispheric.Build(lights?.GetVisibleHemisphericLight());
            var dirLights = BufferLightDirectional.Build(lights?.GetVisibleDirectionalLights(), out int dirLength);
            var pointLights = BufferLightPoint.Build(lights?.GetVisiblePointLights(), out int pointLength);
            var spotLights = BufferLightSpot.Build(lights?.GetVisibleSpotLights(), out int spotLength);

            var data = new PSPerFrame
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
            PerFrame.WriteData(data);
        }
    }
}
