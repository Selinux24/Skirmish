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
        /// Hemispheric lights
        /// </summary>
        protected BufferLightHemispheric HemisphericLight
        {
            get
            {
                return this.hemisphericLightVar.GetValue<BufferLightHemispheric>();
            }
            set
            {
                this.hemisphericLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferLightDirectional DirectionalLight
        {
            get
            {
                return this.directionalLightVar.GetValue<BufferLightDirectional>();
            }
            set
            {
                this.directionalLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferLightPoint PointLight
        {
            get
            {
                return this.pointLightVar.GetValue<BufferLightPoint>();
            }
            set
            {
                this.pointLightVar.SetValue(value);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot SpotLight
        {
            get
            {
                return this.spotLightVar.GetValue<BufferLightSpot>();
            }
            set
            {
                this.spotLightVar.SetValue(value);
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
        /// Color Map
        /// </summary>
        protected EngineShaderResourceView TG1Map
        {
            get
            {
                return this.tg1MapVar.GetResource();
            }
            set
            {
                if (this.currentTG1Map != value)
                {
                    this.tg1MapVar.SetResource(value);

                    this.currentTG1Map = value;

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
                return this.tg2MapVar.GetResource();
            }
            set
            {
                if (this.currentTG2Map != value)
                {
                    this.tg2MapVar.SetResource(value);

                    this.currentTG2Map = value;

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
                return this.tg3MapVar.GetResource();
            }
            set
            {
                if (this.currentTG3Map != value)
                {
                    this.tg3MapVar.SetResource(value);

                    this.currentTG3Map = value;

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
                return this.lightMapVar.GetResource();
            }
            set
            {
                if (this.currentLightMap != value)
                {
                    this.lightMapVar.SetResource(value);

                    this.currentLightMap = value;

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
        public EffectDeferredComposer(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.DeferredDirectionalLight = this.Effect.GetTechniqueByName("DeferredDirectionalLight");
            this.DeferredPointStencil = this.Effect.GetTechniqueByName("DeferredPointStencil");
            this.DeferredPointLight = this.Effect.GetTechniqueByName("DeferredPointLight");
            this.DeferredSpotStencil = this.Effect.GetTechniqueByName("DeferredSpotStencil");
            this.DeferredSpotLight = this.Effect.GetTechniqueByName("DeferredSpotLight");
            this.DeferredCombineLights = this.Effect.GetTechniqueByName("DeferredCombineLights");

            this.worldVar = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorldVar = this.Effect.GetVariableVector("gEyePositionWorld");
            this.directionalLightVar = this.Effect.GetVariable("gDirLight");
            this.pointLightVar = this.Effect.GetVariable("gPointLight");
            this.spotLightVar = this.Effect.GetVariable("gSpotLight");
            this.hemisphericLightVar = this.Effect.GetVariable("gHemiLight");
            this.fogStartVar = this.Effect.GetVariableScalar("gFogStart");
            this.fogRangeVar = this.Effect.GetVariableScalar("gFogRange");
            this.fogColorVar = this.Effect.GetVariableVector("gFogColor");
            this.tg1MapVar = this.Effect.GetVariableTexture("gTG1Map");
            this.tg2MapVar = this.Effect.GetVariableTexture("gTG2Map");
            this.tg3MapVar = this.Effect.GetVariableTexture("gTG3Map");
            this.lightMapVar = this.Effect.GetVariableTexture("gLightMap");
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
            this.World = Matrix.Identity;
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.TG1Map = colorMap;
            this.TG2Map = normalMap;
            this.TG3Map = depthMap;
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
            this.DirectionalLight = new BufferLightDirectional(light);

            this.ShadowMapDirectional = shadowMap?.Texture;
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
            this.PointLight = new BufferLightPoint(light);

            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;

            this.ShadowMapPoint = shadowMap?.Texture;
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
            this.SpotLight = new BufferLightSpot(light);

            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;

            this.ShadowMapSpot = shadowMap?.Texture;
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
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = context.EyePosition;

            var lights = context.Lights;
            if (lights != null)
            {
                var ambientLight = lights.GetVisibleHemisphericLight();
                if (ambientLight != null)
                {
                    this.HemisphericLight = new BufferLightHemispheric(ambientLight);
                }
                else
                {
                    this.HemisphericLight = BufferLightHemispheric.Default;
                }

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;
            }

            this.TG3Map = depthMap;
            this.LightMap = lightMap;
        }
    }
}
