using SharpDX;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Font effect
    /// </summary>
    public class EffectDeferredComposer : Drawer
    {
        /// <summary>
        /// Directional light technique
        /// </summary>
        public readonly EngineEffectTechnique DeferredDirectionalLight = null;
        /// <summary>
        /// Point stencil technique
        /// </summary>
        public readonly EngineEffectTechnique DeferredPointStencil = null;
        /// <summary>
        /// Point light technique
        /// </summary>
        public readonly EngineEffectTechnique DeferredPointLight = null;
        /// <summary>
        /// Spot stencil technique
        /// </summary>
        public readonly EngineEffectTechnique DeferredSpotStencil = null;
        /// <summary>
        /// Spot light technique
        /// </summary>
        public readonly EngineEffectTechnique DeferredSpotLight = null;
        /// <summary>
        /// Technique to combine all light sources
        /// </summary>
        public readonly EngineEffectTechnique DeferredCombineLights = null;

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
        /// Hemispheric light effect variable
        /// </summary>
        private readonly EngineEffectVariable hemisphericLightVar = null;
        /// <summary>
        /// Directional light effect variable
        /// </summary>
        private readonly EngineEffectVariable directionalLightVar = null;
        /// <summary>
        /// Point light effect variable
        /// </summary>
        private readonly EngineEffectVariable pointLightVar = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private readonly EngineEffectVariable spotLightVar = null;
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
        /// Color Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture tg1MapVar = null;
        /// <summary>
        /// Normal Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture tg2MapVar = null;
        /// <summary>
        /// Depth Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture tg3MapVar = null;
        /// <summary>
        /// Light Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture lightMapVar = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialPaletteWidthVar = null;
        /// <summary>
        /// Materials palette
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
        /// Albedo effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar albedoVar = null;

        /// <summary>
        /// Current target 1
        /// </summary>
        private EngineShaderResourceView currentTG1Map = null;
        /// <summary>
        /// Current target 2
        /// </summary>
        private EngineShaderResourceView currentTG2Map = null;
        /// <summary>
        /// Current target 3
        /// </summary>
        private EngineShaderResourceView currentTG3Map = null;
        /// <summary>
        /// Current light map
        /// </summary>
        private EngineShaderResourceView currentLightMap = null;
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
        /// Current spot shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapSpot = null;

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
        /// Hemispheric lights
        /// </summary>
        protected BufferLightHemispheric HemisphericLight
        {
            get
            {
                return hemisphericLightVar.GetValue<BufferLightHemispheric>();
            }
            set
            {
                hemisphericLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferLightDirectional DirectionalLight
        {
            get
            {
                return directionalLightVar.GetValue<BufferLightDirectional>();
            }
            set
            {
                directionalLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferLightPoint PointLight
        {
            get
            {
                return pointLightVar.GetValue<BufferLightPoint>();
            }
            set
            {
                pointLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot SpotLight
        {
            get
            {
                return spotLightVar.GetValue<BufferLightSpot>();
            }
            set
            {
                spotLightVar.SetValue(value);
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
        /// Color Map
        /// </summary>
        protected EngineShaderResourceView TG1Map
        {
            get
            {
                return tg1MapVar.GetResource();
            }
            set
            {
                if (currentTG1Map != value)
                {
                    tg1MapVar.SetResource(value);

                    currentTG1Map = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Normal Map
        /// </summary>
        protected EngineShaderResourceView TG2Map
        {
            get
            {
                return tg2MapVar.GetResource();
            }
            set
            {
                if (currentTG2Map != value)
                {
                    tg2MapVar.SetResource(value);

                    currentTG2Map = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Depth Map
        /// </summary>
        protected EngineShaderResourceView TG3Map
        {
            get
            {
                return tg3MapVar.GetResource();
            }
            set
            {
                if (currentTG3Map != value)
                {
                    tg3MapVar.SetResource(value);

                    currentTG3Map = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Light Map
        /// </summary>
        protected EngineShaderResourceView LightMap
        {
            get
            {
                return lightMapVar.GetResource();
            }
            set
            {
                if (currentLightMap != value)
                {
                    lightMapVar.SetResource(value);

                    currentLightMap = value;

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
        /// Albedo
        /// </summary>
        protected float Albedo
        {
            get
            {
                return albedoVar.GetFloat();
            }
            set
            {
                albedoVar.Set(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferredComposer(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            DeferredDirectionalLight = Effect.GetTechniqueByName("DeferredDirectionalLight");
            DeferredPointStencil = Effect.GetTechniqueByName("DeferredPointStencil");
            DeferredPointLight = Effect.GetTechniqueByName("DeferredPointLight");
            DeferredSpotStencil = Effect.GetTechniqueByName("DeferredSpotStencil");
            DeferredSpotLight = Effect.GetTechniqueByName("DeferredSpotLight");
            DeferredCombineLights = Effect.GetTechniqueByName("DeferredCombineLights");

            worldVar = Effect.GetVariableMatrix("gWorld");
            worldViewProjectionVar = Effect.GetVariableMatrix("gWorldViewProjection");
            eyePositionWorldVar = Effect.GetVariableVector("gEyePositionWorld");
            directionalLightVar = Effect.GetVariable("gDirLight");
            pointLightVar = Effect.GetVariable("gPointLight");
            spotLightVar = Effect.GetVariable("gSpotLight");
            hemisphericLightVar = Effect.GetVariable("gHemiLight");
            fogStartVar = Effect.GetVariableScalar("gFogStart");
            fogRangeVar = Effect.GetVariableScalar("gFogRange");
            fogColorVar = Effect.GetVariableVector("gFogColor");
            albedoVar = Effect.GetVariableScalar("gAlbedo");
            tg1MapVar = Effect.GetVariableTexture("gTG1Map");
            tg2MapVar = Effect.GetVariableTexture("gTG2Map");
            tg3MapVar = Effect.GetVariableTexture("gTG3Map");
            lightMapVar = Effect.GetVariableTexture("gLightMap");
            materialPaletteWidthVar = Effect.GetVariableScalar("gMaterialPaletteWidth");
            materialPaletteVar = Effect.GetVariableTexture("gMaterialPalette");
            lodVar = Effect.GetVariableVector("gLOD");
            shadowMapDirectionalVar = Effect.GetVariableTexture("gShadowMapDir");
            shadowMapPointVar = Effect.GetVariableTexture("gShadowMapPoint");
            shadowMapSpotVar = Effect.GetVariableTexture("gShadowMapSpot");
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
        /// Updates per frame variables
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="colorMap">Color map texture</param>
        /// <param name="normalMap">Normal map texture</param>
        /// <param name="depthMap">Depth map texture</param>
        public void UpdatePerFrame(
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            EngineShaderResourceView colorMap,
            EngineShaderResourceView normalMap,
            EngineShaderResourceView depthMap)
        {
            World = Matrix.Identity;
            WorldViewProjection = viewProjection;
            EyePositionWorld = eyePositionWorld;

            TG1Map = colorMap;
            TG2Map = normalMap;
            TG3Map = depthMap;
        }
        /// <summary>
        /// Updates per directional light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMap">Shadow map</param>
        public void UpdatePerLight(
            ISceneLightDirectional light,
            IShadowMap shadowMap)
        {
            DirectionalLight = new BufferLightDirectional(light);

            ShadowMapDirectional = shadowMap?.Texture;
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMap">Cubic shadow map</param>
        public void UpdatePerLight(
            ISceneLightPoint light,
            Matrix transform,
            Matrix viewProjection,
            IShadowMap shadowMap)
        {
            PointLight = new BufferLightPoint(light);

            World = transform;
            WorldViewProjection = transform * viewProjection;

            ShadowMapPoint = shadowMap?.Texture;
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation and rotation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="shadowMap">Shadow map</param>
        public void UpdatePerLight(
            ISceneLightSpot light,
            Matrix transform,
            Matrix viewProjection,
            IShadowMap shadowMap)
        {
            SpotLight = new BufferLightSpot(light);

            World = transform;
            WorldViewProjection = transform * viewProjection;

            ShadowMapSpot = shadowMap?.Texture;
        }
        /// <summary>
        /// Updates composer variables
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="depthMap">Depth map texture</param>
        /// <param name="lightMap">Light map</param>
        /// <param name="context">Drawing context</param>
        public void UpdateComposer(
            Matrix viewProjection,
            EngineShaderResourceView depthMap,
            EngineShaderResourceView lightMap,
            DrawContext context)
        {
            WorldViewProjection = viewProjection;
            EyePositionWorld = context.EyePosition;

            var lights = context.Lights;
            if (lights != null)
            {
                var ambientLight = lights.GetVisibleHemisphericLight();
                if (ambientLight != null)
                {
                    HemisphericLight = new BufferLightHemispheric(ambientLight);
                }
                else
                {
                    HemisphericLight = BufferLightHemispheric.Default;
                }

                FogStart = lights.FogStart;
                FogRange = lights.FogRange;
                FogColor = lights.FogColor;
                Albedo = lights.Albedo;
            }

            TG3Map = depthMap;
            LightMap = lightMap;
        }
    }
}
