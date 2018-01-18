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
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// View * projection from light matrix for low definition shadows
        /// </summary>
        private EngineEffectVariableMatrix lightViewProjectionLD = null;
        /// <summary>
        /// View * projection from light matrix for high definition shadows
        /// </summary>
        private EngineEffectVariableMatrix lightViewProjectionHD = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
        /// <summary>
        /// Global ambient light effect variable;
        /// </summary>
        private EngineEffectVariableScalar globalAmbient;
        /// <summary>
        /// Directional light effect variable
        /// </summary>
        private EngineEffectVariable directionalLight = null;
        /// <summary>
        /// Point light effect variable
        /// </summary>
        private EngineEffectVariable pointLight = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EngineEffectVariable spotLight = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EngineEffectVariableScalar fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EngineEffectVariableScalar fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EngineEffectVariableVector fogColor = null;
        /// <summary>
        /// Shadow maps flag effect variable
        /// </summary>
        private EngineEffectVariableScalar shadowMaps = null;
        /// <summary>
        /// Color Map effect variable
        /// </summary>
        private EngineEffectVariableTexture tg1Map = null;
        /// <summary>
        /// Normal Map effect variable
        /// </summary>
        private EngineEffectVariableTexture tg2Map = null;
        /// <summary>
        /// Depth Map effect variable
        /// </summary>
        private EngineEffectVariableTexture tg3Map = null;
        /// <summary>
        /// Low definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapLD = null;
        /// <summary>
        /// High definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapHD = null;
        /// <summary>
        /// Cubic shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapCubic = null;
        /// <summary>
        /// Light Map effect variable
        /// </summary>
        private EngineEffectVariableTexture lightMap = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Materials palette
        /// </summary>
        private EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private EngineEffectVariableVector lod = null;

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
        /// Current low definition shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapLD = null;
        /// <summary>
        /// Current high definition shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapHD = null;
        /// <summary>
        /// Current cubic shadow map
        /// </summary>
        private EngineShaderResourceView currentShadowMapCubic = null;
        /// <summary>
        /// Current light map
        /// </summary>
        private EngineShaderResourceView currentLightMap = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private EngineShaderResourceView currentMaterialPalette = null;

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
        /// View * projection from light matrix for low definition shadows
        /// </summary>
        protected Matrix LightViewProjectionLD
        {
            get
            {
                return this.lightViewProjectionLD.GetMatrix();
            }
            set
            {
                this.lightViewProjectionLD.SetMatrix(value);
            }
        }
        /// <summary>
        /// View * projection from light matrix for high definition shadows
        /// </summary>
        protected Matrix LightViewProjectionHD
        {
            get
            {
                return this.lightViewProjectionHD.GetMatrix();
            }
            set
            {
                this.lightViewProjectionHD.SetMatrix(value);
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
        /// Global almbient light intensity
        /// </summary>
        protected float GlobalAmbient
        {
            get
            {
                return this.globalAmbient.GetFloat();
            }
            set
            {
                this.globalAmbient.Set(value);
            }
        }
        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight DirectionalLight
        {
            get
            {
                return this.directionalLight.GetValue<BufferDirectionalLight>();
            }
            set
            {
                this.directionalLight.SetValue(value);
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight PointLight
        {
            get
            {
                return this.pointLight.GetValue<BufferPointLight>();
            }
            set
            {
                this.pointLight.SetValue(value);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight SpotLight
        {
            get
            {
                return this.spotLight.GetValue<BufferSpotLight>();
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
        /// Shadow maps flag
        /// </summary>
        protected uint ShadowMaps
        {
            get
            {
                return this.shadowMaps.GetUInt();
            }
            set
            {
                this.shadowMaps.Set(value);
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
        /// Low definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapLD
        {
            get
            {
                return this.shadowMapLD.GetResource();
            }
            set
            {
                if (this.currentShadowMapLD != value)
                {
                    this.shadowMapLD.SetResource(value);

                    this.currentShadowMapLD = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High definition shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapHD
        {
            get
            {
                return this.shadowMapHD.GetResource();
            }
            set
            {
                if (this.currentShadowMapHD != value)
                {
                    this.shadowMapHD.SetResource(value);

                    this.currentShadowMapHD = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Cubic shadow map
        /// </summary>
        protected EngineShaderResourceView ShadowMapCubic
        {
            get
            {
                return this.shadowMapCubic.GetResource();
            }
            set
            {
                if (this.currentShadowMapCubic != value)
                {
                    this.shadowMapCubic.SetResource(value);

                    this.currentShadowMapCubic = value;

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
            this.lightViewProjectionLD = this.Effect.GetVariableMatrix("gLightViewProjectionLD");
            this.lightViewProjectionHD = this.Effect.GetVariableMatrix("gLightViewProjectionHD");
            this.eyePositionWorld = this.Effect.GetVariableVector("gEyePositionWorld");
            this.globalAmbient = this.Effect.GetVariableScalar("gGlobalAmbient");
            this.directionalLight = this.Effect.GetVariable("gDirLight");
            this.pointLight = this.Effect.GetVariable("gPointLight");
            this.spotLight = this.Effect.GetVariable("gSpotLight");
            this.fogStart = this.Effect.GetVariableScalar("gFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gFogRange");
            this.fogColor = this.Effect.GetVariableVector("gFogColor");
            this.shadowMaps = this.Effect.GetVariableScalar("gShadows");
            this.tg1Map = this.Effect.GetVariableTexture("gTG1Map");
            this.tg2Map = this.Effect.GetVariableTexture("gTG2Map");
            this.tg3Map = this.Effect.GetVariableTexture("gTG3Map");
            this.shadowMapLD = this.Effect.GetVariableTexture("gShadowMapLD");
            this.shadowMapHD = this.Effect.GetVariableTexture("gShadowMapHD");
            this.shadowMapCubic = this.Effect.GetVariableTexture("gShadowMapCubic");
            this.lightMap = this.Effect.GetVariableTexture("gLightMap");
            this.materialPaletteWidth = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPalette = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lod = this.Effect.GetVariableVector("gLOD");
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
        /// <param name="shadowMapLD">Low definition shadow map</param>
        /// <param name="shadowMapHD">High definition shadow map</param>
        public void UpdatePerLight(
            SceneLightDirectional light,
            ShadowMapFlags shadowMaps,
            IShadowMap shadowMapLD,
            IShadowMap shadowMapHD)
        {
            this.DirectionalLight = new BufferDirectionalLight(light);

            this.ShadowMaps = (uint)shadowMaps;
            if (shadowMapLD != null)
            {
                this.ShadowMapLD = shadowMapLD.Texture;
                this.LightViewProjectionLD = shadowMapLD.FromLightViewProjectionArray[0];
            }
            if (shadowMapHD != null)
            {
                this.ShadowMapHD = shadowMapHD.Texture;
                this.LightViewProjectionHD = shadowMapHD.FromLightViewProjectionArray[0];
            }
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapCubic">Cubic shadow map</param>
        public void UpdatePerLight(
            SceneLightPoint light,
            Matrix transform,
            Matrix viewProjection,
            ShadowMapFlags shadowMaps,
            IShadowMap shadowMapCubic)
        {
            this.PointLight = new BufferPointLight(light);
            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;
            this.ShadowMaps = (uint)shadowMaps;
            if (shadowMapCubic != null)
            {
                this.ShadowMapCubic = shadowMapCubic.Texture;
            }
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation and rotation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerLight(
            SceneLightSpot light,
            Matrix transform,
            Matrix viewProjection)
        {
            this.SpotLight = new BufferSpotLight(light);
            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;
        }
        /// <summary>
        /// Updates composer variables
        /// </summary>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="globalAmbient">Global ambient</param>
        /// <param name="fogStart">Fog start</param>
        /// <param name="fogRange">Fog range</param>
        /// <param name="fogColor">Fog color</param>
        /// <param name="depthMap">Depth map texture</param>
        /// <param name="lightMap">Light map</param>
        public void UpdateComposer(
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            float globalAmbient,
            float fogStart,
            float fogRange,
            Color4 fogColor,
            EngineShaderResourceView depthMap,
            EngineShaderResourceView lightMap)
        {
            this.WorldViewProjection = viewProjection;
            this.EyePositionWorld = eyePositionWorld;

            this.GlobalAmbient = globalAmbient;
            this.FogStart = fogStart;
            this.FogRange = fogRange;
            this.FogColor = fogColor;

            this.TG3Map = depthMap;
            this.LightMap = lightMap;
        }
    }
}
