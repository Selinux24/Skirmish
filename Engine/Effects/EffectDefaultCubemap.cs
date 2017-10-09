using SharpDX;
using System;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Cube map effect
    /// </summary>
    public class EffectDefaultCubemap : Drawer
    {
        /// <summary>
        /// Cubemap drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ForwardCubemap = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EngineEffectVariableTexture cubeTexture = null;

        /// <summary>
        /// Current cube texture
        /// </summary>
        private EngineShaderResourceView currentCubeTexture = null;

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
        /// Texture
        /// </summary>
        protected EngineShaderResourceView CubeTexture
        {
            get
            {
                return this.cubeTexture.GetResource();
            }
            set
            {
                if (this.currentCubeTexture != value)
                {
                    this.cubeTexture.SetResource(value);

                    this.currentCubeTexture = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultCubemap(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ForwardCubemap = this.Effect.GetTechniqueByName("ForwardCubemap");

            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.cubeTexture = this.Effect.GetVariableTexture("gCubemap");
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
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Position)
                {
                    switch (mode)
                    {
                        case DrawerModesEnum.Forward:
                            return this.ForwardCubemap;
                        case DrawerModesEnum.Deferred:
                            return this.ForwardCubemap; //TODO: build a proper deferred cubemap
                        default:
                            throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                    }
                }
                else
                {
                    throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad stage for effect: {0}", stage));
            }
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.WorldViewProjection = world * viewProjection;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(
            EngineShaderResourceView cubeTexture)
        {
            this.CubeTexture = cubeTexture;
        }
    }
}
