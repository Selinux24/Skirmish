using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Clouds effect
    /// </summary>
    public class EffectDefaultClouds : Drawer
    {
        /// <summary>
        /// Default clouds technique
        /// </summary>
        public readonly EffectTechnique CloudsStatic = null;
        /// <summary>
        /// Perturbed clouds technique
        /// </summary>
        public readonly EffectTechnique CloudsPerturbed = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// First layer texture effect variable
        /// </summary>
        private EffectShaderResourceVariable firstTexture = null;
        /// <summary>
        /// Second layer texture effect variable
        /// </summary>
        private EffectShaderResourceVariable secondTexture = null;
        /// <summary>
        /// Brightness
        /// </summary>
        private EffectScalarVariable brightness = null;
        /// <summary>
        /// Clouds fadding distance effect variable
        /// </summary>
        private EffectScalarVariable fadingDistance = null;

        /// <summary>
        /// First layer translation effect variable
        /// </summary>
        private EffectVectorVariable firstTranslation = null;
        /// <summary>
        /// Second layer translation effect variable
        /// </summary>
        private EffectVectorVariable secondTranslation = null;
        /// <summary>
        /// Clouds translation effect variable
        /// </summary>
        private EffectScalarVariable translation = null;
        /// <summary>
        /// Clouds scale effect variable
        /// </summary>
        private EffectScalarVariable scale = null;

        /// <summary>
        /// Current first texture
        /// </summary>
        private ShaderResourceView currentFirstTexture = null;
        /// <summary>
        /// Current second texture
        /// </summary>
        private ShaderResourceView currentSecondTexture = null;

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
        /// First layer texture
        /// </summary>
        protected ShaderResourceView FirstTexture
        {
            get
            {
                return this.firstTexture.GetResource();
            }
            set
            {
                if (this.currentFirstTexture != value)
                {
                    this.firstTexture.SetResource(value);

                    this.currentFirstTexture = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Second layer texture
        /// </summary>
        protected ShaderResourceView SecondTexture
        {
            get
            {
                return this.secondTexture.GetResource();
            }
            set
            {
                if (this.currentSecondTexture != value)
                {
                    this.secondTexture.SetResource(value);

                    this.currentSecondTexture = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Brightness
        /// </summary>
        protected float Brightness
        {
            get
            {
                return this.brightness.GetFloat();
            }
            set
            {
                this.brightness.Set(value);
            }
        }
        /// <summary>
        /// Clouds fadding distance
        /// </summary>
        protected float FadingDistance
        {
            get
            {
                return this.fadingDistance.GetFloat();
            }
            set
            {
                this.fadingDistance.Set(value);
            }
        }
        /// <summary>
        /// First layer translation
        /// </summary>
        protected Vector2 FirstTranslation
        {
            get
            {
                var v = this.firstTranslation.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                var v = new Vector4(value.X, value.Y, 0, 0);

                this.firstTranslation.Set(v);
            }
        }
        /// <summary>
        /// Second layer translation
        /// </summary>
        protected Vector2 SecondTranslation
        {
            get
            {
                var v = this.secondTranslation.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                var v = new Vector4(value.X, value.Y, 0, 0);

                this.secondTranslation.Set(v);
            }
        }
        /// <summary>
        /// Clouds translation
        /// </summary>
        protected float Translation
        {
            get
            {
                return this.translation.GetFloat();
            }
            set
            {
                this.translation.Set(value);
            }
        }
        /// <summary>
        /// Clouds scale
        /// </summary>
        protected float Scale
        {
            get
            {
                return this.scale.GetFloat();
            }
            set
            {
                this.scale.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultClouds(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.CloudsStatic = this.Effect.GetTechniqueByName("CloudsStatic");
            this.CloudsPerturbed = this.Effect.GetTechniqueByName("CloudsPerturbed");

            this.AddInputLayout(this.CloudsStatic, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.CloudsPerturbed, VertexPositionTexture.GetInput());

            this.firstTexture = this.Effect.GetVariableByName("gCloudTexture1").AsShaderResource();
            this.secondTexture = this.Effect.GetVariableByName("gCloudTexture2").AsShaderResource();

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.brightness = this.Effect.GetVariableByName("gBrightness").AsScalar();
            this.fadingDistance = this.Effect.GetVariableByName("gFadingDistance").AsScalar();

            this.firstTranslation = this.Effect.GetVariableByName("gFirstTranslation").AsVector();
            this.secondTranslation = this.Effect.GetVariableByName("gSecondTranslation").AsVector();

            this.translation = this.Effect.GetVariableByName("gTranslation").AsScalar();
            this.scale = this.Effect.GetVariableByName("gScale").AsScalar();
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
            throw new EngineException("Use technique variables directly");
        }

        /// <summary>
        /// Update per frame
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="brightness">Brightness</param>
        /// <param name="fadingDistance">FadingDistance</param>
        /// <param name="firstTexture">First texture</param>
        /// <param name="secondTexture">Second texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float brightness,
            float fadingDistance,
            ShaderResourceView firstTexture,
            ShaderResourceView secondTexture)
        {
            this.WorldViewProjection = world * viewProjection;

            this.Brightness = brightness;
            this.FadingDistance = fadingDistance;

            this.FirstTexture = firstTexture;
            this.SecondTexture = secondTexture;
        }
        /// <summary>
        /// Update static clouds
        /// </summary>
        /// <param name="firstTranslation">First layer translation</param>
        /// <param name="secondTranslation">Second layer translation</param>
        public void UpdatePerFrameStatic(
            Vector2 firstTranslation,
            Vector2 secondTranslation)
        {
            this.FirstTranslation = firstTranslation;
            this.SecondTranslation = secondTranslation;
        }
        /// <summary>
        /// Update perturbed clouds
        /// </summary>
        public void UpdatePerFramePerturbed(
            float translation,
            float scale)
        {
            this.Translation = translation;
            this.Scale = scale;
        }
    }
}
