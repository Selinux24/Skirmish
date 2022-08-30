
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow Skinned position-texture instanced drawer
    /// </summary>
    public class ShadowPositionTextureSkinnedInstanced : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionTextureVsI>();
        }
    }
}
