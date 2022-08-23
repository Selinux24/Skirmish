
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow Skinned position-texture instanced drawer
    /// </summary>
    public class ShadowPositionTextureSkinnedInstanced : BuiltInDrawer<ShadowSkinnedPositionTextureVsI, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
