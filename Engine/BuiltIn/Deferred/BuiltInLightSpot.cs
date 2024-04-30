using SharpDX;
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
        /// Vertex shader
        /// </summary>
        private readonly DeferredLightVs vertexShader;
        /// <summary>
        /// Pixel shader
        /// </summary>
        private readonly DeferredLightSpotPs pixelShader;
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerLight> cbLight;
        /// <summary>
        /// Spot light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<BuiltInShaders.BufferLightSpot> cbSpot;
        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInLightSpot() : base()
        {
            vertexShader = SetVertexShader<DeferredLightVs>(false);
            pixelShader = SetPixelShader<DeferredLightSpotPs>(false);

            cbLight = BuiltInShaders.GetConstantBuffer<PerLight>(false);
            cbSpot = BuiltInShaders.GetConstantBuffer<BuiltInShaders.BufferLightSpot>(false);

            pointSampler = BuiltInShaders.GetSamplerPoint();
        }

        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        public void UpdateGeometryMap(EngineShaderResourceView[] geometryMap)
        {
            pixelShader.SetDeferredBuffer(geometryMap);
            pixelShader.SetPointSampler(pointSampler);
        }
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="light">Light constant buffer</param>
        public void UpdatePerLight(IEngineDeviceContext dc, ISceneLightSpot light)
        {
            dc.UpdateConstantBuffer(cbLight, PerLight.Build(light.Local));

            vertexShader.SetPerLightConstantBuffer(cbLight);

            dc.UpdateConstantBuffer(cbSpot, BuiltInShaders.BufferLightSpot.Build(light));

            pixelShader.SetPerLightConstantBuffer(cbSpot);
        }
    }
}
