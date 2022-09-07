
namespace Engine.BuiltIn.ShadowCascade
{
    /// <summary>
    /// Shadow Skinned position-normal-texture instanced drawer
    /// </summary>
    public class BuiltInPositionNormalTextureSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<PositionNormalTextureSkinnedVsI>();
        }
    }
}
