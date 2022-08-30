
namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn.DefaultShadow;

    /// <summary>
    /// Shadow position-normal-texture-tangent drawer
    /// </summary>
    public class ShadowPositionNormalTextureTangent : BuiltInDrawer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public ShadowPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {
            SetVertexShader<ShadowPositionNormalTextureTangentVs>();
        }

        /// <inheritdoc/>
        public void Update(uint textureIndex)
        {
            var vertexShader = GetVertexShader<ShadowPositionNormalTextureTangentVs>();
            vertexShader?.WriteCBPerInstance(textureIndex);
        }
    }
}
