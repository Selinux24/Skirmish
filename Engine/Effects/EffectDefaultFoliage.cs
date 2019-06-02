using SharpDX;
using System;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Foliage effect
    /// </summary>
    public class EffectDefaultFoliage : Drawer
    {
        /// <summary>
        /// Foliage drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ForwardFoliage4 = null;
        /// <summary>
        /// Foliage drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ForwardFoliage8 = null;
        /// <summary>
        /// Foliage drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ForwardFoliage16 = null;

        /// <summary>
        /// Hemispheric light effect variable
        /// </summary>
        private readonly EngineEffectVariable hemiLightVar = null;
        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private readonly EngineEffectVariable dirLightsVar = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private readonly EngineEffectVariable pointLightsVar = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private readonly EngineEffectVariable spotLightsVar = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lightCountVar = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorldVar = null;
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
        /// Start radius
        /// </summary>
        private readonly EngineEffectVariableScalar startRadiusVar = null;
        /// <summary>
        /// End radius
        /// </summary>
        private readonly EngineEffectVariableScalar endRadiusVar = null;
        /// <summary>
        /// World effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndexVar = null;
        /// <summary>
        /// Texture count variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureCountVar = null;
        /// <summary>
        /// Normal map count variable
        /// </summary>
        private readonly EngineEffectVariableScalar normalMapCountVar = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture texturesVar = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMapsVar = null;
        /// <summary>
        /// Wind direction effect variable
        /// </summary>
        private readonly EngineEffectVariableVector windDirectionVar = null;
        /// <summary>
        /// Wind strength effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar windStrengthVar = null;
        /// <summary>
        /// Time effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar totalTimeVar = null;
        /// <summary>
        /// Position delta
        /// </summary>
        private readonly EngineEffectVariableVector deltaVar = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture textureRandomVar = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialPaletteWidthVar = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private readonly EngineEffectVariableTexture materialPaletteVar = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lodVar = null;
        /// <summary>
        /// Directional shadow map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapDirectionalVar = null;
        /// <summary>
        /// Point light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapPointVar = null;
        /// <summary>
        /// Spot light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapSpotVar = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextures = null;
        /// <summary>
        /// Current normal map array
        /// </summary>
        private EngineShaderResourceView currentNormalMaps = null;
        /// <summary>
        /// Current random texture
        /// </summary>
        private EngineShaderResourceView currentTextureRandom = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private EngineShaderResourceView currentMaterialPalette = null;
        /// <summary>
        /// Current directional shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapDirectional = null;
        /// <summary>
        /// Current point light shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapPoint = null;
        /// <summary>
        /// Current spot light shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapSpot = null;

        /// <summary>
        /// Hemispheric lights
        /// </summary>
        protected BufferLightHemispheric HemiLight
        {
            get
            {
                return this.hemiLightVar.GetValue<BufferLightHemispheric>();
            }
            set
            {
                this.hemiLightVar.SetValue(value);
            }
        }
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
        /// Point light
        /// </summary>
        protected BufferLightPoint[] PointLights
        {
            get
            {
                return this.pointLightsVar.GetValue<BufferLightPoint>(BufferLightPoint.MAX);
            }
            set
            {
                this.pointLightsVar.SetValue(value, BufferLightPoint.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot[] SpotLights
        {
            get
            {
                return this.spotLightsVar.GetValue<BufferLightSpot>(BufferLightSpot.MAX);
            }
            set
            {
                this.spotLightsVar.SetValue(value, BufferLightSpot.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                var v = this.lightCountVar.GetVector<Int3>();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                var v = new Int3(value[0], value[1], value[2]);

                this.lightCountVar.Set(v);
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
        protected Color4 FogColor
        {
            get
            {
                return this.fogColorVar.GetVector<Color4>();
            }
            set
            {
                this.fogColorVar.Set(value);
            }
        }
        /// <summary>
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return this.startRadiusVar.GetFloat();
            }
            set
            {
                this.startRadiusVar.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return this.endRadiusVar.GetFloat();
            }
            set
            {
                this.endRadiusVar.Set(value);
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
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return this.materialIndexVar.GetUInt();
            }
            set
            {
                this.materialIndexVar.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return this.textureCountVar.GetUInt();
            }
            set
            {
                this.textureCountVar.Set(value);
            }
        }
        /// <summary>
        /// Normal map count
        /// </summary>
        protected uint NormalMapCount
        {
            get
            {
                return this.normalMapCountVar.GetUInt();
            }
            set
            {
                this.normalMapCountVar.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return this.texturesVar.GetResource();
            }
            set
            {
                if (this.currentTextures != value)
                {
                    this.texturesVar.SetResource(value);

                    this.currentTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Normal maps
        /// </summary>
        protected EngineShaderResourceView NormalMaps
        {
            get
            {
                return this.normalMapsVar.GetResource();
            }
            set
            {
                if (this.currentNormalMaps != value)
                {
                    this.normalMapsVar.SetResource(value);

                    this.currentNormalMaps = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Wind direction
        /// </summary>
        protected Vector3 WindDirection
        {
            get
            {
                return this.windDirectionVar.GetVector<Vector3>();
            }
            set
            {
                this.windDirectionVar.Set(value);
            }
        }
        /// <summary>
        /// Wind strength
        /// </summary>
        protected float WindStrength
        {
            get
            {
                return this.windStrengthVar.GetFloat();
            }
            set
            {
                this.windStrengthVar.Set(value);
            }
        }
        /// <summary>
        /// Time
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
        /// Position delta
        /// </summary>
        protected Vector3 Delta
        {
            get
            {
                return this.deltaVar.GetVector<Vector3>();
            }
            set
            {
                this.deltaVar.Set(value);
            }
        }
        /// <summary>
        /// Random texture
        /// </summary>
        protected EngineShaderResourceView TextureRandom
        {
            get
            {
                return this.textureRandomVar.GetResource();
            }
            set
            {
                if (this.currentTextureRandom != value)
                {
                    this.textureRandomVar.SetResource(value);

                    this.currentTextureRandom = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Material palette width
        /// </summary>
        protected uint MaterialPaletteWidth
        {
            get
            {
                return this.materialPaletteWidthVar.GetUInt();
            }
            set
            {
                this.materialPaletteWidthVar.Set(value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected EngineShaderResourceView MaterialPalette
        {
            get
            {
                return this.materialPaletteVar.GetResource();
            }
            set
            {
                if (this.currentMaterialPalette != value)
                {
                    this.materialPaletteVar.SetResource(value);

                    this.currentMaterialPalette = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Level of detail ranges
        /// </summary>
        protected Vector3 LOD
        {
            get
            {
                return this.lodVar.GetVector<Vector3>();
            }
            set
            {
                this.lodVar.Set(value);
            }
        }
        /// <summary>
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return this.shadowMapDirectionalVar.GetResource();
            }
            set
            {
                if (this.currentShadowMapDirectional != value)
                {
                    this.shadowMapDirectionalVar.SetResource(value);

                    this.currentShadowMapDirectional = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Point light shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapPoint
        {
            get
            {
                return this.shadowMapPointVar.GetResource();
            }
            set
            {
                if (this.currentShadowMapPoint != value)
                {
                    this.shadowMapPointVar.SetResource(value);

                    this.currentShadowMapPoint = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Spot light shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapSpot
        {
            get
            {
                return this.shadowMapSpotVar.GetResource();
            }
            set
            {
                if (this.currentShadowMapSpot != value)
                {
                    this.shadowMapSpotVar.SetResource(value);

                    this.currentShadowMapSpot = value;

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
        public EffectDefaultFoliage(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ForwardFoliage4 = this.Effect.GetTechniqueByName("ForwardFoliage4");
            this.ForwardFoliage8 = this.Effect.GetTechniqueByName("ForwardFoliage8");
            this.ForwardFoliage16 = this.Effect.GetTechniqueByName("ForwardFoliage16");

            this.worldVar = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.materialIndexVar = this.Effect.GetVariableScalar("gMaterialIndex");
            this.hemiLightVar = this.Effect.GetVariable("gPSHemiLight");
            this.dirLightsVar = this.Effect.GetVariable("gDirLights");
            this.pointLightsVar = this.Effect.GetVariable("gPointLights");
            this.spotLightsVar = this.Effect.GetVariable("gSpotLights");
            this.lightCountVar = this.Effect.GetVariableVector("gLightCount");
            this.eyePositionWorldVar = this.Effect.GetVariableVector("gEyePositionWorld");
            this.fogStartVar = this.Effect.GetVariableScalar("gFogStart");
            this.fogRangeVar = this.Effect.GetVariableScalar("gFogRange");
            this.fogColorVar = this.Effect.GetVariableVector("gFogColor");
            this.startRadiusVar = this.Effect.GetVariableScalar("gStartRadius");
            this.endRadiusVar = this.Effect.GetVariableScalar("gEndRadius");
            this.textureCountVar = this.Effect.GetVariableScalar("gTextureCount");
            this.normalMapCountVar = this.Effect.GetVariableScalar("gNormalMapCount");
            this.texturesVar = this.Effect.GetVariableTexture("gTextureArray");
            this.normalMapsVar = this.Effect.GetVariableTexture("gNormalMapArray");
            this.windDirectionVar = this.Effect.GetVariableVector("gWindDirection");
            this.windStrengthVar = this.Effect.GetVariableScalar("gWindStrength");
            this.totalTimeVar = this.Effect.GetVariableScalar("gTotalTime");
            this.deltaVar = this.Effect.GetVariableVector("gDelta");
            this.textureRandomVar = this.Effect.GetVariableTexture("gTextureRandom");
            this.materialPaletteWidthVar = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPaletteVar = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lodVar = this.Effect.GetVariableVector("gLOD");
            this.shadowMapDirectionalVar = this.Effect.GetVariableTexture("gShadowMapDir");
            this.shadowMapPointVar = this.Effect.GetVariableTexture("gShadowMapPoint");
            this.shadowMapSpotVar = this.Effect.GetVariableTexture("gShadowMapSpot");
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        /// <param name="lod1">High level of detail maximum distance</param>
        /// <param name="lod2">Medium level of detail maximum distance</param>
        /// <param name="lod3">Low level of detail maximum distance</param>
        public void UpdateGlobals(
            EngineShaderResourceView materialPalette,
            uint materialPaletteWidth,
            float lod1,
            float lod2,
            float lod3)
        {
            this.MaterialPalette = materialPalette;
            this.MaterialPaletteWidth = materialPaletteWidth;

            this.LOD = new Vector3(lod1, lod2, lod3);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="state">State</param>
        public void UpdatePerFrame(
            DrawContext context,
            EffectFoliageState state)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = context.ViewProjection;
            this.EyePositionWorld = context.EyePosition;

            this.StartRadius = state.StartRadius;
            this.EndRadius = state.EndRadius;
            this.TextureCount = state.TextureCount;
            this.NormalMapCount = state.NormalMapCount;
            this.Textures = state.Texture;
            this.NormalMaps = state.NormalMaps;

            this.MaterialIndex = state.MaterialIndex;

            var bHemiLight = BufferLightHemispheric.Default;
            var bDirLights = new BufferLightDirectional[BufferLightDirectional.MAX];
            var bPointLights = new BufferLightPoint[BufferLightPoint.MAX];
            var bSpotLights = new BufferLightSpot[BufferLightSpot.MAX];
            var lCount = new[] { 0, 0, 0 };

            var lights = context.Lights;
            if (lights != null)
            {
                var hemi = lights.GetVisibleHemisphericLight();
                if (hemi != null)
                {
                    bHemiLight = new BufferLightHemispheric(hemi);
                }

                var dir = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dir.Length, BufferLightDirectional.MAX); i++)
                {
                    bDirLights[i] = new BufferLightDirectional(dir[i]);
                }

                var point = lights.GetVisiblePointLights();
                for (int i = 0; i < Math.Min(point.Length, BufferLightPoint.MAX); i++)
                {
                    bPointLights[i] = new BufferLightPoint(point[i]);
                }

                var spot = lights.GetVisibleSpotLights();
                for (int i = 0; i < Math.Min(spot.Length, BufferLightSpot.MAX); i++)
                {
                    bSpotLights[i] = new BufferLightSpot(spot[i]);
                }

                lCount[0] = Math.Min(dir.Length, BufferLightDirectional.MAX);
                lCount[1] = Math.Min(point.Length, BufferLightPoint.MAX);
                lCount[2] = Math.Min(spot.Length, BufferLightSpot.MAX);

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.ShadowMapDirectional = context.ShadowMapDirectional?.Texture;
                this.ShadowMapPoint = context.ShadowMapPoint?.Texture;
                this.ShadowMapSpot = context.ShadowMapSpot?.Texture;
            }
            else
            {
                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.ShadowMapDirectional = null;
                this.ShadowMapPoint = null;
                this.ShadowMapSpot = null;
            }

            this.HemiLight = bHemiLight;
            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;

            this.WindDirection = state.WindDirection;
            this.WindStrength = state.WindStrength;
            this.TotalTime = state.TotalTime;
            this.Delta = state.Delta;
            this.TextureRandom = state.RandomTexture;
        }
    }
}
