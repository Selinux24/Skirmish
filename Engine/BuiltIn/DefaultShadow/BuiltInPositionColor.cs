
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow position-color drawer
    /// </summary>
    public class BuiltInPositionColor : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionColorVs>();
        }
    }
}
