using Engine.Shaders.Properties;
using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Particles effect
    /// </summary>
    public class EffectDefaultCpuParticles : Drawer
    {
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
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorldVar = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTimeVar = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureCountVar = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureArrayVar = null;

        /// <summary>
        /// Maximum particle duration variable
        /// </summary>
        private readonly EngineEffectVariableScalar maxDurationVar = null;
        /// <summary>
        /// Maximum particle duration randomness variable
        /// </summary>
        private readonly EngineEffectVariableScalar maxDurationRandomnessVar = null;
        /// <summary>
        /// End velocity variable
        /// </summary>
        private readonly EngineEffectVariableScalar endVelocityVar = null;
        /// <summary>
        /// Gravity variable
        /// </summary>
        private readonly EngineEffectVariableVector gravityVar = null;
        /// <summary>
        /// Starting size variable
        /// </summary>
        private readonly EngineEffectVariableVector startSizeVar = null;
        /// <summary>
        /// Ending size variable
        /// </summary>
        private readonly EngineEffectVariableVector endSizeVar = null;
        /// <summary>
        /// Minimum color variable
        /// </summary>
        private readonly EngineEffectVariableVector minColorVar = null;
        /// <summary>
        /// Maximum color variable
        /// </summary>
        private readonly EngineEffectVariableVector maxColorVar = null;
        /// <summary>
        /// Rotation speed variable
        /// </summary>
        private readonly EngineEffectVariableVector rotateSpeedVar = null;

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
                return worldVar.GetMatrix();
            }
            set
            {
                worldVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return worldViewProjectionVar.GetMatrix();
            }
            set
            {
                worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Game time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return totalTimeVar.GetFloat();
            }
            set
            {
                totalTimeVar.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return textureCountVar.GetUInt();
            }
            set
            {
                textureCountVar.Set(value);
            }
        }
        /// <summary>
        /// Textures
        /// </summary>
        protected EngineShaderResourceView TextureArray
        {
            get
            {
                return textureArrayVar.GetResource();
            }
            set
            {
                if (currentTextureArray != value)
                {
                    textureArrayVar.SetResource(value);

                    currentTextureArray = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Maximum particle duration
        /// </summary>
        protected float MaxDuration
        {
            get
            {
                return maxDurationVar.GetFloat();
            }
            set
            {
                maxDurationVar.Set(value);
            }
        }
        /// <summary>
        /// Maximum particle duration randomness
        /// </summary>
        protected float MaxDurationRandomness
        {
            get
            {
                return maxDurationRandomnessVar.GetFloat();
            }
            set
            {
                maxDurationRandomnessVar.Set(value);
            }
        }
        /// <summary>
        /// End velocity
        /// </summary>
        protected float EndVelocity
        {
            get
            {
                return endVelocityVar.GetFloat();
            }
            set
            {
                endVelocityVar.Set(value);
            }
        }
        /// <summary>
        /// Gravity
        /// </summary>
        protected Vector3 Gravity
        {
            get
            {
                return gravityVar.GetVector<Vector3>();
            }
            set
            {
                gravityVar.Set(value);
            }
        }
        /// <summary>
        /// Starting size
        /// </summary>
        protected Vector2 StartSize
        {
            get
            {
                return startSizeVar.GetVector<Vector2>();
            }
            set
            {
                startSizeVar.Set(value);
            }
        }
        /// <summary>
        /// Ending size
        /// </summary>
        protected Vector2 EndSize
        {
            get
            {
                return endSizeVar.GetVector<Vector2>();
            }
            set
            {
                endSizeVar.Set(value);
            }
        }
        /// <summary>
        /// Minimum color
        /// </summary>
        protected Color4 MinColor
        {
            get
            {
                return minColorVar.GetVector<Color4>();
            }
            set
            {
                minColorVar.Set(value);
            }
        }
        /// <summary>
        /// Maximum color
        /// </summary>
        protected Color4 MaxColor
        {
            get
            {
                return maxColorVar.GetVector<Color4>();
            }
            set
            {
                maxColorVar.Set(value);
            }
        }
        /// <summary>
        /// Rotation speed
        /// </summary>
        protected Vector2 RotateSpeed
        {
            get
            {
                return rotateSpeedVar.GetVector<Vector2>();
            }
            set
            {
                rotateSpeedVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public EffectDefaultCpuParticles(Graphics graphics)
            : base(graphics, EffectsResources.ShaderDefaultCPUParticles, true)
        {
            NonRotationDraw = Effect.GetTechniqueByName("NonRotationParticle");
            RotationDraw = Effect.GetTechniqueByName("RotationParticle");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            eyePositionWorldVar = Effect.GetVariableVector("gEyePositionWorld");
            totalTimeVar = Effect.GetVariableScalar("gTotalTime");
            textureCountVar = Effect.GetVariableScalar("gTextureCount");
            textureArrayVar = Effect.GetVariableTexture("gTextureArray");

            maxDurationVar = Effect.GetVariableScalar("gMaxDuration");
            maxDurationRandomnessVar = Effect.GetVariableScalar("gMaxDurationRandomness");
            endVelocityVar = Effect.GetVariableScalar("gEndVelocity");
            gravityVar = Effect.GetVariableVector("gGravity");
            startSizeVar = Effect.GetVariableVector("gStartSize");
            endSizeVar = Effect.GetVariableVector("gEndSize");
            minColorVar = Effect.GetVariableVector("gMinColor");
            maxColorVar = Effect.GetVariableVector("gMaxColor");
            rotateSpeedVar = Effect.GetVariableVector("gRotateSpeed");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World matrix</param>
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
            World = Matrix.Identity;
            WorldViewProjection = viewProjection;
            EyePositionWorld = eyePositionWorld;
            TotalTime = state.TotalTime;
            MaxDuration = state.MaxDuration;
            MaxDurationRandomness = state.MaxDurationRandomness;
            EndVelocity = state.EndVelocity;
            Gravity = state.Gravity;
            StartSize = state.StartSize;
            EndSize = state.EndSize;
            MinColor = state.MinColor;
            MaxColor = state.MaxColor;
            RotateSpeed = state.RotateSpeed;
            TextureCount = textureCount;
            TextureArray = textures;
        }
    }
}
