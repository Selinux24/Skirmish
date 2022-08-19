
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Skinned position-color instanced drawer
    /// </summary>
    public class SkinnedPositionColorInstanced : BuiltInDrawer<SkinnedPositionColorVsI, PositionColorPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
