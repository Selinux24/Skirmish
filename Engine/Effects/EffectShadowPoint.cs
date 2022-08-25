using Engine.Shaders.Properties;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectShadowPoint : EffectShadowBase
    {
        /// <summary>
        /// Map box sides
        /// </summary>
        public const int BoxSides = 6;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix[] WorldViewProjectionArray
        {
            get
            {
                return WorldViewProjectionVariable.GetMatrixArray(BoxSides);
            }
            set
            {
                if (value == null)
                {
                    WorldViewProjectionVariable.SetMatrix(new Matrix[BoxSides]);
                }
                else
                {
                    WorldViewProjectionVariable.SetMatrix(value);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public EffectShadowPoint(Graphics graphics)
            : base(graphics, EffectsResources.ShaderShadowPoints, true)
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
            var viewProjection = context.ShadowMap.FromLightViewProjectionArray;

            if (viewProjection?.Length > 0)
            {
                if (viewProjection.Length != BoxSides)
                {
                    throw new EngineException($"The matrix array must have a length of {BoxSides}");
                }

                var m = new Matrix[viewProjection.Length];
                for (int i = 0; i < viewProjection.Length; i++)
                {
                    m[i] = world * viewProjection[i];
                }

                WorldViewProjectionArray = m;
            }
            else
            {
                WorldViewProjectionArray = null;
            }
        }
        /// <inheritdoc/>
        public override void UpdatePerObject(
            AnimationShadowDrawInfo animation,
            MaterialShadowDrawInfo material,
            uint textureIndex)
        {
            AnimationOffset = animation.Offset1;
            AnimationOffset2 = animation.Offset2;
            AnimationInterpolation = animation.InterpolationAmount;

            DiffuseMap = material.Material?.DiffuseTexture;

            TextureIndex = textureIndex;
        }
    }
}
