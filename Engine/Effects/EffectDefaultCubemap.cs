using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

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
        protected readonly EffectTechnique ForwardCubemap = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable cubeTexture = null;

        /// <summary>
        /// Current cube texture
        /// </summary>
        private ShaderResourceView currentCubeTexture = null;

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
        protected ShaderResourceView CubeTexture
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
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultCubemap(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ForwardCubemap = this.Effect.GetTechniqueByName("ForwardCubemap");

            this.AddInputLayout(this.ForwardCubemap, VertexPosition.GetInput());

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.cubeTexture = this.Effect.GetVariableByName("gCubemap").AsShaderResource();
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
            ShaderResourceView cubeTexture)
        {
            this.CubeTexture = cubeTexture;
        }
    }
}
