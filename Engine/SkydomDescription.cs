
namespace Engine
{
    /// <summary>
    /// Skydom descriptor
    /// </summary>
    public class SkydomDescription : CubemapDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SkydomDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = false;
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = false;

            this.Geometry = CubeMapGeometryEnum.Semispehere;
            this.ReverseFaces = true;
        }
    }
}
