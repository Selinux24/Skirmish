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
        public readonly EffectTechnique FireStreamOut = null;
        /// <summary>
        /// Rain stream out technique
        /// </summary>
        public readonly EffectTechnique RainStreamOut = null;
        /// <summary>
        /// Smoke stream out technique
        /// </summary>
        public readonly EffectTechnique SmokeStreamOut = null;
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
        /// World effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Acceleration effect variable
        /// </summary>
        private EffectVectorVariable accelerationWorld = null;
        /// <summary>
        /// Emit age effect variable
        /// </summary>
        private EffectScalarVariable emitterAge = null;
        /// <summary>
        /// Maximum age effect variable
        /// </summary>
        private EffectScalarVariable maximumAge = null;
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
        /// Acceleration
        /// </summary>
        protected Vector3 AccelerationWorld
        {
            get
            {
                Vector4 v = this.accelerationWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.accelerationWorld.Set(v4);
            }
        }
        /// <summary>
        /// Emit age
        /// </summary>
        protected float EmitterAge
        {
            get
            {
                return this.emitterAge.GetFloat();
            }
            set
            {
                this.emitterAge.Set(value);
            }
        }
        /// <summary>
        /// Maximum age
        /// </summary>
        protected float MaximumAge
        {
            get
            {
                return this.maximumAge.GetFloat();
            }
            set
            {
                this.maximumAge.Set(value);
            }
        }
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectParticles(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.FireStreamOut = this.Effect.GetTechniqueByName("FireStreamOut");
            this.RainStreamOut = this.Effect.GetTechniqueByName("RainStreamOut");
            this.SmokeStreamOut = this.Effect.GetTechniqueByName("SmokeStreamOut");
            this.SolidDraw = this.Effect.GetTechniqueByName("SolidDraw");
            this.LineDraw = this.Effect.GetTechniqueByName("LineDraw");
            this.DeferredSolidDraw = this.Effect.GetTechniqueByName("DeferredSolidDraw");
            this.DeferredLineDraw = this.Effect.GetTechniqueByName("DeferredLineDraw");

            this.AddInputLayout(this.FireStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.RainStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.SmokeStreamOut, VertexParticle.GetInput());
            this.AddInputLayout(this.SolidDraw, VertexParticle.GetInput());
            this.AddInputLayout(this.LineDraw, VertexParticle.GetInput());

            this.emitterAge = this.Effect.GetVariableByName("gEmitterAge").AsScalar();
            this.maximumAge = this.Effect.GetVariableByName("gMaximumAge").AsScalar();
            this.totalTime = this.Effect.GetVariableByName("gTotalTime").AsScalar();
            this.elapsedTime = this.Effect.GetVariableByName("gElapsedTime").AsScalar();
            this.accelerationWorld = this.Effect.GetVariableByName("gAccelerationWorld").AsVector();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();
            this.textureArray = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
            this.textureRandom = this.Effect.GetVariableByName("gTextureRandom").AsShaderResource();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="stage">Stage</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage)
        {
            throw new Exception(string.Format("Bad stage for effect. Use particle class: {0}", stage));
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="particleClass">Particle class</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EffectTechnique GetTechniqueForStreamOut(VertexTypes vertexType, ParticleClasses particleClass)
        {
            if (vertexType == VertexTypes.Particle)
            {
                if (particleClass == ParticleClasses.Fire) return this.FireStreamOut;
                else if (particleClass == ParticleClasses.Smoke) return this.SmokeStreamOut;
                else if (particleClass == ParticleClasses.Rain) return this.RainStreamOut;
                else throw new Exception(string.Format("Bad particle class: {0}", particleClass));
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
        public EffectTechnique GetTechniqueForDrawing(VertexTypes vertexType, ParticleClasses particleClass, DrawerModesEnum drawerMode)
        {
            if (vertexType == VertexTypes.Particle)
            {
                if (drawerMode == DrawerModesEnum.Forward || drawerMode == DrawerModesEnum.ShadowMap)
                {
                    if (particleClass == ParticleClasses.Fire) return this.SolidDraw;
                    else if (particleClass == ParticleClasses.Smoke) return this.SolidDraw;
                    else if (particleClass == ParticleClasses.Rain) return this.LineDraw;
                    else throw new Exception(string.Format("Bad particle class: {0}", particleClass));
                }
                else if (drawerMode == DrawerModesEnum.Deferred)
                {
                    if (particleClass == ParticleClasses.Fire) return this.DeferredSolidDraw;
                    else if (particleClass == ParticleClasses.Smoke) return this.DeferredSolidDraw;
                    else if (particleClass == ParticleClasses.Rain) return this.DeferredLineDraw;
                    else throw new Exception(string.Format("Bad particle class: {0}", particleClass));
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
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Textures</param>
        /// <param name="randomTexture">Texture with random numbers</param>
        /// <param name="emitterAge">Emitter age</param>
        /// <param name="maxAge">Max particle age</param>
        /// <param name="gameTime">Game elapsed time</param>
        /// <param name="timeStep">Time step</param>
        /// <param name="accelerationWorld">Acceleration vector in world coordinates</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            uint textureCount,
            ShaderResourceView textures,
            ShaderResourceView randomTexture,
            float emitterAge,
            float maxAge,
            float gameTime,
            float timeStep,
            Vector3 accelerationWorld)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.TextureCount = textureCount;
            this.TextureArray = textures;

            this.TextureRandom = randomTexture;

            this.EmitterAge = emitterAge;
            this.MaximumAge = maxAge;
            this.TotalTime = gameTime;
            this.ElapsedTime = timeStep;
            this.AccelerationWorld = accelerationWorld;

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
    }
}
