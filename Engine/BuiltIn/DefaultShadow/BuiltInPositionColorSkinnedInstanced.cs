
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow Skinned position-color instanced drawer
    /// </summary>
    public class BuiltInPositionColorSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionColorSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionColorVsI>();
        }
    }
}
