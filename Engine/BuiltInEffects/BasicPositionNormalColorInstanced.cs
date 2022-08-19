
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Basic position-normal-color instanced drawer
    /// </summary>
    public class BasicPositionNormalColorInstanced : BuiltInDrawer<PositionNormalColorVsI, PositionNormalColorPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
