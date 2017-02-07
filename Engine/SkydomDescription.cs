
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
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.DepthEnabled = false;
            this.AlphaEnabled = false;

            this.Geometry = CubeMapGeometryEnum.Sphere;
            this.ReverseFaces = true;
        }
    }
}
