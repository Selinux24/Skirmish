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
        private readonly EngineEffectVariable dirLightsVar = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar lightCountVar = null;
        /// <summary>
        /// Ambient effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar ambientVar = null;
        /// <summary>
        /// World matrix effect variable
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
        /// Base color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector baseColorVar = null;
        /// <summary>
        /// Water color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector waterColorVar = null;
        /// <summary>
        /// Wave parameters effect variable
        /// </summary>
        private readonly EngineEffectVariableVector waveParamsVar = null;
        /// <summary>
        /// Total time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTimeVar = null;
        /// <summary>
        /// Iteration parameters effect variable
        /// </summary>
        private readonly EngineEffectVariableVector iterParamsVar = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogStartVar = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar fogRangeVar = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private readonly EngineEffectVariableVector fogColorVar = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferLightDirectional[] DirLights
        {
            get
            {
                return this.dirLightsVar.GetValue<BufferLightDirectional>(BufferLightDirectional.MAX);
            }
            set
            {
                this.dirLightsVar.SetValue(value, BufferLightDirectional.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int LightCount
        {
            get
            {
                return (int)this.lightCountVar.GetUInt();
            }
            set
            {
                this.lightCountVar.Set(value);
            }
        }
        /// <summary>
        /// Ambient
        /// </summary>
        protected float Ambient
        {
            get
            {
                return this.ambientVar.GetFloat();
            }
            set
            {
                this.ambientVar.Set(value);
            }
        }
        /// <summary>
        /// World matrix
        /// </summary>
        protected Matrix World
        {
            get
            {
                return this.worldVar.GetMatrix();
            }
            set
            {
                this.worldVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// World view projection matrix
        /// </summary>
        protected Matrix WorldViewProjection
        {
            get
            {
                return this.worldViewProjectionVar.GetMatrix();
            }
            set
            {
                this.worldViewProjectionVar.SetMatrix(value);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                return this.eyePositionWorldVar.GetVector<Vector3>();
            }
            set
            {
                this.eyePositionWorldVar.Set(value);
            }
        }
        /// <summary>
        /// Base color
        /// </summary>
        protected Color3 BaseColor
        {
            get
            {
                return this.baseColorVar.GetVector<Color3>();
            }
            set
            {
                this.baseColorVar.Set(value);
            }
        }
        /// <summary>
        /// Water color
        /// </summary>
        protected Color3 WaterColor
        {
            get
            {
                return this.waterColorVar.GetVector<Color3>();
            }
            set
            {
                this.waterColorVar.Set(value);
            }
        }
        /// <summary>
        /// Wave parameters
        /// </summary>
        protected Vector4 WaveParams
        {
            get
            {
                return this.waveParamsVar.GetVector<Vector4>();
            }
            set
            {
                this.waveParamsVar.Set(value);
            }
        }
        /// <summary>
        /// Total time
        /// </summary>
        protected float TotalTime
        {
            get
            {
                return this.totalTimeVar.GetFloat();
            }
            set
            {
                this.totalTimeVar.Set(value);
            }
        }
        /// <summary>
        /// Iterations parameters
        /// </summary>
        protected Int3 IterParams
        {
            get
            {
                return this.iterParamsVar.GetVector<Int3>();
            }
            set
            {
                this.iterParamsVar.Set(value);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return this.fogStartVar.GetFloat();
            }
            set
            {
                this.fogStartVar.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return this.fogRangeVar.GetFloat();
            }
            set
            {
                this.fogRangeVar.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color3 FogColor
        {
            get
            {
                return this.fogColorVar.GetVector<Color3>();
            }
            set
            {
                this.fogColorVar.Set(value);
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

            this.worldVar = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gVSWorldViewProjection");

            this.eyePositionWorldVar = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.baseColorVar = this.Effect.GetVariableVector("gPSBaseColor");
            this.waterColorVar = this.Effect.GetVariableVector("gPSWaterColor");
            this.waveParamsVar = this.Effect.GetVariableVector("gPSWaveParams");
            this.ambientVar = this.Effect.GetVariableScalar("gPSAmbient");
            this.fogRangeVar = this.Effect.GetVariableScalar("gPSFogRange");
            this.fogStartVar = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogColorVar = this.Effect.GetVariableVector("gPSFogColor");
            this.totalTimeVar = this.Effect.GetVariableScalar("gPSTotalTime");
            this.iterParamsVar = this.Effect.GetVariableVector("gPSIters");
            this.lightCountVar = this.Effect.GetVariableScalar("gPSLightCount");
            this.dirLightsVar = this.Effect.GetVariable("gPSDirLights");
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="lights">State</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector3 eyePosition,
            SceneLights lights,
            EffectWaterState state)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePosition;
            this.BaseColor = state.BaseColor.RGB();
            this.WaterColor = state.WaterColor.RGB();
            this.WaveParams = new Vector4(state.WaveHeight, state.WaveChoppy, state.WaveSpeed, state.WaveFrequency);
            this.TotalTime = state.TotalTime;
            this.IterParams = new Int3(state.Steps, state.GeometryIterations, state.ColorIterations);

            var bDirLights = new BufferLightDirectional[BufferLightDirectional.MAX];
            int lCount = 0;

            if (lights != null)
            {
                var dir = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dir.Length, BufferLightDirectional.MAX); i++)
                {
                    bDirLights[i] = new BufferLightDirectional(dir[i]);
                }

                lCount = Math.Min(dir.Length, BufferLightDirectional.MAX);

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
