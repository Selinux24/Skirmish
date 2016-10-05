
namespace Engine
{
    /// <summary>
    /// Triangle drawer description
    /// </summary>
    public class TriangleDrawerDescription : ModelDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TriangleDrawerDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = true;
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.EnableDepthStencil = false;
            this.EnableAlphaBlending = true;
        }
    }
}
