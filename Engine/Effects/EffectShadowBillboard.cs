using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Billboard effect
    /// </summary>
    public class EffectShadowBillboard : Drawer
    {
        /// <summary>
        /// Billboard shadow map drawing technique
        /// </summary>
        protected readonly EngineEffectTechnique ShadowMapBillboard = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Start radius
        /// </summary>
        private EngineEffectVariableScalar startRadius = null;
        /// <summary>
        /// End radius
        /// </summary>
        private EngineEffectVariableScalar endRadius = null;
        /// <summary>
        /// Wind direction effect variable
        /// </summary>
        private EngineEffectVariableVector windDirection = null;
        /// <summary>
        /// Wind strength effect variable
        /// </summary>
        private EngineEffectVariableScalar windStrength = null;
        /// <summary>
        /// Time effect variable
        /// </summary>
        private EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Texture count variable
        /// </summary>
        private EngineEffectVariableScalar textureCount = null;
        /// <summary>
        /// Toggle UV coordinates by primitive ID
        /// </summary>
        private EngineEffectVariableScalar uvToggleByPID = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EngineEffectVariableTexture textures = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private EngineEffectVariableTexture textureRandom = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextures = null;
        /// <summary>
        /// Current random texture
        /// </summary>
        private EngineShaderResourceView currentTextureRandom = null;

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
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return this.startRadius.GetFloat();
            }
            set
            {
                this.startRadius.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return this.endRadius.GetFloat();
            }
            set
            {
                this.endRadius.Set(value);
            }
        }
        /// <summary>
        /// Wind direction
        /// </summary>
        protected Vector3 WindDirection
        {
            get
            {
                return this.windDirection.GetVector<Vector3>();
            }
            set
            {
                this.windDirection.Set(value);
            }
        }
        /// <summary>
        /// Wind strength
        /// </summary>
        protected float WindStrength
        {
            get
            {
                return this.windStrength.GetFloat();
            }
            set
            {
                this.windStrength.Set(value);
            }
        }
        /// <summary>
        /// Time
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
        /// Toggle UV coordinates by primitive ID
        /// </summary>
        protected uint UVToggleByPID
        {
            get
            {
                return this.uvToggleByPID.GetUInt();
            }
            set
            {
                this.uvToggleByPID.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return this.textures.GetResource();
            }
            set
            {
                if (this.currentTextures != value)
                {
                    this.textures.SetResource(value);

                    this.currentTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Random texture
        /// </summary>
        protected EngineShaderResourceView TextureRandom
        {
            get
            {
                return this.textureRandom.GetResource();
            }
            set
            {
                if (this.currentTextureRandom != value)
                {
                    this.textureRandom.SetResource(value);

                    this.currentTextureRandom = value;

                    Counters.TextureUpdates++;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectShadowBillboard(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ShadowMapBillboard = this.Effect.GetTechniqueByName("ShadowMapBillboard");

            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorld = this.Effect.GetVariableVector("gEyePositionWorld");
            this.startRadius = this.Effect.GetVariableScalar("gStartRadius");
            this.endRadius = this.Effect.GetVariableScalar("gEndRadius");
            this.textureCount = this.Effect.GetVariableScalar("gTextureCount");
            this.uvToggleByPID = this.Effect.GetVariableScalar("gUVToggleByPID");
            this.textures = this.Effect.GetVariableTexture("gTextureArray");

            this.windDirection = this.Effect.GetVariableVector("gWindDirection");
            this.windStrength = this.Effect.GetVariableScalar("gWindStrength");
            this.totalTime = this.Effect.GetVariableScalar("gTotalTime");
            this.textureRandom = this.Effect.GetVariableTexture("gTextureRandom");
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
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Billboard)
                {
                    switch (mode)
                    {
                        case DrawerModesEnum.ShadowMap:
                            return this.ShadowMapBillboard;
                        default:
                            throw new EngineException(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                    }
                }
                else
                {
                    throw new EngineException(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else
            {
                throw new EngineException(string.Format("Bad stage for effect: {0}", stage));
            }
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="windDirection">Wind direction</param>
        /// <param name="windStrength">Wind strength</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="randomTexture">Random texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            Vector3 windDirection,
            float windStrength,
            float totalTime,
            EngineShaderResourceView randomTexture)
        {
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.WindDirection = windDirection;
            this.WindStrength = windStrength;
            this.TotalTime = totalTime;
            this.TextureRandom = randomTexture;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="startRadius">Drawing start radius</param>
        /// <param name="endRadius">Drawing end radius</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="uvToggle">Toggle UV by primitive ID</param>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(
            float startRadius,
            float endRadius,
            uint textureCount,
            bool uvToggle,
            EngineShaderResourceView texture)
        {
            this.StartRadius = startRadius;
            this.EndRadius = endRadius;
            this.TextureCount = textureCount;
            this.UVToggleByPID = (uint)(uvToggle ? 1 : 0);
            this.Textures = texture;
        }
    }
}
