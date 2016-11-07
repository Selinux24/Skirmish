using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Particles effect
    /// </summary>
    public class EffectGPUParticles : Drawer
    {
        /// <summary>
        /// Fire stream out technique
        /// </summary>
        public readonly EffectTechnique ParticleStreamOut = null;
        /// <summary>
        /// Solid drawing technique
        /// </summary>
        public readonly EffectTechnique SolidDraw = null;
        /// <summary>
        /// Line drawing technique
        /// </summary>
        public readonly EffectTechnique LineDraw = null;
        /// <summary>
        /// Solid deferred drawing technique
        /// </summary>
        public readonly EffectTechnique DeferredSolidDraw = null;
        /// <summary>
        /// Line deferred drawing technique
        /// </summary>
        public readonly EffectTechnique DeferredLineDraw = null;

        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private EffectScalarVariable totalTime = null;
        /// <summary>
        /// Time step effect variable
        /// </summary>
        private EffectScalarVariable elapsedTime = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable viewProjection = null;
        /// <summary>
        /// Emit age effect variable
        /// </summary>
        private EffectScalarVariable emissionRate = null;
        /// <summary>
        /// Minium energy effect variable
        /// </summary>
        private EffectScalarVariable energyMin = null;
        /// <summary>
        /// Maximum energy effect variable
        /// </summary>
        private EffectScalarVariable energyMax = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private EffectScalarVariable textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private EffectShaderResourceVariable textureArray = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textureRandom = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EffectScalarVariable fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EffectScalarVariable fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EffectVectorVariable fogColor = null;

        private EffectVectorVariable particleOrbit = null;

        private EffectVectorVariable particlePosition = null;
        private EffectVectorVariable particlePositionVariance = null;
        private EffectVectorVariable particleVelocity = null;
        private EffectVectorVariable particleVelocityVariance = null;
        private EffectVectorVariable particleAcceleration = null;
        private EffectVectorVariable particleAccelerationVariance = null;

        private EffectVectorVariable particleColorStart = null;
        private EffectVectorVariable particleColorStartVariance = null;
        private EffectVectorVariable particleColorEnd = null;
        private EffectVectorVariable particleColorEndVariance = null;

        private EffectScalarVariable particleSizeStartMin = null;
        private EffectScalarVariable particleSizeStartMax = null;
        private EffectScalarVariable particleSizeEndMin = null;
        private EffectScalarVariable particleSizeEndMax = null;

        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                Vector4 v = this.eyePositionWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.eyePositionWorld.Set(v4);
            }
        }
        /// <summary>
        /// Game time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return this.totalTime.GetFloat();
            }
            set
            {
                this.totalTime.Set(value);
            }
        }
        /// <summary>
        /// Time step
        /// </summary>
        protected float ElapsedTime
        {
            get
            {
                return this.elapsedTime.GetFloat();
            }
            set
            {
                this.elapsedTime.Set(value);
            }
        }
        /// <summary>
        /// Emit age
        /// </summary>
        protected float EmissionRate
        {
            get
            {
                return this.emissionRate.GetFloat();
            }
            set
            {
                this.emissionRate.Set(value);
            }
        }
        /// <summary>
        /// Minimum energy
        /// </summary>
        protected float EnergyMin
        {
            get
            {
                return this.energyMin.GetFloat();
            }
            set
            {
                this.energyMin.Set(value);
            }
        }
        /// <summary>
        /// Maximum energy
        /// </summary>
        protected float EnergyMax
        {
            get
            {
                return this.energyMax.GetFloat();
            }
            set
            {
                this.energyMax.Set(value);
            }
        }
        /// <summary>
        /// View projection matrix
        /// </summary>
        protected Matrix ViewProjection
        {
            get
            {
                return this.viewProjection.GetMatrix();
            }
            set
            {
                this.viewProjection.SetMatrix(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return (uint)this.textureCount.GetInt();
            }
            set
            {
                this.textureCount.Set(value);
            }
        }
        /// <summary>
        /// Textures
        /// </summary>
        protected ShaderResourceView TextureArray
        {
            get
            {
                return this.textureArray.GetResource();
            }
            set
            {
                this.textureArray.SetResource(value);
            }
        }
        /// <summary>
        /// Random texture
        /// </summary>
        protected ShaderResourceView TextureRandom
        {
            get
            {
                return this.textureRandom.GetResource();
            }
            set
            {
                this.textureRandom.SetResource(value);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return this.fogStart.GetFloat();
            }
            set
            {
                this.fogStart.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return this.fogRange.GetFloat();
            }
            set
            {
                this.fogRange.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color4 FogColor
        {
            get
            {
                return new Color4(this.fogColor.GetFloatVector());
            }
            set
            {
                this.fogColor.Set(value);
            }
        }

        protected Vector4 ParticleOrbit
        {
            get
            {
                return this.particleOrbit.GetFloatVector();
            }
            set
            {
                this.particleOrbit.Set(value);
            }
        }

        protected Vector3 ParticlePosition
        {
            get
            {
                Vector4 v = this.particlePosition.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particlePosition.Set(v4);
            }
        }
        protected Vector3 ParticlePositionVariance
        {
            get
            {
                Vector4 v = this.particlePositionVariance.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particlePositionVariance.Set(v4);
            }
        }
        protected Vector3 ParticleVelocity
        {
            get
            {
                Vector4 v = this.particleVelocity.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particleVelocity.Set(v4);
            }
        }
        protected Vector3 ParticleVelocityVariance
        {
            get
            {
                Vector4 v = this.particleVelocityVariance.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particleVelocityVariance.Set(v4);
            }
        }
        protected Vector3 ParticleAcceleration
        {
            get
            {
                Vector4 v = this.particleAcceleration.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particleAcceleration.Set(v4);
            }
        }
        protected Vector3 ParticleAccelerationVariance
        {
            get
            {
                Vector4 v = this.particleAccelerationVariance.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particleAccelerationVariance.Set(v4);
            }
        }

        protected Color4 ParticleColorStart
        {
            get
            {
                return new Color4(this.particleColorStart.GetFloatVector());
            }
            set
            {
                this.particleColorStart.Set(value);
            }
        }
        protected Color4 ParticleColorStartVariance
        {
            get
            {
                return new Color4(this.particleColorStartVariance.GetFloatVector());
            }
            set
            {
                this.particleColorStartVariance.Set(value);
            }
        }
        protected Color4 ParticleColorEnd
        {
            get
            {
                return new Color4(this.particleColorEnd.GetFloatVector());
            }
            set
            {
                this.particleColorEnd.Set(value);
            }
        }
        protected Color4 ParticleColorEndVariance
        {
            get
            {
                return new Color4(this.particleColorEndVariance.GetFloatVector());
            }
            set
            {
                this.particleColorEndVariance.Set(value);
            }
        }

        protected float ParticleSizeStartMin
        {
            get
            {
                return this.particleSizeStartMin.GetFloat();
            }
            set
            {
                this.particleSizeStartMin.Set(value);
            }
        }
        protected float ParticleSizeStartMax
        {
            get
            {
                return this.particleSizeStartMax.GetFloat();
            }
            set
            {
                this.particleSizeStartMax.Set(value);
            }
        }
        protected float ParticleSizeEndMin
        {
            get
            {
                return this.particleSizeEndMin.GetFloat();
            }
            set
            {
                this.particleSizeEndMin.Set(value);
            }
        }
        protected float ParticleSizeEndMax
        {
            get
            {
                return this.particleSizeEndMax.GetFloat();
            }
            set
            {
                this.particleSizeEndMax.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectGPUParticles(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ParticleStreamOut = this.Effect.GetTechniqueByName("ParticleStreamOut");
            this.SolidDraw = this.Effect.GetTechniqueByName("SolidDraw");
            this.LineDraw = this.Effect.GetTechniqueByName("LineDraw");
            this.DeferredSolidDraw = this.Effect.GetTechniqueByName("DeferredSolidDraw");
            this.DeferredLineDraw = this.Effect.GetTechniqueByName("DeferredLineDraw");

            this.AddInputLayout(this.ParticleStreamOut, VertexGPUParticle.GetInput());
            this.AddInputLayout(this.SolidDraw, VertexGPUParticle.GetInput());
            this.AddInputLayout(this.LineDraw, VertexGPUParticle.GetInput());

            this.emissionRate = this.Effect.GetVariableByName("gEmissionRate").AsScalar();
            this.energyMin = this.Effect.GetVariableByName("gEnergyMin").AsScalar();
            this.energyMax = this.Effect.GetVariableByName("gEnergyMax").AsScalar();
            this.totalTime = this.Effect.GetVariableByName("gTotalTime").AsScalar();
            this.elapsedTime = this.Effect.GetVariableByName("gElapsedTime").AsScalar();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.viewProjection = this.Effect.GetVariableByName("gViewProjection").AsMatrix();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();
            this.textureArray = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
            this.textureRandom = this.Effect.GetVariableByName("gTextureRandom").AsShaderResource();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();

            this.particleOrbit = this.Effect.GetVariableByName("gParticleOrbit").AsVector();

            this.particlePosition = this.Effect.GetVariableByName("gParticlePosition").AsVector();
            this.particlePositionVariance = this.Effect.GetVariableByName("gParticlePositionVariance").AsVector();
            this.particleVelocity = this.Effect.GetVariableByName("gParticleVelocity").AsVector();
            this.particleVelocityVariance = this.Effect.GetVariableByName("gParticleVelocityVariance").AsVector();
            this.particleAcceleration = this.Effect.GetVariableByName("gParticleAcceleration").AsVector();
            this.particleAccelerationVariance = this.Effect.GetVariableByName("gParticleAccelerationVariance").AsVector();

            this.particleColorStart = this.Effect.GetVariableByName("gParticleColorStart").AsVector();
            this.particleColorStartVariance = this.Effect.GetVariableByName("gParticleColorStartVariance").AsVector();
            this.particleColorEnd = this.Effect.GetVariableByName("gParticleColorEnd").AsVector();
            this.particleColorEndVariance = this.Effect.GetVariableByName("gParticleColorEndVariance").AsVector();

            this.particleSizeStartMin = this.Effect.GetVariableByName("gSizeStartMin").AsScalar();
            this.particleSizeStartMax = this.Effect.GetVariableByName("gSizeStartMax").AsScalar();
            this.particleSizeEndMin = this.Effect.GetVariableByName("gSizeEndMin").AsScalar();
            this.particleSizeEndMax = this.Effect.GetVariableByName("gSizeEndMax").AsScalar();
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
            throw new Exception(string.Format("Bad stage for effect. Use particle class: {0}", stage));
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EffectTechnique GetTechniqueForStreamOut(VertexTypes vertexType)
        {
            if (vertexType == VertexTypes.GPUParticle)
            {
                return this.ParticleStreamOut;
            }
            else
            {
                throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, DrawingStages.StreamOut));
            }
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="drawerMode">Drawer mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EffectTechnique GetTechniqueForDrawing(VertexTypes vertexType, DrawerModesEnum drawerMode)
        {
            if (vertexType == VertexTypes.GPUParticle)
            {
                if (drawerMode == DrawerModesEnum.Forward || drawerMode == DrawerModesEnum.ShadowMap)
                {
                    return this.SolidDraw;
                }
                else if (drawerMode == DrawerModesEnum.Deferred)
                {
                    return this.DeferredSolidDraw;
                }
                else
                {
                    throw new Exception(string.Format("Bad drawer mode for effect and stage: {0} - {1}", vertexType, DrawingStages.Drawing));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, DrawingStages.Drawing));
            }
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="lights">Scene lights</param>
        /// <param name="randomTexture">Random texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            ShaderResourceView randomTexture)
        {
            this.ViewProjection = viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.TextureRandom = randomTexture;

            if (lights != null)
            {
                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;
            }
            else
            {
                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;
            }
        }

        public void UpdatePerEmitter(
            float totalTime,
            float elapsedTime,
            float emissionRate,
            uint textureCount,
            ShaderResourceView textures,
            float energyMin,
            float energyMax,
            bool ellipsoid,
            bool orbitPosition,
            bool orbitVelocity,
            bool orbitAcceleration,
            float sizeStartMin,
            float sizeStartMax,
            float sizeEndMin,
            float sizeEndMax,
            Color4 colorStart,
            Color4 colorStartVar,
            Color4 colorEnd,
            Color4 colorEndVar,
            Vector3 position,
            Vector3 positionVar,
            Vector3 velocity,
            Vector3 velocityVar,
            Vector3 acceleration,
            Vector3 accelerationVar)
        {
            this.TotalTime = totalTime;
            this.ElapsedTime = elapsedTime;

            this.EmissionRate = emissionRate;

            this.TextureCount = textureCount;
            this.TextureArray = textures;

            this.EnergyMin = energyMin;
            this.EnergyMin = energyMax;

            this.ParticleOrbit = new Vector4(
                orbitPosition ? 1 : 0,
                orbitVelocity ? 1 : 0,
                orbitAcceleration ? 1 : 0,
                ellipsoid ? 1 : 0);

            this.ParticleSizeStartMin = sizeStartMin;
            this.ParticleSizeStartMax = sizeStartMax;
            this.ParticleSizeEndMin = sizeEndMin;
            this.ParticleSizeEndMax = sizeEndMax;

            this.ParticleColorStart = colorStart;
            this.ParticleColorStartVariance = colorStartVar;
            this.ParticleColorEnd = colorEnd;
            this.ParticleColorEndVariance = colorEndVar;

            this.ParticlePosition = position;
            this.ParticlePositionVariance = positionVar;
            this.ParticleVelocity = velocity;
            this.ParticleVelocityVariance = velocityVar;
            this.ParticleAcceleration = acceleration;
            this.ParticleAccelerationVariance = accelerationVar;
        }
    }
}
