using Engine.Shaders.Properties;
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
        /// Directional lights effect variable
        /// </summary>
        private readonly EngineEffectVariable dirLightsVar = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar lightCountVar = null;
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
        /// Water color alpha effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar waterAlphaVar = null;
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
                return dirLightsVar.GetValue<BufferLightDirectional>(BufferLightDirectional.MAX);
            }
            set
            {
                dirLightsVar.SetValue(value, BufferLightDirectional.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int LightCount
        {
            get
            {
                return (int)lightCountVar.GetUInt();
            }
            set
            {
                lightCountVar.Set(value);
            }
        }
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
        /// Base color
        /// </summary>
        protected Color3 BaseColor
        {
            get
            {
                return baseColorVar.GetVector<Color3>();
            }
            set
            {
                baseColorVar.Set(value);
            }
        }
        /// <summary>
        /// Water color
        /// </summary>
        protected Color3 WaterColor
        {
            get
            {
                return waterColorVar.GetVector<Color3>();
            }
            set
            {
                waterColorVar.Set(value);
            }
        }
        /// <summary>
        /// Water alpha color component
        /// </summary>
        protected float WaterAlpha
        {
            get
            {
                return waterAlphaVar.GetFloat();
            }
            set
            {
                waterAlphaVar.Set(value);
            }
        }
        /// <summary>
        /// Wave parameters
        /// </summary>
        protected Vector4 WaveParams
        {
            get
            {
                return waveParamsVar.GetVector<Vector4>();
            }
            set
            {
                waveParamsVar.Set(value);
            }
        }
        /// <summary>
        /// Total time
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
        /// Iterations parameters
        /// </summary>
        protected Int3 IterParams
        {
            get
            {
                return iterParamsVar.GetVector<Int3>();
            }
            set
            {
                iterParamsVar.Set(value);
            }
        }
        /// <summary>
        /// Fog start distance
        /// </summary>
        protected float FogStart
        {
            get
            {
                return fogStartVar.GetFloat();
            }
            set
            {
                fogStartVar.Set(value);
            }
        }
        /// <summary>
        /// Fog range distance
        /// </summary>
        protected float FogRange
        {
            get
            {
                return fogRangeVar.GetFloat();
            }
            set
            {
                fogRangeVar.Set(value);
            }
        }
        /// <summary>
        /// Fog color
        /// </summary>
        protected Color3 FogColor
        {
            get
            {
                return fogColorVar.GetVector<Color3>();
            }
            set
            {
                fogColorVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public EffectDefaultWater(Graphics graphics)
            : base(graphics, EffectsResources.ShaderDefaultWater, true)
        {
            Water = Effect.GetTechniqueByName("Water");

            worldVar = Effect.GetVariableMatrix("gVSWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gVSWorldViewProjection");

            eyePositionWorldVar = Effect.GetVariableVector("gPSEyePositionWorld");
            baseColorVar = Effect.GetVariableVector("gPSBaseColor");
            waterColorVar = Effect.GetVariableVector("gPSWaterColor");
            waterAlphaVar = Effect.GetVariableScalar("gPSWaterAlpha");
            waveParamsVar = Effect.GetVariableVector("gPSWaveParams");
            fogRangeVar = Effect.GetVariableScalar("gPSFogRange");
            fogStartVar = Effect.GetVariableScalar("gPSFogStart");
            fogColorVar = Effect.GetVariableVector("gPSFogColor");
            totalTimeVar = Effect.GetVariableScalar("gPSTotalTime");
            iterParamsVar = Effect.GetVariableVector("gPSIters");
            lightCountVar = Effect.GetVariableScalar("gPSLightCount");
            dirLightsVar = Effect.GetVariable("gPSDirLights");
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
            World = Matrix.Identity;
            WorldViewProjection = viewProjection;
            EyePositionWorld = eyePosition;
            BaseColor = state.BaseColor;
            WaterColor = state.WaterColor.RGB();
            WaterAlpha = state.WaterColor.Alpha;
            WaveParams = new Vector4(state.WaveHeight, state.WaveChoppy, state.WaveSpeed, state.WaveFrequency);
            TotalTime = state.TotalTime;
            IterParams = new Int3(state.Steps, state.GeometryIterations, state.ColorIterations);

            if (lights != null)
            {
                DirLights = BufferLightDirectional.Build(lights.GetVisibleDirectionalLights(), out int dirLength);
                LightCount = dirLength;

                FogStart = lights.FogStart;
                FogRange = lights.FogRange;
                FogColor = lights.FogColor.RGB();
            }
            else
            {
                DirLights = BufferLightDirectional.Default;
                LightCount = 0;

                FogStart = 0;
                FogRange = 0;
                FogColor = Color.Transparent.RGB();
            }
        }
    }
}
