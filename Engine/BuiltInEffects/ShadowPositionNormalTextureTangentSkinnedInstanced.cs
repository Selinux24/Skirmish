
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;

    /// <summary>
    /// Shadow Skinned position-normal-texture-tangent instanced drawer
    /// </summary>
    public class ShadowPositionNormalTextureTangentSkinnedInstanced : BuiltInDrawer<ShadowSkinnedPositionNormalTextureTangentVsI, EmptyGs, EmptyPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureTangentSkinnedInstanced(Graphics graphics) : base(graphics)
        {

        }
    }
}
