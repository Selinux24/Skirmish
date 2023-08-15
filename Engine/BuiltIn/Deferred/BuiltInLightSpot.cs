﻿using SharpDX;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Spot light drawer
    /// </summary>
    public class BuiltInLightSpot : BuiltInDrawer
    {
        /// <summary>
        /// Per light data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 64)]
        struct PerLight : IBufferData
        {
            public static PerLight Build(Matrix local)
            {
                return new PerLight
                {
                    Local = Matrix.Transpose(local),
                };
            }

            /// <summary>
            /// Local transform
            /// </summary>
            [FieldOffset(0)]
            public Matrix Local;

            /// <inheritdoc/>
            public readonly int GetStride()
            {
                return Marshal.SizeOf(typeof(PerLight));
            }
        }

        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInLightSpot() : base()
        {
            SetVertexShader<DeferredLightVs>(false);
            SetPixelShader<DeferredLightSpotPs>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(IEnumerable<EngineShaderResourceView> geometryMap)
        {
            var pixelShader = GetPixelShader<DeferredLightSpotPs>();
            pixelShader?.SetDeferredBuffer(geometryMap);
            pixelShader?.SetPointSampler(pointSampler);
        }
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="light">Light constant buffer</param>
        public void UpdatePerLight(IEngineDeviceContext dc, ISceneLightSpot light)
        {
            var cbLight = BuiltInShaders.GetConstantBuffer<PerLight>();
            dc.UpdateConstantBuffer(cbLight, PerLight.Build(light.Local));

            var vertexShader = GetVertexShader<DeferredLightVs>();
            vertexShader?.SetPerLightConstantBuffer(cbLight);

            var cbSpot = BuiltInShaders.GetConstantBuffer<BuiltInShaders.BufferLightSpot>();
            dc.UpdateConstantBuffer(cbSpot, BuiltInShaders.BufferLightSpot.Build(light));

            var pixelShader = GetPixelShader<DeferredLightSpotPs>();
            pixelShader?.SetPerLightConstantBuffer(cbSpot);
        }
    }
}
