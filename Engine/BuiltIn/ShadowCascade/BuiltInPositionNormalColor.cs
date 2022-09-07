
namespace Engine.BuiltIn.ShadowCascade
{
    /// <summary>
    /// Shadow position-normal-color drawer
    /// </summary>
    public class BuiltInPositionNormalColor : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorVs>();
        }
    }
}
