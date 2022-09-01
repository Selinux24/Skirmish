
namespace Engine.BuiltIn.ShadowSpots
{
    /// <summary>
    /// Shadow position-normal-color instanced drawer
    /// </summary>
    public class BuiltInPositionNormalColorInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalColorInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalColorVsI>();
        }
    }
}
