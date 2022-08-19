
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Basic position-color instanced drawer
    /// </summary>
    public class BasicPositionColorInstanced : BuiltInDrawer<PositionColorVsI, PositionColorPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
