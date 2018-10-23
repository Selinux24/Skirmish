using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Particles effect
    /// </summary>
    public class EffectDefaultGpuParticles : Drawer
    {
        /// <summary>
        /// Fire stream out technique
        /// </summary>
        public readonly EngineEffectTechnique ParticleStreamOut = null;
        /// <summary>
        /// Non rotation particles drawing technique
        /// </summary>
        public readonly EngineEffectTechnique NonRotationDraw = null;
        /// <summary>
        /// Rotation particles drawing technique
        /// </summary>
        public readonly EngineEffectTechnique RotationDraw = null;

        /// <summary>
        /// World effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Elapsed time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar elapsedTime = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureArray = null;

        /// <summary>
        /// Emission age effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar emissionRate = null;
        /// <summary>
        /// Velocity sensitivity effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar velocitySensitivity = null;
        /// <summary>
        /// Horizontal velocity effect variable
        /// </summary>
        private readonly EngineEffectVariableVector horizontalVelocity = null;
        /// <summary>
        /// Vertical velocity effect variable
        /// </summary>
        private readonly EngineEffectVariableVector verticalVelocity = null;
        /// <summary>
        /// Random values effect variable
        /// </summary>
        private readonly EngineEffectVariableVector randomValues = null;

        /// <summary>
        /// Maximum particle duration variable
        /// </summary>
        private readonly EngineEffectVariableScalar maxDuration = null;
        /// <summary>
        /// Maximum particle duration randomness variable
        /// </summary>
        private readonly EngineEffectVariableScalar maxDurationRandomness = null;
        /// <summary>
        /// End velocity variable
        /// </summary>
        private readonly EngineEffectVariableScalar endVelocity = null;
        /// <summary>
        /// Gravity variable
        /// </summary>
        private readonly EngineEffectVariableVector gravity = null;
        /// <summary>
        /// Starting size variable
        /// </summary>
        private readonly EngineEffectVariableVector startSize = null;
        /// <summary>
        /// Ending size variable
        /// </summary>
        private readonly EngineEffectVariableVector endSize = null;
        /// <summary>
        /// Minimum color variable
        /// </summary>
        private readonly EngineEffectVariableVector minColor = null;
        /// <summary>
        /// Maximum color variable
        /// </summary>
        private readonly EngineEffectVariableVector maxColor = null;
        /// <summary>
        /// Rotation speed variable
        /// </summary>
        private readonly EngineEffectVariableVector rotateSpeed = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextureArray = null;

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
                return this.eyePositionWorld.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorld.Set(value);
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
                return this.textureCount.GetUInt();
            }
            set
            {
                this.textureCount.Set(value);
            }
        }
        /// <summary>
        /// Textures
        /// </summary>
        protected EngineShaderResourceView TextureArray
        {
            get
            {
                return this.textureArray.GetResource();
            }
            set
            {
                if (this.currentTextureArray != value)
                {
                    this.textureArray.SetResource(value);

                    this.currentTextureArray = value;

                    Counters.TextureUpdates++;
                }
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
                return this.horizontalVelocity.GetVector<Vector2>();
            }
            set
            {
                this.horizontalVelocity.Set(value);
            }
        }
        /// <summary>
        /// Vertical velocity
        /// </summary>
        protected Vector2 VerticalVelocity
        {
            get
            {
                return this.verticalVelocity.GetVector<Vector2>();
            }
            set
            {
                this.verticalVelocity.Set(value);
            }
        }
        /// <summary>
        /// Random values
        /// </summary>
        protected Vector4 RandomValues
        {
            get
            {
                return this.randomValues.GetVector<Vector4>();
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
                return this.gravity.GetVector<Vector3>();
            }
            set
            {
                this.gravity.Set(value);
            }
        }
        /// <summary>
        /// Starting size
        /// </summary>
        protected Vector2 StartSize
        {
            get
            {
                return this.startSize.GetVector<Vector2>();
            }
            set
            {
                this.startSize.Set(value);
            }
        }
        /// <summary>
        /// Ending size
        /// </summary>
        protected Vector2 EndSize
        {
            get
            {
                return this.endSize.GetVector<Vector2>();
            }
            set
            {
                this.endSize.Set(value);
            }
        }
        /// <summary>
        /// Minimum color
        /// </summary>
        protected Color4 MinColor
        {
            get
            {
                return this.minColor.GetVector<Color4>();
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
                return this.maxColor.GetVector<Color4>();
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
                return this.rotateSpeed.GetVector<Vector2>();
            }
            set
            {
                this.rotateSpeed.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultGpuParticles(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ParticleStreamOut = this.Effect.GetTechniqueByName("ParticleStreamOut");
            this.NonRotationDraw = this.Effect.GetTechniqueByName("NonRotationParticle");
            this.RotationDraw = this.Effect.GetTechniqueByName("RotationParticle");

            this.world = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorld = this.Effect.GetVariableVector("gEyePositionWorld");
            this.totalTime = this.Effect.GetVariableScalar("gTotalTime");
            this.elapsedTime = this.Effect.GetVariableScalar("gElapsedTime");
            this.textureCount = this.Effect.GetVariableScalar("gTextureCount");
            this.textureArray = this.Effect.GetVariableTexture("gTextureArray");

            this.emissionRate = this.Effect.GetVariableScalar("gEmissionRate");
            this.velocitySensitivity = this.Effect.GetVariableScalar("gVelocitySensitivity");
            this.horizontalVelocity = this.Effect.GetVariableVector("gHorizontalVelocity");
            this.verticalVelocity = this.Effect.GetVariableVector("gVerticalVelocity");
            this.randomValues = this.Effect.GetVariableVector("gRandomValues");

            this.maxDuration = this.Effect.GetVariableScalar("gMaxDuration");
            this.maxDurationRandomness = this.Effect.GetVariableScalar("gMaxDurationRandomness");
            this.endVelocity = this.Effect.GetVariableScalar("gEndVelocity");
            this.gravity = this.Effect.GetVariableVector("gGravity");
            this.startSize = this.Effect.GetVariableVector("gStartSize");
            this.endSize = this.Effect.GetVariableVector("gEndSize");
            this.minColor = this.Effect.GetVariableVector("gMinColor");
            this.maxColor = this.Effect.GetVariableVector("gMaxColor");
            this.rotateSpeed = this.Effect.GetVariableVector("gRotateSpeed");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="state">Particle state</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="textures">Texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            EffectParticleState state,
            uint textureCount,
            EngineShaderResourceView textures)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePositionWorld;
            this.TotalTime = state.TotalTime;
            this.ElapsedTime = state.ElapsedTime;

            this.EmissionRate = state.EmissionRate;
            this.VelocitySensitivity = state.VelocitySensitivity;
            this.HorizontalVelocity = state.HorizontalVelocity;
            this.VerticalVelocity = state.VerticalVelocity;
            this.RandomValues = state.RandomValues;

            this.MaxDuration = state.MaxDuration;
            this.MaxDurationRandomness = state.MaxDurationRandomness;
            this.EndVelocity = state.EndVelocity;
            this.Gravity = state.Gravity;
            this.StartSize = state.StartSize;
            this.EndSize = state.EndSize;
            this.MinColor = state.MinColor;
            this.MaxColor = state.MaxColor;
            this.RotateSpeed = state.RotateSpeed;
            this.TextureCount = textureCount;
            this.TextureArray = textures;
        }
    }
}
