
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Skinned position-normal-color instanced drawer
    /// </summary>
    public class SkinnedPositionNormalColorInstanced : BuiltInDrawer<SkinnedPositionNormalColorVsI, PositionNormalColorPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
