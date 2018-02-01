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
        private EngineEffectVariable hemiLight = null;
        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private EngineEffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private EngineEffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EngineEffectVariable spotLights = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private EngineEffectVariableVector lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EngineEffectVariableVector eyePositionWorld = null;
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
        /// Start radius
        /// </summary>
        private EngineEffectVariableScalar startRadius = null;
        /// <summary>
        /// End radius
        /// </summary>
        private EngineEffectVariableScalar endRadius = null;
        /// <summary>
        /// World effect variable
        /// </summary>
        private EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Low defintion map from light View * Projection transform
        /// </summary>
        private EngineEffectVariableMatrix fromLightViewProjectionLD = null;
        /// <summary>
        /// High definition map from light View * Projection transform
        /// </summary>
        private EngineEffectVariableMatrix fromLightViewProjectionHD = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Texture count variable
        /// </summary>
        private EngineEffectVariableScalar textureCount = null;
        /// <summary>
        /// Normal map count variable
        /// </summary>
        private EngineEffectVariableScalar normalMapCount = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EngineEffectVariableTexture textures = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EngineEffectVariableTexture normalMaps = null;
        /// <summary>
        /// Low definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapLD = null;
        /// <summary>
        /// High definition shadow map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapHD = null;
        /// <summary>
        /// Cubic shadows map effect variable
        /// </summary>
        private EngineEffectVariableTexture shadowMapCubic = null;
        /// <summary>
        /// Wind direction effect variable
        /// </summary>
        private EngineEffectVariableVector windDirection = null;
        /// <summary>
        /// Wind strength effect variable
        /// </summary>
        private EngineEffectVariableScalar windStrength = null;
        /// <summary>
        /// Time effect variable
        /// </summary>
        private EngineEffectVariableScalar totalTime = null;
        /// <summary>
        /// Position delta
        /// </summary>
        private EngineEffectVariableVector delta = null;
        /// <summary>
        /// Random texture effect variable
        /// </summary>
        private EngineEffectVariableTexture textureRandom = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private EngineEffectVariableVector lod = null;

        /// <summary>
        /// Current texture array
        /// </summary>
        private EngineShaderResourceView currentTextures = null;
        /// <summary>
        /// Current normal map array
        /// </summary>
        private EngineShaderResourceView currentNormalMaps = null;
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
        /// Current random texture
        /// </summary>
        private EngineShaderResourceView currentTextureRandom = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private EngineShaderResourceView currentMaterialPalette = null;

        /// <summary>
        /// Hemispheric lights
        /// </summary>
        protected BufferHemisphericLight HemiLight
        {
            get
            {
                return this.hemiLight.GetValue<BufferHemisphericLight>();
            }
            set
            {
                this.hemiLight.SetValue(value);
            }
        }
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
        /// Point light
        /// </summary>
        protected BufferPointLight[] PointLights
        {
            get
            {
                return this.pointLights.GetValue<BufferPointLight>(BufferPointLight.MAX);
            }
            set
            {
                this.pointLights.SetValue(value, BufferPointLight.MAX);
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight[] SpotLights
        {
            get
            {
                return this.spotLights.GetValue<BufferSpotLight>(BufferSpotLight.MAX);
            }
            set
            {
                this.spotLights.SetValue(value, BufferSpotLight.MAX);
            }
        }
        /// <summary>
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                var v = this.lightCount.GetVector<Int3>();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                var v = new Int3(value[0], value[1], value[2]);

                this.lightCount.Set(v);
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
        /// Start radius
        /// </summary>
        protected float StartRadius
        {
            get
            {
                return this.startRadius.GetFloat();
            }
            set
            {
                this.startRadius.Set(value);
            }
        }
        /// <summary>
        /// End radius
        /// </summary>
        protected float EndRadius
        {
            get
            {
                return this.endRadius.GetFloat();
            }
            set
            {
                this.endRadius.Set(value);
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
        /// Low definition map from light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjectionLD
        {
            get
            {
                return this.fromLightViewProjectionLD.GetMatrix();
            }
            set
            {
                this.fromLightViewProjectionLD.SetMatrix(value);
            }
        }
        /// <summary>
        /// High definition map from light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjectionHD
        {
            get
            {
                return this.fromLightViewProjectionHD.GetMatrix();
            }
            set
            {
                this.fromLightViewProjectionHD.SetMatrix(value);
            }
        }
        /// <summary>
        /// Material index
        /// </summary>
        protected uint MaterialIndex
        {
            get
            {
                return this.materialIndex.GetUInt();
            }
            set
            {
                this.materialIndex.Set(value);
            }
        }
        /// <summary>
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return this.textureCount.GetUInt();
            }
            set
            {
                this.textureCount.Set(value);
            }
        }
        /// <summary>
        /// Normal map count
        /// </summary>
        protected uint NormalMapCount
        {
            get
            {
                return this.normalMapCount.GetUInt();
            }
            set
            {
                this.normalMapCount.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected EngineShaderResourceView Textures
        {
            get
            {
                return this.textures.GetResource();
            }
            set
            {
                if (this.currentTextures != value)
                {
                    this.textures.SetResource(value);

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
                return this.normalMaps.GetResource();
            }
            set
            {
                if (this.currentNormalMaps != value)
                {
                    this.normalMaps.SetResource(value);

                    this.currentNormalMaps = value;

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
        /// Wind direction
        /// </summary>
        protected Vector3 WindDirection
        {
            get
            {
                return this.windDirection.GetVector<Vector3>();
            }
            set
            {
                this.windDirection.Set(value);
            }
        }
        /// <summary>
        /// Wind strength
        /// </summary>
        protected float WindStrength
        {
            get
            {
                return this.windStrength.GetFloat();
            }
            set
            {
                this.windStrength.Set(value);
            }
        }
        /// <summary>
        /// Time
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
        /// Position delta
        /// </summary>
        protected Vector3 Delta
        {
            get
            {
                return this.delta.GetVector<Vector3>();
            }
            set
            {
                this.delta.Set(value);
            }
        }
        /// <summary>
        /// Random texture
        /// </summary>
        protected EngineShaderResourceView TextureRandom
        {
            get
            {
                return this.textureRandom.GetResource();
            }
            set
            {
                if (this.currentTextureRandom != value)
                {
                    this.textureRandom.SetResource(value);

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
        public EffectDefaultFoliage(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.ForwardFoliage4 = this.Effect.GetTechniqueByName("ForwardFoliage4");
            this.ForwardFoliage8 = this.Effect.GetTechniqueByName("ForwardFoliage8");
            this.ForwardFoliage16 = this.Effect.GetTechniqueByName("ForwardFoliage16");

            this.world = this.Effect.GetVariableMatrix("gWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gWorldViewProjection");
            this.fromLightViewProjectionLD = this.Effect.GetVariableMatrix("gLightViewProjectionLD");
            this.fromLightViewProjectionHD = this.Effect.GetVariableMatrix("gLightViewProjectionHD");
            this.materialIndex = this.Effect.GetVariableScalar("gMaterialIndex");
            this.hemiLight = this.Effect.GetVariable("gPSHemiLight");
            this.dirLights = this.Effect.GetVariable("gDirLights");
            this.pointLights = this.Effect.GetVariable("gPointLights");
            this.spotLights = this.Effect.GetVariable("gSpotLights");
            this.lightCount = this.Effect.GetVariableVector("gLightCount");
            this.eyePositionWorld = this.Effect.GetVariableVector("gEyePositionWorld");
            this.fogStart = this.Effect.GetVariableScalar("gFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gFogRange");
            this.fogColor = this.Effect.GetVariableVector("gFogColor");
            this.shadowMaps = this.Effect.GetVariableScalar("gShadows");
            this.startRadius = this.Effect.GetVariableScalar("gStartRadius");
            this.endRadius = this.Effect.GetVariableScalar("gEndRadius");
            this.textureCount = this.Effect.GetVariableScalar("gTextureCount");
            this.normalMapCount = this.Effect.GetVariableScalar("gNormalMapCount");
            this.textures = this.Effect.GetVariableTexture("gTextureArray");
            this.normalMaps = this.Effect.GetVariableTexture("gNormalMapArray");
            this.shadowMapLD = this.Effect.GetVariableTexture("gShadowMapLD");
            this.shadowMapHD = this.Effect.GetVariableTexture("gShadowMapHD");
            this.shadowMapCubic = this.Effect.GetVariableTexture("gPSShadowMapCubic");
            this.windDirection = this.Effect.GetVariableVector("gWindDirection");
            this.windStrength = this.Effect.GetVariableScalar("gWindStrength");
            this.totalTime = this.Effect.GetVariableScalar("gTotalTime");
            this.delta = this.Effect.GetVariableVector("gDelta");
            this.textureRandom = this.Effect.GetVariableTexture("gTextureRandom");
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
        /// Update per frame data
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <param name="windDirection">Wind direction</param>
        /// <param name="windStrength">Wind strength</param>
        /// <param name="totalTime">Total time</param>
        /// <param name="delta">Delta</param>
        /// <param name="randomTexture">Random texture</param>
        /// <param name="startRadius">Drawing start radius</param>
        /// <param name="endRadius">Drawing end radius</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="normalMapCount">Normal map count</param>
        /// <param name="texture">Texture</param>
        /// <param name="normalMaps">Normal maps</param>
        /// <param name="materialIndex">Material index</param>
        public void UpdatePerFrame(
            DrawContext context,
            Vector3 windDirection,
            float windStrength,
            float totalTime,
            Vector3 delta,
            EngineShaderResourceView randomTexture,
            float startRadius,
            float endRadius,
            uint materialIndex,
            uint textureCount,
            uint normalMapCount,
            EngineShaderResourceView texture,
            EngineShaderResourceView normalMaps)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = context.ViewProjection;
            this.EyePositionWorld = context.EyePosition;

            this.StartRadius = startRadius;
            this.EndRadius = endRadius;
            this.TextureCount = textureCount;
            this.NormalMapCount = normalMapCount;
            this.Textures = texture;
            this.NormalMaps = normalMaps;

            this.MaterialIndex = materialIndex;

            var bHemiLight = BufferHemisphericLight.Default;
            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            var lights = context.Lights;
            if (lights != null)
            {
                var hemiLight = lights.GetVisibleHemisphericLight();
                if (hemiLight != null)
                {
                    bHemiLight = new BufferHemisphericLight(hemiLight);
                }

                var dirLights = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dirLights.Length, BufferDirectionalLight.MAX); i++)
                {
                    bDirLights[i] = new BufferDirectionalLight(dirLights[i]);
                }

                var pointLights = lights.GetVisiblePointLights();
                for (int i = 0; i < Math.Min(pointLights.Length, BufferPointLight.MAX); i++)
                {
                    bPointLights[i] = new BufferPointLight(pointLights[i]);
                }

                var spotLights = lights.GetVisibleSpotLights();
                for (int i = 0; i < Math.Min(spotLights.Length, BufferSpotLight.MAX); i++)
                {
                    bSpotLights[i] = new BufferSpotLight(spotLights[i]);
                }

                lCount[0] = Math.Min(dirLights.Length, BufferDirectionalLight.MAX);
                lCount[1] = Math.Min(pointLights.Length, BufferPointLight.MAX);
                lCount[2] = Math.Min(spotLights.Length, BufferSpotLight.MAX);

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.ShadowMaps = (uint)context.ShadowMaps;
                if (context.ShadowMapLow != null)
                {
                    this.FromLightViewProjectionLD = context.ShadowMapLow.FromLightViewProjectionArray[0];
                    this.ShadowMapLD = context.ShadowMapLow.Texture;
                }
                if (context.ShadowMapHigh != null)
                {
                    this.FromLightViewProjectionHD = context.ShadowMapHigh.FromLightViewProjectionArray[0];
                    this.ShadowMapHD = context.ShadowMapHigh.Texture;
                }
                if (context.ShadowMapCube != null)
                {
                    this.ShadowMapCubic = context.ShadowMapCube.Texture;
                }
            }
            else
            {
                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.ShadowMaps = 0;
                this.ShadowMapLD = null;
                this.FromLightViewProjectionLD = Matrix.Identity;
                this.ShadowMapHD = null;
                this.FromLightViewProjectionHD = Matrix.Identity;
                this.ShadowMapCubic = null;
            }

            this.HemiLight = bHemiLight;
            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;

            this.WindDirection = windDirection;
            this.WindStrength = windStrength;
            this.TotalTime = totalTime;
            this.Delta = delta;
            this.TextureRandom = randomTexture;
        }
    }
}
