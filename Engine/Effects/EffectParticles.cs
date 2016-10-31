using System;
using SharpDX;
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
    public class EffectParticles : Drawer
    {
        /// <summary>
        /// Fire stream out technique
        /// </summary>
        public readonly EffectTechnique ParticleStreamOut = null;
        /// <summary>
        /// Rain stream out technique
        /// </summary>
        public readonly EffectTechnique ParticleDraw = null;
        /// <summary>
        /// Smoke stream out technique
        /// </summary>
        public readonly EffectTechnique DeferredParticleDraw = null;

        /// <summary>
        /// World effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
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
        /// <summary>
        /// Time step effect variable
        /// </summary>
        private EffectScalarVariable elapsedTime = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textureRandom = null;

        private EffectVectorVariable position = null;
        private EffectMatrixVariable rotation = null;
        /// <summary>
        /// Emit age effect variable
        /// </summary>
        private EffectScalarVariable emissionRate = null;
        private EffectVectorVariable particleOrbit = null;
        private EffectScalarVariable particleEllipsoid = null;
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
        private EffectScalarVariable particleEnergyMax = null;
        private EffectScalarVariable particleEnergyMin = null;
        private EffectScalarVariable particleSizeStartMax = null;
        private EffectScalarVariable particleSizeStartMin = null;
        private EffectScalarVariable particleSizeEndMax = null;
        private EffectScalarVariable particleSizeEndMin = null;
        private EffectScalarVariable particleRotationPerParticleSpeedMin = null;
        private EffectScalarVariable particleRotationPerParticleSpeedMax = null;
        private EffectVectorVariable particleRotationAxis = null;
        private EffectVectorVariable particleRotationAxisVariance = null;
        private EffectScalarVariable particleRotationSpeedMin = null;
        private EffectScalarVariable particleRotationSpeedMax = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private EffectScalarVariable textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private EffectShaderResourceVariable textureArray = null;

        /// <summary>
        /// World
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

        protected Vector3 Position
        {
            get
            {
                Vector4 v = this.position.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.position.Set(v4);
            }
        }
        protected Matrix Rotation
        {
            get
            {
                return this.rotation.GetMatrix();
            }
            set
            {
                this.rotation.SetMatrix(value);
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
        protected float ParticleEllipsoid
        {
            get
            {
                return this.particleEllipsoid.GetFloat();
            }
            set
            {
                this.particleEllipsoid.Set(value);
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
        protected float ParticleEnergyMax
        {
            get
            {
                return this.particleEnergyMax.GetFloat();
            }
            set
            {
                this.particleEnergyMax.Set(value);
            }
        }
        protected float ParticleEnergyMin
        {
            get
            {
                return this.particleEnergyMin.GetFloat();
            }
            set
            {
                this.particleEnergyMin.Set(value);
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
        protected float ParticleRotationPerParticleSpeedMin
        {
            get
            {
                return this.particleRotationPerParticleSpeedMin.GetFloat();
            }
            set
            {
                this.particleRotationPerParticleSpeedMin.Set(value);
            }
        }
        protected float ParticleRotationPerParticleSpeedMax
        {
            get
            {
                return this.particleRotationPerParticleSpeedMax.GetFloat();
            }
            set
            {
                this.particleRotationPerParticleSpeedMax.Set(value);
            }
        }
        protected Vector3 ParticleRotationAxis
        {
            get
            {
                Vector4 v = this.particleRotationAxis.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particleRotationAxis.Set(v4);
            }
        }
        protected Vector3 ParticleRotationAxisVariance
        {
            get
            {
                Vector4 v = this.particleRotationAxisVariance.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.particleRotationAxisVariance.Set(v4);
            }
        }
        protected float ParticleRotationSpeedMin
        {
            get
            {
                return this.particleRotationSpeedMin.GetFloat();
            }
            set
            {
                this.particleRotationSpeedMin.Set(value);
            }
        }
        protected float ParticleRotationSpeedMax
        {
            get
            {
                return this.particleRotationSpeedMax.GetFloat();
            }
            set
            {
                this.particleRotationSpeedMax.Set(value);
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
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectParticles(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ParticleStreamOut = this.Effect.GetTechniqueByName("ParticleStreamOut");
            this.ParticleDraw = this.Effect.GetTechniqueByName("ParticleDraw");
            this.DeferredParticleDraw = this.Effect.GetTechniqueByName("DeferredParticleDraw");

            this.AddInputLayout(this.ParticleStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.ParticleDraw, VertexParticle.GetInput());
            this.AddInputLayout(this.DeferredParticleDraw, VertexParticle.GetInput());

            //Per frame
            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.elapsedTime = this.Effect.GetVariableByName("gElapsedTime").AsScalar();
            this.textureRandom = this.Effect.GetVariableByName("gTextureRandom").AsShaderResource();

            //Per emitter
            this.position = this.Effect.GetVariableByName("gPosition").AsVector();
            this.rotation = this.Effect.GetVariableByName("gRotation").AsMatrix();
            this.emissionRate = this.Effect.GetVariableByName("gEmissionRate").AsScalar();
            this.particleOrbit = this.Effect.GetVariableByName("gParticleOrbit").AsVector();
            this.particleEllipsoid = this.Effect.GetVariableByName("gParticleEllipsoid").AsScalar();
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
            this.particleEnergyMax = this.Effect.GetVariableByName("gParticleEnergyMax").AsScalar();
            this.particleEnergyMin = this.Effect.GetVariableByName("gParticleEnergyMin").AsScalar();
            this.particleSizeStartMax = this.Effect.GetVariableByName("gParticleSizeStartMax").AsScalar();
            this.particleSizeStartMin = this.Effect.GetVariableByName("gParticleSizeStartMin").AsScalar();
            this.particleSizeEndMax = this.Effect.GetVariableByName("gParticleSizeEndMax").AsScalar();
            this.particleSizeEndMin = this.Effect.GetVariableByName("gParticleSizeEndMin").AsScalar();
            this.particleRotationPerParticleSpeedMin = this.Effect.GetVariableByName("gParticleRotationPerParticleSpeedMin").AsScalar();
            this.particleRotationPerParticleSpeedMax = this.Effect.GetVariableByName("gParticleRotationPerParticleSpeedMax").AsScalar();
            this.particleRotationAxis = this.Effect.GetVariableByName("gParticleRotationAxis").AsVector();
            this.particleRotationAxisVariance = this.Effect.GetVariableByName("gParticleRotationAxisVariance").AsVector();
            this.particleRotationSpeedMin = this.Effect.GetVariableByName("gParticleRotationSpeedMin").AsScalar();
            this.particleRotationSpeedMax = this.Effect.GetVariableByName("gParticleRotationSpeedMax").AsScalar();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();
            this.textureArray = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
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
        /// <param name="particleClass">Particle class</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EffectTechnique GetTechniqueForStreamOut(VertexTypes vertexType)
        {
            if (vertexType == VertexTypes.Particle)
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
        /// <param name="particleClass">Particle class</param>
        /// <param name="drawerMode">Drawer mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EffectTechnique GetTechniqueForDrawing(VertexTypes vertexType, DrawerModesEnum drawerMode)
        {
            if (vertexType == VertexTypes.Particle)
            {
                if (drawerMode == DrawerModesEnum.Forward || drawerMode == DrawerModesEnum.ShadowMap)
                {
                    return this.ParticleDraw;
                }
                else if (drawerMode == DrawerModesEnum.Deferred)
                {
                    return this.DeferredParticleDraw;
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

        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            float elapsedTime,
            ShaderResourceView randomTexture)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.Position = world.TranslationVector;
            Matrix rot = world;
            rot.TranslationVector = new Vector3(0);
            this.Rotation = rot;

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

            this.ElapsedTime = elapsedTime;

            this.TextureRandom = randomTexture;
        }

        public void UpdatePerEmitter(
            float emissionRate,
            bool particleOrbitPosition,
            bool particleOrbitVelocity,
            bool particleOrbitAcceleration,
            bool particleEllipsoid,
            Vector3 particlePosition,
            Vector3 particlePositionVariance,
            Vector3 particleVelocity,
            Vector3 particleVelocityVariance,
            Vector3 particleAcceleration,
            Vector3 particleAccelerationVariance,
            Color4 particleColorStart,
            Color4 particleColorStartVariance,
            Color4 particleColorEnd,
            Color4 particleColorEndVariance,
            float particleEnergyMax,
            float particleEnergyMin,
            float particleSizeStartMax,
            float particleSizeStartMin,
            float particleSizeEndMax,
            float particleSizeEndMin,
            float particleRotationPerParticleSpeedMin,
            float particleRotationPerParticleSpeedMax,
            Vector3 particleRotationAxis,
            Vector3 particleRotationAxisVariance,
            float particleRotationSpeedMin,
            float particleRotationSpeedMax,
            uint textureCount,
            ShaderResourceView textures)
        {
            this.EmissionRate = emissionRate;

            this.ParticleOrbit = new Vector4(particleOrbitPosition ? 1.0f : 0.0f, particleOrbitVelocity ? 1.0f : 0.0f, particleOrbitAcceleration ? 1.0f : 0.0f, 0.0f);
            this.ParticleEllipsoid = particleEllipsoid ? 1.0f : 0.0f;
            this.ParticlePosition = particlePosition;
            this.ParticlePositionVariance = particlePositionVariance;
            this.ParticleVelocity = particleVelocity;
            this.ParticleVelocityVariance = particleVelocityVariance;
            this.ParticleAcceleration = particleAcceleration;
            this.ParticleAccelerationVariance = particleAccelerationVariance;
            this.ParticleColorStart = particleColorStart;
            this.ParticleColorStartVariance = particleColorStartVariance;
            this.ParticleColorEnd = particleColorEnd;
            this.ParticleColorEndVariance = particleColorEndVariance;
            this.ParticleEnergyMax = particleEnergyMax;
            this.ParticleEnergyMin = particleEnergyMin;
            this.ParticleSizeStartMax = particleSizeStartMax;
            this.ParticleSizeStartMin = particleSizeStartMin;
            this.ParticleSizeEndMax = particleSizeEndMax;
            this.ParticleSizeEndMin = particleSizeEndMin;
            this.ParticleRotationPerParticleSpeedMin = particleRotationPerParticleSpeedMin;
            this.ParticleRotationPerParticleSpeedMax = particleRotationPerParticleSpeedMax;
            this.ParticleRotationAxis = particleRotationAxis;
            this.ParticleRotationAxisVariance = particleRotationAxisVariance;
            this.ParticleRotationSpeedMin = particleRotationSpeedMin;
            this.ParticleRotationSpeedMax = particleRotationSpeedMax;
            this.TextureCount = textureCount;
            this.TextureArray = textures;
        }
    }
}
