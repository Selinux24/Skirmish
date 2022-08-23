
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow Skinned position-normal-texture instanced drawer
    /// </summary>
    public class ShadowPositionNormalTextureSkinnedInstanced : BuiltInDrawer<ShadowSkinnedPositionNormalTextureVsI, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureSkinnedInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
