
namespace Engine.BuiltIn.DefaultShadow
{
    /// <summary>
    /// Shadow position-normal-texture instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureVsI>();
        }
    }
}
