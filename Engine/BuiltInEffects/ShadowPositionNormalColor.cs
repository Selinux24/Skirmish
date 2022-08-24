
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-normal-color drawer
    /// </summary>
    public class ShadowPositionNormalColor : BuiltInDrawer<ShadowPositionNormalColorVs, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColor(Graphics graphics) : base(graphics)
        {

        }
    }
}
