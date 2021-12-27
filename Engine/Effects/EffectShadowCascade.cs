using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Shadows generation effect
    /// </summary>
    public class EffectShadowCascade : EffectShadowBase
    {
        /// <summary>
        /// Maximum number of cascades
        /// </summary>
        protected const int MaxCascades = 3;

        /// <summary>
        /// Cascade view*projection matrix array
        /// </summary>
        protected Matrix[] WorldViewProjectionArray
        {
            get
            {
                return WorldViewProjectionVariable.GetMatrixArray(MaxCascades);
            }
            set
            {
                if (value == null)
                {
                    WorldViewProjectionVariable.SetMatrix(new Matrix[MaxCascades]);
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
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowCascade(Graphics graphics, byte[] effect, bool compile)
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
            var viewProjection = context.ShadowMap.FromLightViewProjectionArray;

            if (viewProjection != null && viewProjection.Length > 0)
            {
                if (viewProjection.Length != MaxCascades)
                {
                    throw new EngineException($"The matrix array must have a length of {MaxCascades}");
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
