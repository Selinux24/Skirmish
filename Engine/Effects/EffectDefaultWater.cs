using SharpDX;
using System;

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
        /// Directional lights effect variable
        /// </summary>
        private readonly EngineEffectVariable dirLights = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar lightCount = null;
        /// <summary>
        /// Ambient effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar ambient = null;
        /// <summary>
        /// World matrix effect variable
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
        /// Base color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector baseColor = null;
        /// <summary>
        /// Water color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector waterColor = null;
        /// <summary>
        /// Wave parameters effect variable
        /// </summary>
        private readonly EngineEffectVariableVector waveParams = null;
        /// <summary>
        /// Total time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Iteration parameters effect variable
        /// </summary>
        private readonly EngineEffectVariableVector iterParams = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector fogColor = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                return this.dirLights.GetValue<BufferDirectionalLight>(BufferDirectionalLight.MAX);
            }
            set
            {
                this.dirLights.SetValue(value, BufferDirectionalLight.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int LightCount
        {
            get
            {
                return (int)this.lightCount.GetUInt();
            }
            set
            {
                this.lightCount.Set(value);
            }
        }
        /// <summary>
        /// Ambient
        /// </summary>
        protected float Ambient
        {
            get
            {
                return this.ambient.GetFloat();
            }
            set
            {
                this.ambient.Set(value);
            }
        }
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
        protected Color3 FogColor
        {
            get
            {
                return this.fogColor.GetVector<Color3>();
            }
            set
            {
                this.fogColor.Set(value);
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
            this.baseColor = this.Effect.GetVariableVector("gPSBaseColor");
            this.waterColor = this.Effect.GetVariableVector("gPSWaterColor");
            this.waveParams = this.Effect.GetVariableVector("gPSWaveParams");
            this.ambient = this.Effect.GetVariableScalar("gPSAmbient");
            this.fogRange = this.Effect.GetVariableScalar("gPSFogRange");
            this.fogStart = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogColor = this.Effect.GetVariableVector("gPSFogColor");
            this.totalTime = this.Effect.GetVariableScalar("gPSTotalTime");
            this.iterParams = this.Effect.GetVariableVector("gPSIters");
            this.lightCount = this.Effect.GetVariableScalar("gPSLightCount");
            this.dirLights = this.Effect.GetVariable("gPSDirLights");
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
            Matrix viewProjection,
            Vector3 eyePosition,
            SceneLights lights,
            Color baseColor,
            Color waterColor,
            float waveHeight,
            float waveChoppy,
            float waveSpeed,
            float waveFrequency,
            float totalTime,
            int steps = 8,
            int geometryIterations = 4,
            int colorIterations = 6)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePosition;
            this.BaseColor = baseColor.RGB();
            this.WaterColor = waterColor.RGB();
            this.WaveParams = new Vector4(waveHeight, waveChoppy, waveSpeed, waveFrequency);
            this.TotalTime = totalTime;
            this.IterParams = new Int3(steps, geometryIterations, colorIterations);

            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            int lCount = 0;

            if (lights != null)
            {
                var dir = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dir.Length, BufferDirectionalLight.MAX); i++)
                {
                    bDirLights[i] = new BufferDirectionalLight(dir[i]);
                }

                lCount = Math.Min(dir.Length, BufferDirectionalLight.MAX);

                this.Ambient = lights.Intensity;

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor.RGB();
            }
            else
            {
                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent.RGB();
            }

            this.DirLights = bDirLights;
            this.LightCount = lCount;
        }
    }
}
