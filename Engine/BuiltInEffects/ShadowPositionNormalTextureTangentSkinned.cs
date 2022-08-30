
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-texture-tangent drawer
    /// </summary>
    public class ShadowPositionNormalTextureTangentSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureTangentSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowSkinnedPositionNormalTextureTangentVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<ShadowSkinnedPositionNormalTextureTangentVs>();
            vertexShader?.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
