
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
            this.DepthEnabled = false;
            this.BlendMode = BlendModes.Opaque;

            this.Geometry = CubeMapGeometry.Sphere;
            this.ReverseFaces = true;
        }
    }
}
