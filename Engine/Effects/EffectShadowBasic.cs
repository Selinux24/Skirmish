using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectShadowBasic : EffectShadowBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowBasic(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {

        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public override void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            this.AnimationPalette = animationPalette;
            this.AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="context">Context</param>
        public override void UpdatePerFrame(
            Matrix world,
            DrawContextShadows context)
        {
            this.WorldViewProjection = world * context.ShadowMap.FromLightViewProjectionArray[0];
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        public override void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex)
        {
            this.AnimationOffset = animationOffset;
            this.DiffuseMap = material?.DiffuseTexture;
            this.TextureIndex = textureIndex;
        }
    }
}
