using SharpDX;

namespace Engine.BuiltInEffects
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Default;
    using Engine.Common;

    /// <summary>
    /// Basic position-texture instanced drawer
    /// </summary>
    public class BasicPositionTextureInstanced : BuiltInDrawer
    {
        private readonly EngineSamplerState linear;
        private readonly EngineSamplerState anisotropic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics</param>
        public BasicPositionTextureInstanced(Graphics graphics) : base(graphics)
        {
            SetVertexShader<BasicPositionTextureVsI>();
            SetPixelShader<BasicPositionTexturePs>();

            linear = BuiltInShaders.GetSamplerLinear();
            anisotropic = BuiltInShaders.GetSamplerAnisotropic();
        }

        /// <inheritdoc/>
        public override void Update(MaterialDrawInfo material, Color4 tintColor, uint textureIndex, AnimationDrawInfo animation)
        {
            var vertexShader = GetVertexShader<BasicPositionTextureVsI>();
            vertexShader?.WriteCBPerObject(material, tintColor);

            var sampler = material.UseAnisotropic ? anisotropic : linear;
            var pixelShader = GetPixelShader<BasicPositionTexturePs>();
            pixelShader?.SetDiffuseMap(material.Material?.DiffuseTexture);
            pixelShader?.SetDiffseSampler(sampler);
        }
    }
}
