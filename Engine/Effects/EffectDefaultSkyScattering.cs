using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultSkyScattering : Drawer
    {
        /// <summary>
        /// Default sky scattering technique
        /// </summary>
        protected readonly EffectTechnique SkyScattering = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Sphere radii effect variable
        /// </summary>
        private EffectVectorVariable sphereRadii = null;
        /// <summary>
        /// 
        /// </summary>
        private EffectVectorVariable scatteringCoefficients = null;
        /// <summary>
        /// 
        /// </summary>
        private EffectVectorVariable inverseWaveLength = null;
        /// <summary>
        /// 
        /// </summary>
        private EffectVectorVariable misc = null;
        /// <summary>
        /// Back color variable
        /// </summary>
        private EffectVectorVariable backColor = null;
        /// <summary>
        /// Light direction effect variable
        /// </summary>
        private EffectVectorVariable lightDirectionWorld = null;

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
        /// Sphere radii
        /// </summary>
        protected Vector4 SphereRadii
        {
            get
            {
                return this.sphereRadii.GetFloatVector();
            }
            set
            {
                this.sphereRadii.Set(value);
            }
        }
        /// <summary>
        /// Scattering coefficients
        /// </summary>
        protected Vector4 ScatteringCoefficients
        {
            get
            {
                return this.scatteringCoefficients.GetFloatVector();
            }
            set
            {
                this.scatteringCoefficients.Set(value);
            }
        }
        /// <summary>
        /// Inverse waveLength
        /// </summary>
        protected Vector4 InverseWaveLength
        {
            get
            {
                return this.inverseWaveLength.GetFloatVector();
            }
            set
            {
                this.inverseWaveLength.Set(value);
            }
        }
        /// <summary>
        /// Misc: camera height, squared camera height, scale and scale over scale depth
        /// </summary>
        protected Vector4 Misc
        {
            get
            {
                return this.misc.GetFloatVector();
            }
            set
            {
                this.misc.Set(value);
            }
        }
        /// <summary>
        /// Back color
        /// </summary>
        protected Color4 BackColor
        {
            get
            {
                Vector4 v = this.backColor.GetFloatVector();

                return new Color4(v.X, v.Y, v.Z, v.W);
            }
            set
            {
                this.backColor.Set(value);
            }
        }
        /// <summary>
        /// Light direction
        /// </summary>
        protected Vector3 LightDirectionWorld
        {
            get
            {
                Vector4 v = this.lightDirectionWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.lightDirectionWorld.Set(v4);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultSkyScattering(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.SkyScattering = this.Effect.GetTechniqueByName("SkyScattering");

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.sphereRadii = this.Effect.GetVariableByName("gSphereRadii").AsVector();
            this.scatteringCoefficients = this.Effect.GetVariableByName("gScatteringCoeffs").AsVector();
            this.inverseWaveLength = this.Effect.GetVariableByName("gInvWaveLength").AsVector();
            this.misc = this.Effect.GetVariableByName("gMisc").AsVector();
            this.backColor = this.Effect.GetVariableByName("gBackColor").AsVector();
            this.lightDirectionWorld = this.Effect.GetVariableByName("gLightDirection").AsVector();
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
                            return this.SkyScattering;
                        case DrawerModesEnum.Deferred:
                            return this.SkyScattering; //TODO: build a proper deferred scattering effect
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
        /// Update per frame
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="planetRadius">Planet radius</param>
        /// <param name="planetAtmosphereRadius">Planet atmosphere radius from surface</param>
        /// <param name="sphereOuterRadius">Sphere inner radius</param>
        /// <param name="sphereInnerRadius">Sphere outer radius</param>
        /// <param name="skyBrightness">Sky brightness</param>
        /// <param name="rayleighScattering">Rayleigh scattering constant</param>
        /// <param name="rayleighScattering4PI">Rayleigh scattering constant * 4 * PI</param>
        /// <param name="mieScattering">Mie scattering constant</param>
        /// <param name="mieScattering4PI">Mie scattering constant * 4 * PI</param>
        /// <param name="invWaveLength4">Inverse light wave length</param>
        /// <param name="scale">Scale</param>
        /// <param name="rayleighScaleDepth">Rayleigh scale depth</param>
        /// <param name="backColor">Back color</param>
        /// <param name="lightDirection">Light direction</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float planetRadius,
            float planetAtmosphereRadius,
            float sphereOuterRadius,
            float sphereInnerRadius,
            float skyBrightness,
            float rayleighScattering,
            float rayleighScattering4PI,
            float mieScattering,
            float mieScattering4PI,
            Color4 invWaveLength4,
            float scale,
            float rayleighScaleDepth,
            Color4 backColor,
            Vector3 lightDirection)
        {
            this.WorldViewProjection = world * viewProjection;

            this.SphereRadii = new Vector4(
                sphereOuterRadius, sphereOuterRadius * sphereOuterRadius,
                sphereInnerRadius, sphereInnerRadius * sphereInnerRadius);

            this.ScatteringCoefficients = new Vector4(
                rayleighScattering * skyBrightness, rayleighScattering4PI,
                mieScattering * skyBrightness, mieScattering4PI);

            this.InverseWaveLength = invWaveLength4;

            this.Misc = new Vector4(planetRadius, planetAtmosphereRadius, scale, scale / rayleighScaleDepth);

            this.BackColor = backColor;

            this.LightDirectionWorld = -lightDirection;
        }
    }
}
