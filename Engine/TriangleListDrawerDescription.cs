
namespace Engine
{
    /// <summary>
    /// Triangle drawer description
    /// </summary>
    public class TriangleListDrawerDescription : ModelDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TriangleListDrawerDescription()
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
