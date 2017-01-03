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
    public class EffectDefaultGPUParticles : Drawer
    {
        /// <summary>
        /// Fire stream out technique
        /// </summary>
        protected readonly EffectTechnique ParticleStreamOut = null;
        /// <summary>
        /// Non rotation particles drawing technique
        /// </summary>
        protected readonly EffectTechnique NonRotationDraw = null;
        /// <summary>
        /// Rotation particles drawing technique
        /// </summary>
        protected readonly EffectTechnique RotationDraw = null;

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
        /// Game time effect variable
        /// </summary>
        private EffectScalarVariable totalTime = null;
        /// <summary>
        /// Elapsed time effect variable
        /// </summary>
        private EffectScalarVariable elapsedTime = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private EffectScalarVariable textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private EffectShaderResourceVariable textureArray = null;

        /// <summary>
        /// Emission age effect variable
        /// </summary>
        private EffectScalarVariable emissionRate = null;
        /// <summary>
        /// Velocity sensitivity effect variable
        /// </summary>
        private EffectScalarVariable velocitySensitivity = null;
        /// <summary>
        /// Horizontal velocity effect variable
        /// </summary>
        private EffectVectorVariable horizontalVelocity = null;
        /// <summary>
        /// Vertical velocity effect variable
        /// </summary>
        private EffectVectorVariable verticalVelocity = null;
        /// <summary>
        /// Random values effect variable
        /// </summary>
        private EffectVectorVariable randomValues = null;

        /// <summary>
        /// Maximum particle duration variable
        /// </summary>
        private EffectScalarVariable maxDuration = null;
        /// <summary>
        /// Maximum particle duration randomness variable
        /// </summary>
        private EffectScalarVariable maxDurationRandomness = null;
        /// <summary>
        /// End velocity variable
        /// </summary>
        private EffectScalarVariable endVelocity = null;
        /// <summary>
        /// Gravity variable
        /// </summary>
        private EffectVectorVariable gravity = null;
        /// <summary>
        /// Starting size variable
        /// </summary>
        private EffectVectorVariable startSize = null;
        /// <summary>
        /// Ending size variable
        /// </summary>
        private EffectVectorVariable endSize = null;
        /// <summary>
        /// Minimum color variable
        /// </summary>
        private EffectVectorVariable minColor = null;
        /// <summary>
        /// Maximum color variable
        /// </summary>
        private EffectVectorVariable maxColor = null;
        /// <summary>
        /// Rotation speed variable
        /// </summary>
        private EffectVectorVariable rotateSpeed = null;

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
        /// Elapsed time
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
        /// Emission rate
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
        /// Velocity sensitivity
        /// </summary>
        protected float VelocitySensitivity
        {
            get
            {
                return this.velocitySensitivity.GetFloat();
            }
            set
            {
                this.velocitySensitivity.Set(value);
            }
        }
        /// <summary>
        /// Horizontal velocity
        /// </summary>
        protected Vector2 HorizontalVelocity
        {
            get
            {
                var v4 = this.horizontalVelocity.GetFloatVector();

                return new Vector2(v4.X, v4.Y);
            }
            set
            {
                var v4 = new Vector4(value.X, value.Y, 0, 0);

                this.horizontalVelocity.Set(v4);
            }
        }
        /// <summary>
        /// Vertical velocity
        /// </summary>
        protected Vector2 VerticalVelocity
        {
            get
            {
                var v4 = this.verticalVelocity.GetFloatVector();

                return new Vector2(v4.X, v4.Y);
            }
            set
            {
                var v4 = new Vector4(value.X, value.Y, 0, 0);

                this.verticalVelocity.Set(v4);
            }
        }
        /// <summary>
        /// Random values
        /// </summary>
        protected Vector4 RandomValues
        {
            get
            {
                return this.randomValues.GetFloatVector();
            }
            set
            {
                this.randomValues.Set(value);
            }
        }

        /// <summary>
        /// Maximum particle duration
        /// </summary>
        protected float MaxDuration
        {
            get
            {
                return this.maxDuration.GetFloat();
            }
            set
            {
                this.maxDuration.Set(value);
            }
        }
        /// <summary>
        /// Maximum particle duration randomness
        /// </summary>
        protected float MaxDurationRandomness
        {
            get
            {
                return this.maxDurationRandomness.GetFloat();
            }
            set
            {
                this.maxDurationRandomness.Set(value);
            }
        }
        /// <summary>
        /// End velocity
        /// </summary>
        protected float EndVelocity
        {
            get
            {
                return this.endVelocity.GetFloat();
            }
            set
            {
                this.endVelocity.Set(value);
            }
        }
        /// <summary>
        /// Gravity
        /// </summary>
        protected Vector3 Gravity
        {
            get
            {
                Vector4 v = this.gravity.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.gravity.Set(v4);
            }
        }
        /// <summary>
        /// Starting size
        /// </summary>
        protected Vector2 StartSize
        {
            get
            {
                Vector4 v = this.startSize.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 1f, 1f);

                this.startSize.Set(v4);
            }
        }
        /// <summary>
        /// Ending size
        /// </summary>
        protected Vector2 EndSize
        {
            get
            {
                Vector4 v = this.endSize.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 1f, 1f);

                this.endSize.Set(v4);
            }
        }
        /// <summary>
        /// Minimum color
        /// </summary>
        protected Color4 MinColor
        {
            get
            {
                return new Color4(this.minColor.GetFloatVector());
            }
            set
            {
                this.minColor.Set(value);
            }
        }
        /// <summary>
        /// Maximum color
        /// </summary>
        protected Color4 MaxColor
        {
            get
            {
                return new Color4(this.maxColor.GetFloatVector());
            }
            set
            {
                this.maxColor.Set(value);
            }
        }
        /// <summary>
        /// Rotation speed
        /// </summary>
        protected Vector2 RotateSpeed
        {
            get
            {
                Vector4 v = this.rotateSpeed.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 1f, 1f);

                this.rotateSpeed.Set(v4);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultGPUParticles(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ParticleStreamOut = this.Effect.GetTechniqueByName("ParticleStreamOut");
            this.NonRotationDraw = this.Effect.GetTechniqueByName("NonRotationParticle");
            this.RotationDraw = this.Effect.GetTechniqueByName("RotationParticle");

            this.AddInputLayout(this.ParticleStreamOut, VertexGPUParticle.GetInput());
            this.AddInputLayout(this.NonRotationDraw, VertexGPUParticle.GetInput());
            this.AddInputLayout(this.RotationDraw, VertexGPUParticle.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.totalTime = this.Effect.GetVariableByName("gTotalTime").AsScalar();
            this.elapsedTime = this.Effect.GetVariableByName("gElapsedTime").AsScalar();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();
            this.textureArray = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();

            this.emissionRate = this.Effect.GetVariableByName("gEmissionRate").AsScalar();
            this.velocitySensitivity = this.Effect.GetVariableByName("gVelocitySensitivity").AsScalar();
            this.horizontalVelocity = this.Effect.GetVariableByName("gHorizontalVelocity").AsVector();
            this.verticalVelocity = this.Effect.GetVariableByName("gVerticalVelocity").AsVector();
            this.randomValues = this.Effect.GetVariableByName("gRandomValues").AsVector();

            this.maxDuration = this.Effect.GetVariableByName("gMaxDuration").AsScalar();
            this.maxDurationRandomness = this.Effect.GetVariableByName("gMaxDurationRandomness").AsScalar();
            this.endVelocity = this.Effect.GetVariableByName("gEndVelocity").AsScalar();
            this.gravity = this.Effect.GetVariableByName("gGravity").AsVector();
            this.startSize = this.Effect.GetVariableByName("gStartSize").AsVector();
            this.endSize = this.Effect.GetVariableByName("gEndSize").AsVector();
            this.minColor = this.Effect.GetVariableByName("gMinColor").AsVector();
            this.maxColor = this.Effect.GetVariableByName("gMaxColor").AsVector();
            this.rotateSpeed = this.Effect.GetVariableByName("gRotateSpeed").AsVector();
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
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <param name="rotation">Rotation</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public virtual EffectTechnique GetTechniqueForDrawing(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode, bool rotation)
        {
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.GPUParticle)
                {
                    switch (mode)
                    {
                        case DrawerModesEnum.Forward:
                            return rotation ? this.RotationDraw : this.NonRotationDraw;
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
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="maxDuration">Maximum particle duration</param>
        /// <param name="maxDurationRandomness">Maximum particle duration randomness</param>
        /// <param name="endVelocity">End particle velocity</param>
        /// <param name="gravity">Gravity</param>
        /// <param name="startSize">Start size</param>
        /// <param name="endSize">End size</param>
        /// <param name="minColor">Minimum color</param>
        /// <param name="maxColor">Maximum color</param>
        /// <param name="rotateSpeed">Rotation speed</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            float totalTime,
            float elapsedTime,
            float emissionRate,
            float velocitySensitivity,
            Vector2 horizontalVelocity,
            Vector2 verticalVelocity,
            Vector4 randomValues,
            float maxDuration,
            float maxDurationRandomness,
            float endVelocity,
            Vector3 gravity,
            Vector2 startSize,
            Vector2 endSize,
            Color4 minColor,
            Color4 maxColor,
            Vector2 rotateSpeed,
            uint textureCount,
            ShaderResourceView textures)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;
            this.TotalTime = totalTime;
            this.ElapsedTime = elapsedTime;

            this.EmissionRate = emissionRate;
            this.VelocitySensitivity = velocitySensitivity;
            this.HorizontalVelocity = horizontalVelocity;
            this.VerticalVelocity = verticalVelocity;
            this.RandomValues = randomValues;

            this.MaxDuration = maxDuration;
            this.MaxDurationRandomness = maxDurationRandomness;
            this.EndVelocity = endVelocity;
            this.Gravity = gravity;
            this.StartSize = startSize;
            this.EndSize = endSize;
            this.MinColor = minColor;
            this.MaxColor = maxColor;
            this.RotateSpeed = rotateSpeed;
            this.TextureCount = textureCount;
            this.TextureArray = textures;
        }
    }
}
