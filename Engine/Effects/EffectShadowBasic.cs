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

        /// <inheritdoc/>
        public override void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {
            AnimationPalette = animationPalette;
            AnimationPaletteWidth = animationPaletteWidth;
        }
        /// <inheritdoc/>
        public override void UpdatePerFrame(
            Matrix world,
            DrawContextShadows context)
        {
            WorldViewProjection = world * context.ShadowMap.FromLightViewProjectionArray[0];
        }
        /// <inheritdoc/>
        public override void UpdatePerObject(
            uint animationOffset,
            uint animationOffset2,
            float animationInterpolation,
            IMeshMaterial material,
            uint textureIndex)
        {
            AnimationOffset = animationOffset;
            AnimationOffset2 = animationOffset2;
            AnimationInterpolation = animationInterpolation;
            DiffuseMap = material?.DiffuseTexture;
            TextureIndex = textureIndex;
        }
    }
}
