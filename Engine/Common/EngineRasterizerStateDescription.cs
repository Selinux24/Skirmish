
namespace Engine.Common
{
    using DXConservativeRasterizationMode = SharpDX.Direct3D11.ConservativeRasterizationMode;
    using DXCullMode = SharpDX.Direct3D11.CullMode;
    using DXFillMode = SharpDX.Direct3D11.FillMode;
    using DXRasterizerStateDescription2 = SharpDX.Direct3D11.RasterizerStateDescription2;

    /// <summary>
    /// Describes rasterizer state.
    /// </summary>
    public struct EngineRasterizerStateDescription
    {
        public static explicit operator DXRasterizerStateDescription2(EngineRasterizerStateDescription obj)
        {
            return new DXRasterizerStateDescription2
            {
                FillMode = (DXFillMode)obj.FillMode,
                CullMode = (DXCullMode)obj.CullMode,
                IsFrontCounterClockwise = obj.IsFrontCounterClockwise,
                DepthBias = obj.DepthBias,
                DepthBiasClamp = obj.DepthBiasClamp,
                SlopeScaledDepthBias = obj.SlopeScaledDepthBias,
                IsDepthClipEnabled = obj.IsDepthClipEnabled,
                IsScissorEnabled = obj.IsScissorEnabled,
                IsMultisampleEnabled = obj.IsMultisampleEnabled,
                IsAntialiasedLineEnabled = obj.IsAntialiasedLineEnabled,
                ForcedSampleCount = obj.ForcedSampleCount,
                ConservativeRasterizationMode = (DXConservativeRasterizationMode)obj.ConservativeRasterizationMode,
            };
        }

        public static explicit operator EngineRasterizerStateDescription(DXRasterizerStateDescription2 obj)
        {
            return new EngineRasterizerStateDescription
            {
                FillMode = (FillMode)obj.FillMode,
                CullMode = (CullMode)obj.CullMode,
                IsFrontCounterClockwise = obj.IsFrontCounterClockwise,
                DepthBias = obj.DepthBias,
                DepthBiasClamp = obj.DepthBiasClamp,
                SlopeScaledDepthBias = obj.SlopeScaledDepthBias,
                IsDepthClipEnabled = obj.IsDepthClipEnabled,
                IsScissorEnabled = obj.IsScissorEnabled,
                IsMultisampleEnabled = obj.IsMultisampleEnabled,
                IsAntialiasedLineEnabled = obj.IsAntialiasedLineEnabled,
                ForcedSampleCount = obj.ForcedSampleCount,
                ConservativeRasterizationMode = (ConservativeRasterizationMode)obj.ConservativeRasterizationMode,
            };
        }

        /// <summary>
        /// A FillMode-typed value that determines the fill mode to use when rendering.
        /// </summary>
        public FillMode FillMode;
        /// <summary>
        /// A CullMode-typed value that indicates that triangles facing the specified direction are not drawn.
        /// </summary>
        public CullMode CullMode;
        /// <summary>
        /// Specifies whether a triangle is front- or back-facing. If TRUE, a triangle will
        /// be considered front-facing if its vertices are counter-clockwise on the render
        /// target and considered back-facing if they are clockwise. If SharpDX.Result.False,
        /// the opposite is true.
        /// </summary>
        public bool IsFrontCounterClockwise;
        /// <summary>
        /// Depth value added to a given pixel. For info about depth bias, see Depth Bias.
        /// </summary>
        public int DepthBias;
        /// <summary>
        /// Maximum depth bias of a pixel. For info about depth bias, see Depth Bias.
        /// </summary>
        public float DepthBiasClamp;
        /// <summary>
        /// Scalar on a given pixel's slope. For info about depth bias, see Depth Bias.
        /// </summary>
        public float SlopeScaledDepthBias;
        /// <summary>
        /// Specifies whether to enable clipping based on distance. The hardware always performs
        /// x and y clipping of rasterized coordinates. When DepthClipEnable is set to the
        /// default?TRUE, the hardware also clips the z value (that is, the hardware performs
        /// the last step of the following algorithm).
        ///  0 < w
        /// -w <= x <= w (or arbitrarily wider range if implementation uses a guard band
        /// to reduce clipping burden)
        /// -w <= y <= w (or arbitrarily wider range if implementation uses a guard band
        /// to reduce clipping burden)
        /// 0 <= z <= w
        /// When you set DepthClipEnable to SharpDX.Result.False, the hardware skips the
        /// z clipping (that is, the last step in the preceding algorithm). However, the
        /// hardware still performs the "0 < w" clipping. When z clipping is disabled, improper
        /// depth ordering at the pixel level might result. However, when z clipping is disabled,
        /// stencil shadow implementations are simplified. In other words, you can avoid
        /// complex special-case handling for geometry that goes beyond the back clipping
        /// plane.
        /// </summary>
        public bool IsDepthClipEnabled;
        /// <summary>
        /// Specifies whether to enable scissor-rectangle culling. All pixels outside an active scissor rectangle are culled.
        /// </summary>
        public bool IsScissorEnabled;
        /// <summary>
        /// Specifies whether to use the quadrilateral or alpha line anti-aliasing algorithm
        /// on multisample antialiasing (MSAA) render targets. Set to TRUE to use the quadrilateral
        /// line anti-aliasing algorithm and to SharpDX.Result.False to use the alpha line
        /// anti-aliasing algorithm. For more info about this member, see Remarks.
        /// </summary>
        public bool IsMultisampleEnabled;
        /// <summary>
        /// Specifies whether to enable line antialiasing; only applies if doing line drawing
        /// and MultisampleEnable is SharpDX.Result.False. For more info about this member,
        /// see Remarks.
        /// </summary>
        public bool IsAntialiasedLineEnabled;
        /// <summary>
        /// The sample count that is forced while UAV rendering or rasterizing. Valid values
        /// are 0, 1, 2, 4, 8, and optionally 16. 0 indicates that the sample count is not
        /// forced. Note??If you want to render with ForcedSampleCount set to 1 or greater,
        /// you must follow these guidelines: Don't bind depth-stencil views. Disable depth
        /// testing. Ensure the shader doesn't output depth. If you have any render-target
        /// views bound (D3D11_BIND_RENDER_TARGET) and ForcedSampleCount is greater than
        /// 1, ensure that every render target has only a single sample. Don't operate the
        /// shader at sample frequency. Therefore, ID3D11ShaderReflection::IsSampleFrequencyShader
        /// returns SharpDX.Result.False. Otherwise, rendering behavior is undefined. For
        /// info about how to configure depth-stencil, see Configuring Depth-Stencil Functionality.
        /// </summary>
        public int ForcedSampleCount;
        /// <summary>
        /// A ConservativeRasterizationMode-typed value that identifies whether conservative rasterization is on or off.
        /// </summary>
        public ConservativeRasterizationMode ConservativeRasterizationMode;
    }

    /// <summary>
    /// Determines the fill mode to use when rendering triangles.
    /// </summary>
    public enum FillMode
    {
        /// <summary>
        /// Draw lines connecting the vertices. Adjacent vertices are not drawn.
        /// </summary>
        Wireframe = DXFillMode.Wireframe,
        /// <summary>
        /// Fill the triangles formed by the vertices. Adjacent vertices are not drawn.
        /// </summary>
        Solid = DXFillMode.Solid
    }
    /// <summary>
    /// Indicates triangles facing a particular direction are not drawn.
    /// </summary>
    public enum CullMode
    {
        /// <summary>
        /// Always draw all triangles.
        /// </summary>
        None = DXCullMode.None,
        /// <summary>
        /// Do not draw triangles that are front-facing.
        /// </summary>
        Front = DXCullMode.Front,
        /// <summary>
        /// Do not draw triangles that are back-facing.
        /// </summary>
        Back = DXCullMode.Back
    }
    /// <summary>
    /// Identifies whether conservative rasterization is on or off.
    /// </summary>
    public enum ConservativeRasterizationMode
    {
        /// <summary>
        /// Conservative rasterization is off.
        /// </summary>
        Off = DXConservativeRasterizationMode.Off,
        /// <summary>
        /// Conservative rasterization is on.
        /// </summary>
        On = DXConservativeRasterizationMode.On
    }
}
