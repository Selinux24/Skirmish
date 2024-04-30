using SharpDX;
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
        /// Vertex shader
        /// </summary>
        private readonly DeferredLightVs vertexShader;
        /// <summary>
        /// Pixel shader
        /// </summary>
        private readonly DeferredLightPointPs pixelShader;
        /// <summary>
        /// Per light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerLight> cbLight;
        /// <summary>
        /// Point light constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<BuiltInShaders.BufferLightPoint> cbPoint;
        /// <summary>
        /// Point sampler
        /// </summary>
        private readonly EngineSamplerState pointSampler;

        /// <summary>
        /// Constructor
        /// </summary>
        public BuiltInLightPoint() : base()
        {
            vertexShader = SetVertexShader<DeferredLightVs>(false);
            pixelShader = SetPixelShader<DeferredLightPointPs>(false);

            cbLight = BuiltInShaders.GetConstantBuffer<PerLight>(false);
            cbPoint = BuiltInShaders.GetConstantBuffer<BuiltInShaders.BufferLightPoint>(false);

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
        public void UpdatePerLight(IEngineDeviceContext dc, ISceneLightPoint light)
        {
            dc.UpdateConstantBuffer(cbLight, PerLight.Build(light.Local));

            vertexShader.SetPerLightConstantBuffer(cbLight);

            dc.UpdateConstantBuffer(cbPoint, BuiltInShaders.BufferLightPoint.Build(light));

            pixelShader.SetPerLightConstantBuffer(cbPoint);
        }
    }
}
