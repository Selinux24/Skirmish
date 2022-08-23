
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow skinned position-normal-color instanced drawer
    /// </summary>
    public class ShadowSkinnedPositionNormalColorInstanced : BuiltInDrawer<ShadowSkinnedPositionNormalColorVsI, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowSkinnedPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
