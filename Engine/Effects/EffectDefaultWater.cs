using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Water effect
    /// </summary>
    public class EffectDefaultWater : Drawer
    {
        /// <summary>
        /// Position color drawing technique
        /// </summary>
        public readonly EngineEffectTechnique Water = null;

        /// <summary>
        /// World matrix effect variable
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
        /// Main light direction effect
        /// </summary>
        private EngineEffectVariableVector lightDirection = null;
        /// <summary>
        /// Base color effect variable
        /// </summary>
        private EngineEffectVariableVector baseColor = null;
        /// <summary>
        /// Water color effect variable
        /// </summary>
        private EngineEffectVariableVector waterColor = null;
        /// <summary>
        /// Wave parameters effect variable
        /// </summary>
        private EngineEffectVariableVector waveParams = null;
        /// <summary>
        /// Total time effect variable
        /// </summary>
        private EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Iteration parameters effect variable
        /// </summary>
        private EngineEffectVariableVector iterParams = null;

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
        /// Main light direction
        /// </summary>
        protected Vector3 LightDirection
        {
            get
            {
                return this.lightDirection.GetVector<Vector3>();
            }
            set
            {
                this.lightDirection.Set(value);
            }
        }
        /// <summary>
        /// Base color
        /// </summary>
        protected Color3 BaseColor
        {
            get
            {
                return this.baseColor.GetVector<Color3>();
            }
            set
            {
                this.baseColor.Set(value);
            }
        }
        /// <summary>
        /// Water color
        /// </summary>
        protected Color3 WaterColor
        {
            get
            {
                return this.waterColor.GetVector<Color3>();
            }
            set
            {
                this.waterColor.Set(value);
            }
        }
        /// <summary>
        /// Wave parameters
        /// </summary>
        protected Vector4 WaveParams
        {
            get
            {
                return this.waveParams.GetVector<Vector4>();
            }
            set
            {
                this.waveParams.Set(value);
            }
        }
        /// <summary>
        /// Total time
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
        /// Iterations parameters
        /// </summary>
        protected Int3 IterParams
        {
            get
            {
                return this.iterParams.GetVector<Int3>();
            }
            set
            {
                this.iterParams.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultWater(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.Water = this.Effect.GetTechniqueByName("Water");

            this.world = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gVSWorldViewProjection");

            this.eyePositionWorld = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.lightDirection = this.Effect.GetVariableVector("gPSLightDirection");
            this.baseColor = this.Effect.GetVariableVector("gPSBaseColor");
            this.waterColor = this.Effect.GetVariableVector("gPSWaterColor");
            this.waveParams = this.Effect.GetVariableVector("gPSWaveParams");
            this.totalTime = this.Effect.GetVariableScalar("gPSTotalTime");
            this.iterParams = this.Effect.GetVariableVector("gPSIters");
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
            throw new EngineException("Use techniques directly");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="lightDirection">Light direction</param>
        /// <param name="baseColor">Base color</param>
        /// <param name="waterColor">Water color</param>
        /// <param name="waveHeight">Wave heigth</param>
        /// <param name="waveChoppy">Wave choppy</param>
        /// <param name="waveSpeed">Wave speed</param>
        /// <param name="waveFrequency">Wave frequency</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="steps">Shader steps</param>
        /// <param name="geometryIterations">Geometry iterations</param>
        /// <param name="colorIterations">Color iterations</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePosition,
            Vector3 lightDirection,
            Color baseColor,
            Color waterColor,
            float waveHeight,
            float waveChoppy,
            float waveSpeed,
            float waveFrequency,
            float totalTime,
            int steps = 8,
            int geometryIterations = 3,
            int colorIterations = 5)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePosition;
            this.LightDirection = lightDirection;
            this.BaseColor = baseColor.RGB();
            this.WaterColor = waterColor.RGB();
            this.WaveParams = new Vector4(waveHeight, waveChoppy, waveSpeed, waveFrequency);
            this.TotalTime = totalTime;
            this.IterParams = new Int3(steps, geometryIterations, colorIterations);
        }
    }
}
