
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Shadows;

    /// <summary>
    /// Shadow position-color instanced drawer
    /// </summary>
    public class ShadowPositionColorInstanced : BuiltInDrawer<ShadowPositionColorVsI, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
