
namespace Engine
{
    /// <summary>
    /// Line drawer description
    /// </summary>
    public class LineDrawerDescription : ModelDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LineDrawerDescription()
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
