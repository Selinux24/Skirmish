using SharpDX;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn.Deferred
{
    using Engine.Common;

    /// <summary>
    /// Point light drawer
    /// </summary>
    public class BuiltInLightPoint : BuiltInDrawer
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
        /// <param name="graphics">Graphics</param>
        public BuiltInLightPoint(Graphics graphics) : base(graphics)
        {
            SetVertexShader<DeferredLightVs>(false);
            SetPixelShader<DeferredLightPointPs>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(IEnumerable<EngineShaderResourceView> geometryMap)
        {
            var pixelShader = GetPixelShader<DeferredLightPointPs>();
            pixelShader?.SetDeferredBuffer(geometryMap);
            pixelShader?.SetPointSampler(pointSampler);
        }
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="light">Light constant buffer</param>
        public void UpdatePerLight(IEngineDeviceContext dc, ISceneLightPoint light)
        {
            var cbLight = BuiltInShaders.GetConstantBuffer<PerLight>();
            cbLight?.WriteData(PerLight.Build(light.Local));
            dc.UpdateConstantBuffer(cbLight);

            var vertexShader = GetVertexShader<DeferredLightVs>();
            vertexShader?.SetPerLightConstantBuffer(cbLight);

            var cbPoint = BuiltInShaders.GetConstantBuffer<BuiltInShaders.BufferLightPoint>();
            cbPoint?.WriteData(BuiltInShaders.BufferLightPoint.Build(light));
            dc.UpdateConstantBuffer(cbPoint);

            var pixelShader = GetPixelShader<DeferredLightPointPs>();
            pixelShader?.SetPerLightConstantBuffer(cbPoint);
        }
    }
}
