
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow Skinned position-color instanced drawer
    /// </summary>
    public class ShadowPositionColorSkinnedInstanced : BuiltInDrawer<ShadowSkinnedPositionColorVsI, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
