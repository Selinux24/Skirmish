
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-color drawer
    /// </summary>
    public class ShadowPositionColor : BuiltInDrawer<ShadowPositionColorVs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColor(Graphics graphics) : base(graphics)
        {

        }
    }
}
