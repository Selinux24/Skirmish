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
        /// Hemispheric light effect variable
        /// </summary>
        private readonly EngineEffectVariable hemisphericLight = null;
        /// <summary>
        /// Directional light effect variable
        /// </summary>
        private readonly EngineEffectVariable directionalLight = null;
        /// <summary>
        /// Point light effect variable
        /// </summary>
        private readonly EngineEffectVariable pointLight = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private readonly EngineEffectVariable spotLight = null;
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
        /// Color Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture tg1Map = null;
        /// <summary>
        /// Normal Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture tg2Map = null;
        /// <summary>
        /// Depth Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture tg3Map = null;
        /// <summary>
        /// Light Map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture lightMap = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Materials palette
        /// </summary>
        private readonly EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lod = null;
        /// <summary>
        /// Directional shadow map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapDirectional = null;
        /// <summary>
        /// Point light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapPoint = null;
        /// <summary>
        /// Spot light shadows map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture shadowMapSpot = null;

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
        /// Hemispheric lights
        /// </summary>
        protected BufferLightHemispheric HemisphericLight
        {
            get
            {
                return this.hemisphericLight.GetValue<BufferLightHemispheric>();
            }
            set
            {
                this.hemisphericLight.SetValue(value);
            }
        }
        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferLightDirectional DirectionalLight
        {
            get
            {
                return this.directionalLight.GetValue<BufferLightDirectional>();
            }
            set
            {
                this.directionalLight.SetValue(value);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferLightPoint PointLight
        {
            get
            {
                return this.pointLight.GetValue<BufferLightPoint>();
            }
            set
            {
                this.pointLight.SetValue(value);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferLightSpot SpotLight
        {
            get
            {
                return this.spotLight.GetValue<BufferLightSpot>();
            }
            set
            {
                this.spotLight.SetValue(value);
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
        protected Color4 FogColor
        {
            get
            {
                return this.fogColor.GetVector<Color4>();
            }
            set
            {
                this.fogColor.Set(value);
            }
        }
        /// <summary>
        /// Color Map
        /// </summary>
        protected EngineShaderResourceView TG1Map
        {
            get
            {
                return this.tg1Map.GetResource();
            }
            set
            {
                if (this.currentTG1Map != value)
                {
                    this.tg1Map.SetResource(value);

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
                return this.tg2Map.GetResource();
            }
            set
            {
                if (this.currentTG2Map != value)
                {
                    this.tg2Map.SetResource(value);

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
                return this.tg3Map.GetResource();
            }
            set
            {
                if (this.currentTG3Map != value)
                {
                    this.tg3Map.SetResource(value);

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
                return this.lightMap.GetResource();
            }
            set
            {
                if (this.currentLightMap != value)
                {
                    this.lightMap.SetResource(value);

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
                return this.materialPaletteWidth.GetUInt();
            }
            set
            {
                this.materialPaletteWidth.Set(value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected EngineShaderResourceView MaterialPalette
        {
            get
            {
                return this.materialPalette.GetResource();
            }
            set
            {
                if (this.currentMaterialPalette != value)
                {
                    this.materialPalette.SetResource(value);

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
                return this.lod.GetVector<Vector3>();
            }
            set
            {
                this.lod.Set(value);
            }
        }
        /// <summary>
        /// Directional shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapDirectional
        {
            get
            {
                return this.shadowMapDirectional.GetResource();
            }
            set
            {
                if (this.currentShadowMapDirectional != value)
                {
                    this.shadowMapDirectional.SetResource(value);

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
                return this.shadowMapPoint.GetResource();
            }
            set
            {
                if (this.currentShadowMapPoint != value)
                {
                    this.shadowMapPoint.SetResource(value);

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
                return this.shadowMapSpot.GetResource();
            }
            set
            {
                if (this.currentShadowMapSpot != value)
                {
                    this.shadowMapSpot.SetResource(value);

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

            this.world = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.eyePositionWorld = this.Effect.GetVariableVector("gEyePositionWorld");
            this.directionalLight = this.Effect.GetVariable("gDirLight");
            this.pointLight = this.Effect.GetVariable("gPointLight");
            this.spotLight = this.Effect.GetVariable("gSpotLight");
            this.hemisphericLight = this.Effect.GetVariable("gHemiLight");
            this.fogStart = this.Effect.GetVariableScalar("gFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gFogRange");
            this.fogColor = this.Effect.GetVariableVector("gFogColor");
            this.tg1Map = this.Effect.GetVariableTexture("gTG1Map");
            this.tg2Map = this.Effect.GetVariableTexture("gTG2Map");
            this.tg3Map = this.Effect.GetVariableTexture("gTG3Map");
            this.lightMap = this.Effect.GetVariableTexture("gLightMap");
            this.materialPaletteWidth = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPalette = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lod = this.Effect.GetVariableVector("gLOD");
            this.shadowMapDirectional = this.Effect.GetVariableTexture("gShadowMapDir");
            this.shadowMapPoint = this.Effect.GetVariableTexture("gShadowMapPoint");
            this.shadowMapSpot = this.Effect.GetVariableTexture("gShadowMapSpot");
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
            SceneLightDirectional light,
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
            SceneLightPoint light,
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
            SceneLightSpot light,
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
