
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow skinned position-normal-color instanced drawer
    /// </summary>
    public class ShadowPositionNormalColorSkinnedInstanced : BuiltInDrawer<ShadowSkinnedPositionNormalColorVsI, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
