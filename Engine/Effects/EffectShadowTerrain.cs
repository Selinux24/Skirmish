using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectShadowTerrain : Drawer
    {
        /// <summary>
        /// Shadow mapping technique
        /// </summary>
        protected readonly EngineEffectTechnique TerrainShadowMap = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;

        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjection.GetMatrix();
            }
            set
            {
                this.worldViewProjection.SetMatrix(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.TerrainShadowMap = this.Effect.GetTechniqueByName("TerrainShadowMap");

            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            EngineEffectTechnique technique = null;

            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Terrain)
                {
                    if (mode == DrawerModesEnum.ShadowMap) technique = this.TerrainShadowMap;
                }
            }

            if (technique == null)
            {
                throw new EngineException(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
            }

            return technique;
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.WorldViewProjection = world * viewProjection;
        }
    }
}
