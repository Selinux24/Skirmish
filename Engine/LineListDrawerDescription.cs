
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
            this.CastShadow = false;
            this.DeferredEnabled = false;
            this.DepthEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
