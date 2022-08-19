using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.Common;

    /// <summary>
    /// Skinned position-normal-texture-tangent drawer
    /// </summary>
    public class SkinnedPositionNormalTextureTangent : BuiltInDrawer<SkinnedPositionNormalTextureTangentVs, PositionNormalTextureTangentPs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public SkinnedPositionNormalTextureTangent(Graphics graphics) : base(graphics)
        {

        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            VertexShader.WriteCBPerInstance(material, tintColor, textureIndex, animation);

            PixelShader.SetDiffuseMap(material.Material?.DiffuseTexture);
            PixelShader.SetNormalMap(material.Material?.NormalMap);
        }
    }
}
