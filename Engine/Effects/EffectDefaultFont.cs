using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Font effect
    /// </summary>
    public class EffectDefaultFont : Drawer
    {
        /// <summary>
        /// Font drawing technique
        /// </summary>
        protected readonly EffectTechnique FontDrawer = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Color effect variable
        /// </summary>
        private EffectVectorVariable color = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable texture = null;

        /// <summary>
        /// Current font texture
        /// </summary>
        private ShaderResourceView currentTexture = null;

        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.world.GetMatrix();
            }
            set
            {
                this.world.SetMatrix(value);
            }
        }
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
        /// Color
        /// </summary>
        protected Color4 Color
        {
            get
            {
                return this.color.GetVector<Color4>();
            }
            set
            {
                this.color.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected ShaderResourceView Texture
        {
            get
            {
                return this.texture.GetResource();
            }
            set
            {
                if (this.currentTexture != value)
                {
                    this.texture.SetResource(value);

                    this.currentTexture = value;

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
        public EffectDefaultFont(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.FontDrawer = this.Effect.GetTechniqueByName("FontDrawer");

            this.AddInputLayout(this.FontDrawer, VertexPositionTexture.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.color = this.Effect.GetVariableByName("gColor").AsVector();
            this.texture = this.Effect.GetVariableByName("gTexture").AsShaderResource();
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
                if (vertexType == VertexTypes.PositionTexture && mode == DrawerModesEnum.Forward)
                {
                    return this.FontDrawer;
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
        /// <param name="color">Color</param>
        /// <param name="texture">Font texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Color4 color,
            ShaderResourceView texture)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.Color = color;
            this.Texture = texture;
        }
    }
}
