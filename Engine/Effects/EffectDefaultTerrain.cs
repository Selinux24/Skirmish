using SharpDX;
using System;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Basic effect
    /// </summary>
    public class EffectDefaultTerrain : Drawer
    {
        /// <summary>
        /// Forward with alpha map drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainAlphaMapForward = null;
        /// <summary>
        /// Forward with slopes drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainSlopesForward = null;
        /// <summary>
        /// Forward full drawing technique
        /// </summary>
        public readonly EngineEffectTechnique TerrainFullForward = null;

        /// <summary>
        /// Hemispheric light effect variable
        /// </summary>
        private readonly EngineEffectVariable hemiLight = null;
        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private readonly EngineEffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private readonly EngineEffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private readonly EngineEffectVariable spotLights = null;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private readonly EngineEffectVariableVector eyePositionWorld = null;
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
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjection = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureResolution = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorSpecular = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndex = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMap = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture colorTextures = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture alphaMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector parameters = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private readonly EngineEffectVariableTexture materialPalette = null;
        /// <summary>
        /// Level of detail ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector lod = null;
        /// <summary>
        /// Sampler for diffuse maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerDiffuse = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerNormal = null;
        /// <summary>
        /// Sampler for specular maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerSpecular = null;
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
        /// Current diffuse map (Low resolution)
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapLR = null;
        /// <summary>
        /// Current normal map (High resolution)
        /// </summary>
        private EngineShaderResourceView currentDiffuseMapHR = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private EngineShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private EngineShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current color texure array
        /// </summary>
        private EngineShaderResourceView currentColorTextures = null;
        /// <summary>
        /// Current alpha map
        /// </summary>
        private EngineShaderResourceView currentAlphaMap = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private EngineShaderResourceView currentMaterialPalette = null;
        /// <summary>
        /// Use anisotropic sampling
        /// </summary>
        private bool? anisotropic = null;
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
        /// Sampler point
        /// </summary>
        private EngineSamplerState samplerPoint = null;
        /// <summary>
        /// Sampler linear
        /// </summary>
        private EngineSamplerState samplerLinear = null;
        /// <summary>
        /// Sampler anisotropic
        /// </summary>
        private EngineSamplerState samplerAnisotropic = null;

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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return this.textureResolution.GetFloat();
            }
            set
            {
                this.textureResolution.Set(value);
            }
        }
        /// <summary>
        /// Use diffuse map color
        /// </summary>
        protected bool UseColorDiffuse
        {
            get
            {
                return this.useColorDiffuse.GetBool();
            }
            set
            {
                this.useColorDiffuse.Set(value);
            }
        }
        /// <summary>
        /// Use specular map color
        /// </summary>
        protected bool UseColorSpecular
        {
            get
            {
                return this.useColorSpecular.GetBool();
            }
            set
            {
                this.useColorSpecular.Set(value);
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
        /// Low resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapLR
        {
            get
            {
                return this.diffuseMapLR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapLR != value)
                {
                    this.diffuseMapLR.SetResource(value);

                    this.currentDiffuseMapLR = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// High resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapHR
        {
            get
            {
                return this.diffuseMapHR.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapHR != value)
                {
                    this.diffuseMapHR.SetResource(value);

                    this.currentDiffuseMapHR = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Normal map
        /// </summary>
        protected EngineShaderResourceView NormalMap
        {
            get
            {
                return this.normalMap.GetResource();
            }
            set
            {
                if (this.currentNormalMap != value)
                {
                    this.normalMap.SetResource(value);

                    this.currentNormalMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Scpecular map
        /// </summary>
        protected EngineShaderResourceView SpecularMap
        {
            get
            {
                return this.specularMap.GetResource();
            }
            set
            {
                if (this.currentSpecularMap != value)
                {
                    this.specularMap.SetResource(value);

                    this.currentSpecularMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Color textures for alpha map
        /// </summary>
        protected EngineShaderResourceView ColorTextures
        {
            get
            {
                return this.colorTextures.GetResource();
            }
            set
            {
                if (this.currentColorTextures != value)
                {
                    this.colorTextures.SetResource(value);

                    this.currentColorTextures = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Alpha map
        /// </summary>
        protected EngineShaderResourceView AlphaMap
        {
            get
            {
                return this.alphaMap.GetResource();
            }
            set
            {
                if (this.currentAlphaMap != value)
                {
                    this.alphaMap.SetResource(value);

                    this.currentAlphaMap = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Slope ranges
        /// </summary>
        protected Vector4 Parameters
        {
            get
            {
                return this.parameters.GetVector<Vector4>();
            }
            set
            {
                this.parameters.Set(value);
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
        /// Gets or sets if the effect use anisotropic filtering
        /// </summary>
        public bool Anisotropic
        {
            get
            {
                return this.anisotropic == true;
            }
            set
            {
                if (this.anisotropic != value)
                {
                    this.anisotropic = value;

                    var sampler = this.anisotropic == true ?
                        this.samplerAnisotropic.GetSamplerState() :
                        this.samplerLinear.GetSamplerState();

                    this.samplerDiffuse.SetValue(0, sampler);
                    this.samplerNormal.SetValue(0, sampler);
                    this.samplerSpecular.SetValue(0, sampler);
                }
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
        public EffectDefaultTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.TerrainAlphaMapForward = this.Effect.GetTechniqueByName("TerrainAlphaMapForward");
            this.TerrainSlopesForward = this.Effect.GetTechniqueByName("TerrainSlopesForward");
            this.TerrainFullForward = this.Effect.GetTechniqueByName("TerrainFullForward");

            //Globals
            this.materialPaletteWidth = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPalette = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lod = this.Effect.GetVariableVector("gLOD");

            //Per frame
            this.world = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjection = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.textureResolution = this.Effect.GetVariableScalar("gVSTextureResolution");

            this.eyePositionWorld = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.lightCount = this.Effect.GetVariableVector("gPSLightCount");
            this.fogColor = this.Effect.GetVariableVector("gPSFogColor");
            this.fogStart = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogRange = this.Effect.GetVariableScalar("gPSFogRange");
            this.hemiLight = this.Effect.GetVariable("gPSHemiLight");
            this.dirLights = this.Effect.GetVariable("gPSDirLights");
            this.pointLights = this.Effect.GetVariable("gPSPointLights");
            this.spotLights = this.Effect.GetVariable("gPSSpotLights");
            this.shadowMapDirectional = this.Effect.GetVariableTexture("gPSShadowMapDir");
            this.shadowMapPoint = this.Effect.GetVariableTexture("gPSShadowMapPoint");
            this.shadowMapSpot = this.Effect.GetVariableTexture("gPSShadowMapSpot");

            //Per object
            this.parameters = this.Effect.GetVariableVector("gPSParams");
            this.useColorDiffuse = this.Effect.GetVariableScalar("gPSUseColorDiffuse");
            this.useColorSpecular = this.Effect.GetVariableScalar("gPSUseColorSpecular");
            this.materialIndex = this.Effect.GetVariableScalar("gPSMaterialIndex");
            this.normalMap = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMap = this.Effect.GetVariableTexture("gPSSpecularMapArray");
            this.colorTextures = this.Effect.GetVariableTexture("gPSColorTextureArray");
            this.alphaMap = this.Effect.GetVariableTexture("gPSAlphaTexture");
            this.diffuseMapLR = this.Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            this.diffuseMapHR = this.Effect.GetVariableTexture("gPSDiffuseMapHRArray");

            //Samplers
            this.samplerDiffuse = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerSpecular = this.Effect.GetVariableSampler("SamplerSpecular");
            this.samplerNormal = this.Effect.GetVariableSampler("SamplerNormal");

            //Initialize states
            this.samplerPoint = EngineSamplerState.Point(graphics);
            this.samplerLinear = EngineSamplerState.Linear(graphics);
            this.samplerAnisotropic = EngineSamplerState.Anisotropic(graphics, 8);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EffectDefaultTerrain()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.samplerPoint != null)
                {
                    this.samplerPoint.Dispose();
                    this.samplerPoint = null;
                }
                if (this.samplerLinear != null)
                {
                    this.samplerLinear.Dispose();
                    this.samplerLinear = null;
                }
                if (this.samplerAnisotropic != null)
                {
                    this.samplerAnisotropic.Dispose();
                    this.samplerAnisotropic = null;
                }
            }

            base.Dispose(disposing);
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
        /// <param name="textureResolution">Texture resolution</param>
        /// <param name="context">Drawing context</param>
        public void UpdatePerFrame(
            float textureResolution,
            DrawContext context)
        {
            this.World = Matrix.Identity;
            this.WorldViewProjection = context.ViewProjection;
            this.TextureResolution = textureResolution;

            var bHemiLight = BufferHemisphericLight.Default;
            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            var lights = context.Lights;

            if (lights != null)
            {
                this.EyePositionWorld = context.EyePosition;

                var hemi = lights.GetVisibleHemisphericLight();
                if (hemi != null)
                {
                    bHemiLight = new BufferHemisphericLight(hemi);
                }

                var dir = lights.GetVisibleDirectionalLights();
                for (int i = 0; i < Math.Min(dir.Length, BufferDirectionalLight.MAX); i++)
                {
                    bDirLights[i] = new BufferDirectionalLight(dir[i]);
                }

                var point = lights.GetVisiblePointLights();
                for (int i = 0; i < Math.Min(point.Length, BufferPointLight.MAX); i++)
                {
                    bPointLights[i] = new BufferPointLight(point[i]);
                }

                var spot = lights.GetVisibleSpotLights();
                for (int i = 0; i < Math.Min(spot.Length, BufferSpotLight.MAX); i++)
                {
                    bSpotLights[i] = new BufferSpotLight(spot[i]);
                }

                lCount[0] = Math.Min(dir.Length, BufferDirectionalLight.MAX);
                lCount[1] = Math.Min(point.Length, BufferPointLight.MAX);
                lCount[2] = Math.Min(spot.Length, BufferSpotLight.MAX);

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.ShadowMapDirectional = context.ShadowMapDirectional?.Texture;
                this.ShadowMapPoint = context.ShadowMapPoint?.Texture;
                this.ShadowMapSpot = context.ShadowMapSpot?.Texture;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

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
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="state">State</param>
        public void UpdatePerObject(
            EffectTerrainState state)
        {
            this.Anisotropic = state.UseAnisotropic;

            this.NormalMap = state.NormalMap;
            this.SpecularMap = state.SpecularMap;
            this.UseColorSpecular = state.SpecularMap != null;

            this.AlphaMap = state.AlphaMap;
            this.ColorTextures = state.ColorTextures;
            this.UseColorDiffuse = colorTextures != null;

            this.DiffuseMapLR = state.DiffuseMapLR;
            this.DiffuseMapHR = state.DiffuseMapHR;

            this.Parameters = new Vector4(0, state.Proportion, state.SlopeRanges.X, state.SlopeRanges.Y);

            this.MaterialIndex = state.MaterialIndex;
        }
    }
}
