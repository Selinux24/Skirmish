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
    /// Blur effect
    /// </summary>
    public class EffectPostBlur : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EffectTechnique Blur = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Blur direction effect variable
        /// </summary>
        private EffectVectorVariable blurDirection = null;
        /// <summary>
        /// Texture size effect variable
        /// </summary>
        private EffectVectorVariable textureSize = null;
        /// <summary>
        /// Diffuse map effect variable
        /// </summary>
        private EffectShaderResourceVariable diffuseMap = null;

        /// <summary>
        /// Current diffuse map
        /// </summary>
        private ShaderResourceView currentDiffuseMap = null;

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
        /// Blur direction
        /// </summary>
        protected Vector2 BlurDirection
        {
            get
            {
                Vector4 v = this.blurDirection.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 0f, 0f);

                this.blurDirection.Set(v4);
            }
        }
        /// <summary>
        /// Texture size
        /// </summary>
        protected Vector2 TextureSize
        {
            get
            {
                Vector4 v = this.textureSize.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 0f, 0f);

                this.textureSize.Set(v4);
            }
        }
        /// <summary>
        /// Diffuse map
        /// </summary>
        protected ShaderResourceView DiffuseMap
        {
            get
            {
                return this.diffuseMap.GetResource();
            }
            set
            {
                if (this.currentDiffuseMap != value)
                {
                    this.diffuseMap.SetResource(value);

                    this.currentDiffuseMap = value;

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
        public EffectPostBlur(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.Blur = this.Effect.GetTechniqueByName("Blur");

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.blurDirection = this.Effect.GetVariableByName("gBlurDirection").AsVector();
            this.textureSize = this.Effect.GetVariableByName("gTextureSize").AsVector();
            this.diffuseMap = this.Effect.GetVariableByName("gDiffuseMap").AsShaderResource();
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
                    return this.Blur;
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
        /// <param name="direction">Blur direction</param>
        /// <param name="size">Texture size</param>
        /// <param name="diffuseMap">DiffuseMap</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector2 direction,
            Vector2 size,
            ShaderResourceView diffuseMap)
        {
            this.WorldViewProjection = world * viewProjection;
            this.BlurDirection = direction;
            this.TextureSize = size;
            this.DiffuseMap = diffuseMap;
        }
    }
}
