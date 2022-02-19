using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Billboard effect
    /// </summary>
    public class EffectDefaultBillboard : Drawer
    {
        /// <summary>
        /// Billboard drawing technique
        /// </summary>
        public readonly EngineEffectTechnique ForwardBillboard = null;

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
        /// Point lights shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapPointVar = null;
        /// <summary>
        /// Spot lights shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapSpotVar = null;
        /// <summary>
        /// Shadow intensity
        /// </summary>
        private readonly EngineEffectVariableScalar shadowIntensityVar = null;

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
                return hemiLightVar.GetValue<BufferLightHemispheric>();
            }
            set
            {
                hemiLightVar.SetValue(value);
            }
        }
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
        /// Point light
        /// </summary>
        protected BufferLightPoint[] PointLights
        {
            get
            {
                return pointLightsVar.GetValue<BufferLightPoint>(BufferLightPoint.MAX);
            }
            set
            {
                pointLightsVar.SetValue(value, BufferLightPoint.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot[] SpotLights
        {
            get
            {
                return spotLightsVar.GetValue<BufferLightSpot>(BufferLightSpot.MAX);
            }
            set
            {
                spotLightsVar.SetValue(value, BufferLightSpot.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                var v = lightCountVar.GetVector<Int3>();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                var v = new Int3(value[0], value[1], value[2]);

                lightCountVar.Set(v);
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
        protected Color4 FogColor
        {
            get
            {
                return fogColorVar.GetVector<Color4>();
            }
            set
            {
                fogColorVar.Set(value);
            }
        }
        /// <summary>
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return startRadiusVar.GetFloat();
            }
            set
            {
                startRadiusVar.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return endRadiusVar.GetFloat();
            }
            set
            {
                endRadiusVar.Set(value);
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
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return materialIndexVar.GetUInt();
            }
            set
            {
                materialIndexVar.Set(value);
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
        /// Normal map count
        /// </summary>
        protected uint NormalMapCount
        {
            get
            {
                return normalMapCountVar.GetUInt();
            }
            set
            {
                normalMapCountVar.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return texturesVar.GetResource();
            }
            set
            {
                if (currentTextures != value)
                {
                    texturesVar.SetResource(value);

                    currentTextures = value;

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
                return normalMapsVar.GetResource();
            }
            set
            {
                if (currentNormalMaps != value)
                {
                    normalMapsVar.SetResource(value);

                    currentNormalMaps = value;

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
                return textureRandomVar.GetResource();
            }
            set
            {
                if (currentTextureRandom != value)
                {
                    textureRandomVar.SetResource(value);

                    currentTextureRandom = value;

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
                return materialPaletteWidthVar.GetUInt();
            }
            set
            {
                materialPaletteWidthVar.Set(value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected EngineShaderResourceView MaterialPalette
        {
            get
            {
                return materialPaletteVar.GetResource();
            }
            set
            {
                if (currentMaterialPalette != value)
                {
                    materialPaletteVar.SetResource(value);

                    currentMaterialPalette = value;

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
                return lodVar.GetVector<Vector3>();
            }
            set
            {
                lodVar.Set(value);
            }
        }
        /// <summary>
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return shadowMapDirectionalVar.GetResource();
            }
            set
            {
                if (currentShadowMapDirectional != value)
                {
                    shadowMapDirectionalVar.SetResource(value);

                    currentShadowMapDirectional = value;

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
                return shadowMapPointVar.GetResource();
            }
            set
            {
                if (currentShadowMapPoint != value)
                {
                    shadowMapPointVar.SetResource(value);

                    currentShadowMapPoint = value;

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
                return shadowMapSpotVar.GetResource();
            }
            set
            {
                if (currentShadowMapSpot != value)
                {
                    shadowMapSpotVar.SetResource(value);

                    currentShadowMapSpot = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Shadow intensity
        /// </summary>
        protected float ShadowIntensity
        {
            get
            {
                return shadowIntensityVar.GetFloat();
            }
            set
            {
                shadowIntensityVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultBillboard(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            ForwardBillboard = Effect.GetTechniqueByName("ForwardBillboard");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            materialIndexVar = Effect.GetVariableScalar("gMaterialIndex");
            hemiLightVar = Effect.GetVariable("gPSHemiLight");
            dirLightsVar = Effect.GetVariable("gDirLights");
            pointLightsVar = Effect.GetVariable("gPointLights");
            spotLightsVar = Effect.GetVariable("gSpotLights");
            lightCountVar = Effect.GetVariableVector("gLightCount");
            eyePositionWorldVar = Effect.GetVariableVector("gEyePositionWorld");
            fogStartVar = Effect.GetVariableScalar("gFogStart");
            fogRangeVar = Effect.GetVariableScalar("gFogRange");
            fogColorVar = Effect.GetVariableVector("gFogColor");
            startRadiusVar = Effect.GetVariableScalar("gStartRadius");
            endRadiusVar = Effect.GetVariableScalar("gEndRadius");
            textureCountVar = Effect.GetVariableScalar("gTextureCount");
            normalMapCountVar = Effect.GetVariableScalar("gNormalMapCount");
            texturesVar = Effect.GetVariableTexture("gTextureArray");
            normalMapsVar = Effect.GetVariableTexture("gNormalMapArray");
            textureRandomVar = Effect.GetVariableTexture("gTextureRandom");
            materialPaletteWidthVar = Effect.GetVariableScalar("gMaterialPaletteWidth");
            materialPaletteVar = Effect.GetVariableTexture("gMaterialPalette");
            lodVar = Effect.GetVariableVector("gLOD");
            shadowMapDirectionalVar = Effect.GetVariableTexture("gShadowMapDir");
            shadowMapPointVar = Effect.GetVariableTexture("gShadowMapPoint");
            shadowMapSpotVar = Effect.GetVariableTexture("gShadowMapSpot");
            shadowIntensityVar = Effect.GetVariableScalar("gShadowIntensity");
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
            MaterialPalette = materialPalette;
            MaterialPaletteWidth = materialPaletteWidth;

            LOD = new Vector3(lod1, lod2, lod3);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="state">State</param>
        public void UpdatePerFrame(
            DrawContext context,
            EffectBillboardState state)
        {
            World = Matrix.Identity;
            WorldViewProjection = context.ViewProjection;
            EyePositionWorld = context.EyePosition;

            StartRadius = state.StartRadius;
            EndRadius = state.EndRadius;
            TextureCount = state.TextureCount;
            NormalMapCount = state.NormalMapCount;
            Textures = state.Texture;
            NormalMaps = state.NormalMaps;

            MaterialIndex = state.MaterialIndex;

            var lights = context.Lights;
            if (lights != null)
            {
                HemiLight = BufferLightHemispheric.Build(lights.GetVisibleHemisphericLight());
                DirLights = BufferLightDirectional.Build(lights.GetVisibleDirectionalLights(), out int dirLength);
                PointLights = BufferLightPoint.Build(lights.GetVisiblePointLights(), out int pointLength);
                SpotLights = BufferLightSpot.Build(lights.GetVisibleSpotLights(), out int spotLength);
                LightCount = new[] { dirLength, pointLength, spotLength };

                FogStart = lights.FogStart;
                FogRange = lights.FogRange;
                FogColor = lights.FogColor;

                ShadowMapDirectional = context.ShadowMapDirectional?.Texture;
                ShadowMapPoint = context.ShadowMapPoint?.Texture;
                ShadowMapSpot = context.ShadowMapSpot?.Texture;
                ShadowIntensity = lights.ShadowIntensity;
            }
            else
            {
                HemiLight = BufferLightHemispheric.Default;
                DirLights = BufferLightDirectional.Default;
                PointLights = BufferLightPoint.Default;
                SpotLights = BufferLightSpot.Default;
                LightCount = new[] { 0, 0, 0 };

                FogStart = 0;
                FogRange = 0;
                FogColor = Color.Transparent;

                ShadowMapDirectional = null;
                ShadowMapPoint = null;
                ShadowMapSpot = null;
                ShadowIntensity = 1;
            }

            TextureRandom = state.RandomTexture;
        }
    }
}
