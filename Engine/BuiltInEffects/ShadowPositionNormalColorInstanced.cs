
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-normal-color instanced drawer
    /// </summary>
    public class ShadowPositionNormalColorInstanced : BuiltInDrawer<ShadowPositionNormalColorVsI, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
