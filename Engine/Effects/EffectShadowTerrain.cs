using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;

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
        protected readonly EffectTechnique TerrainShadowMap = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;

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
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowTerrain(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.TerrainShadowMap = this.Effect.GetTechniqueByName("TerrainShadowMap");

            this.AddInputLayout(this.TerrainShadowMap, VertexTerrain.GetInput());

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            EffectTechnique technique = null;

            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Terrain)
                {
                    if (mode == DrawerModesEnum.ShadowMap) technique = this.TerrainShadowMap;
                }
            }

            if (technique == null)
            {
                throw new Exception(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
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
