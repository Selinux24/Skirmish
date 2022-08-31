
namespace Engine.BuiltIn.DefaultShadow
{
    using Engine.Common;

    /// <summary>
    /// Shadow Skinned position-normal-texture-tangent drawer
    /// </summary>
    public class BuiltInPositionNormalTextureTangentSkinned : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BuiltInPositionNormalTextureTangentSkinned(Graphics graphics) : base(graphics)
        {
            SetVertexShader<SkinnedPositionNormalTextureTangentVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<SkinnedPositionNormalTextureTangentVs>();
            vertexShader?.WriteCBPerInstance(textureIndex, animation);
        }
    }
}
