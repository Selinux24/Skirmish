
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-normal-color drawer
    /// </summary>
    public class ShadowPositionNormalColor : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColor(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalColorVs>();
        }
    }
}
