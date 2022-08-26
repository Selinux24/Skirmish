
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow position-normal-texture instanced drawer
    /// </summary>
    public class ShadowPositionNormalTextureInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalTextureVsI>();
        }
    }
}
