
namespace Engine
{
    /// <summary>
    /// Cube-map description
    /// </summary>
    public class CubemapDescription : DrawableDescription
    {
        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath = "Resources";
        /// <summary>
        /// Texture
        /// </summary>
        public string Texture;
        /// <summary>
        /// Radius
        /// </summary>
        public float Radius;

        /// <summary>
        /// Constructor
        /// </summary>
        public CubemapDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = false;
            this.CastShadow = false;
            this.DeferredEnabled = true;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = false;
        }
    }
}
