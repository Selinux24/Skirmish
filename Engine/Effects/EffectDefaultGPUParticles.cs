using SharpDX;
using System;

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
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Game time effect variable
        /// </summary>
        private EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Elapsed time effect variable
        /// </summary>
        private EngineEffectVariableScalar elapsedTime = null;
        /// <summary>
        /// Texture count effect variable
        /// </summary>
        private EngineEffectVariableScalar textureCount = null;
        /// <summary>
        /// Textures effect variable
        /// </summary>
        private EngineEffectVariableTexture textureArray = null;

        /// <summary>
        /// Emission age effect variable
        /// </summary>
        private EngineEffectVariableScalar emissionRate = null;
        /// <summary>
        /// Velocity sensitivity effect variable
        /// </summary>
        private EngineEffectVariableScalar velocitySensitivity = null;
        /// <summary>
        /// Horizontal velocity effect variable
        /// </summary>
        private EngineEffectVariableVector horizontalVelocity = null;
        /// <summary>
        /// Vertical velocity effect variable
        /// </summary>
        private EngineEffectVariableVector verticalVelocity = null;
        /// <summary>
        /// Random values effect variable
        /// </summary>
        private EngineEffectVariableVector randomValues = null;

        /// <summary>
        /// Maximum particle duration variable
        /// </summary>
        private EngineEffectVariableScalar maxDuration = null;
        /// <summary>
        /// Maximum particle duration randomness variable
        /// </summary>
        private EngineEffectVariableScalar maxDurationRandomness = null;
        /// <summary>
        /// End velocity variable
        /// </summary>
        private EngineEffectVariableScalar endVelocity = null;
        /// <summary>
        /// Gravity variable
        /// </summary>
        private EngineEffectVariableVector gravity = null;
        /// <summary>
        /// Starting size variable
        /// </summary>
        private EngineEffectVariableVector startSize = null;
        /// <summary>
        /// Ending size variable
        /// </summary>
        private EngineEffectVariableVector endSize = null;
        /// <summary>
        /// Minimum color variable
        /// </summary>
        private EngineEffectVariableVector minColor = null;
        /// <summary>
        /// Maximum color variable
        /// </summary>
        private EngineEffectVariableVector maxColor = null;
        /// <summary>
        /// Rotation speed variable
        /// </summary>
        private EngineEffectVariableVector rotateSpeed = null;

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
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultGPUParticles(Graphics graphics, byte[] effect, bool compile)
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
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            throw new Exception(string.Format("Bad stage for effect. Use particle class: {0}", stage));
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public EngineEffectTechnique GetTechniqueForStreamOut(VertexTypes vertexType)
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
        public virtual EngineEffectTechnique GetTechniqueForDrawing(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode, bool rotation)
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
            EngineShaderResourceView textures)
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
