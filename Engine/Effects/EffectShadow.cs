using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Shadows generation effect
    /// </summary>
    public class EffectShadow : Drawer, IShadowMapDrawer
    {
        #region Technique variables

        /// <summary>
        /// Spot shadows
        /// </summary>
        protected readonly EngineEffectTechnique SpotShadowGen = null;
        /// <summary>
        /// Point shadows
        /// </summary>
        protected readonly EngineEffectTechnique PointShadowGen = null;
        /// <summary>
        /// Cascaded shadows
        /// </summary>
        protected readonly EngineEffectTechnique CascadedShadowMapsGen = null;

        #endregion

        /// <summary>
        /// Spot light shadow matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix shadowMat = null;
        /// <summary>
        /// Point light cube view*projection matrix array effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix cubeViewProj = null;
        /// <summary>
        /// Cascade view*projection matrix array effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix cascadeViewProj = null;

        /// <summary>
        /// Spot light shadow matrix
        /// </summary>
        protected Matrix ShadowMat
        {
            get
            {
                return this.shadowMat.GetMatrix();
            }
            set
            {
                this.shadowMat.SetMatrix(value);
            }
        }
        /// <summary>
        /// Point light cube view*projection matrix array
        /// </summary>
        protected Matrix[] CubeViewProj
        {
            get
            {
                return this.cubeViewProj.GetMatrixArray(6);
            }
            set
            {
                if (value == null)
                {
                    this.cubeViewProj.SetMatrix(new Matrix[6]);
                }
                else
                {
                    this.cubeViewProj.SetMatrix(value);
                }
            }
        }
        /// <summary>
        /// Cascade view*projection matrix array
        /// </summary>
        protected Matrix[] CascadeViewProj
        {
            get
            {
                return this.cascadeViewProj.GetMatrixArray(3);
            }
            set
            {
                if (value == null)
                {
                    this.cascadeViewProj.SetMatrix(new Matrix[3]);
                }
                else
                {
                    this.cascadeViewProj.SetMatrix(value);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadow(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.SpotShadowGen = this.Effect.GetTechniqueByName("SpotShadowGen");
            this.PointShadowGen = this.Effect.GetTechniqueByName("PointShadowGen");
            this.CascadedShadowMapsGen = this.Effect.GetTechniqueByName("CascadedShadowMapsGen");

            this.shadowMat = this.Effect.GetVariableMatrix("ShadowMat");
            this.cubeViewProj = this.Effect.GetVariableMatrix("CubeViewProj");
            this.cascadeViewProj = this.Effect.GetVariableMatrix("CascadeViewProj");
        }

        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="transparent">Use transparent textures</param>
        /// <returns>Returns the technique to process the specified vertex type</returns>
        public EngineEffectTechnique GetTechnique(
            VertexTypes vertexType,
            bool instanced,
            bool transparent)
        {
            return CascadedShadowMapsGen;
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="animationPalette">Animation palette texture</param>
        /// <param name="animationPaletteWith">Animation palette texture width</param>
        public void UpdateGlobals(
            EngineShaderResourceView animationPalette,
            uint animationPaletteWidth)
        {

        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="context">Context</param>
        public void UpdatePerFrame(
            Matrix world,
            DrawContextShadows context)
        {
            this.CascadeViewProj = context.ShadowMap.FromLightViewProjectionArray;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="animationOffset">Animation index</param>
        /// <param name="material">Material</param>
        /// <param name="textureIndex">Texture index</param>
        public void UpdatePerObject(
            uint animationOffset,
            MeshMaterial material,
            uint textureIndex)
        {

        }
    }
}
