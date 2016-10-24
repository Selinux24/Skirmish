
namespace Engine
{
    /// <summary>
    /// Line drawer description
    /// </summary>
    public class LineListDrawerDescription : ModelDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LineListDrawerDescription()
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
