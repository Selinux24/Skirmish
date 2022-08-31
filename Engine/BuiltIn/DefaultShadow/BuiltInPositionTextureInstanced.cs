
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow position-texture instanced drawer
    /// </summary>
    public class BuiltInPositionTextureInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionTextureInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionTextureVsI>();
        }
    }
}
