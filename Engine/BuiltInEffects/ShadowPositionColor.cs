
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Shadows;

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
