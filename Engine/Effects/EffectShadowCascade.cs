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
                return this.WorldViewProjectionVariable.GetMatrixArray(MaxCascades);
            }
            set
            {
                if (value == null)
                {
                    this.WorldViewProjectionVariable.SetMatrix(new Matrix[MaxCascades]);
                }
                else
                {
                    this.WorldViewProjectionVariable.SetMatrix(value);
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

                this.WorldViewProjectionArray = m;
            }
            else
            {
                this.WorldViewProjectionArray = null;
            }
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
