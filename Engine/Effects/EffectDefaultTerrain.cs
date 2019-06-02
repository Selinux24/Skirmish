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
        /// World matrix effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldVar = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private readonly EngineEffectVariableMatrix worldViewProjectionVar = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar textureResolutionVar = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorDiffuseVar = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private readonly EngineEffectVariableScalar useColorSpecularVar = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private readonly EngineEffectVariableScalar materialIndexVar = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapLRVar = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture diffuseMapHRVar = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture normalMapVar = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture specularMapVar = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture colorTexturesVar = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private readonly EngineEffectVariableTexture alphaMapVar = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private readonly EngineEffectVariableVector parametersVar = null;
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
        /// Sampler for diffuse maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerDiffuseVar = null;
        /// <summary>
        /// Sampler for normal maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerNormalVar = null;
        /// <summary>
        /// Sampler for specular maps
        /// </summary>
        private readonly EngineEffectVariableSampler samplerSpecularVar = null;
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
        /// Texture resolution
        /// </summary>
        protected float TextureResolution
        {
            get
            {
                return this.textureResolutionVar.GetFloat();
            }
            set
            {
                this.textureResolutionVar.Set(value);
            }
        }
        /// <summary>
        /// Use diffuse map color
        /// </summary>
        protected bool UseColorDiffuse
        {
            get
            {
                return this.useColorDiffuseVar.GetBool();
            }
            set
            {
                this.useColorDiffuseVar.Set(value);
            }
        }
        /// <summary>
        /// Use specular map color
        /// </summary>
        protected bool UseColorSpecular
        {
            get
            {
                return this.useColorSpecularVar.GetBool();
            }
            set
            {
                this.useColorSpecularVar.Set(value);
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
        /// Low resolution textures
        /// </summary>
        protected EngineShaderResourceView DiffuseMapLR
        {
            get
            {
                return this.diffuseMapLRVar.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapLR != value)
                {
                    this.diffuseMapLRVar.SetResource(value);

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
                return this.diffuseMapHRVar.GetResource();
            }
            set
            {
                if (this.currentDiffuseMapHR != value)
                {
                    this.diffuseMapHRVar.SetResource(value);

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
                return this.normalMapVar.GetResource();
            }
            set
            {
                if (this.currentNormalMap != value)
                {
                    this.normalMapVar.SetResource(value);

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
                return this.specularMapVar.GetResource();
            }
            set
            {
                if (this.currentSpecularMap != value)
                {
                    this.specularMapVar.SetResource(value);

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
                return this.colorTexturesVar.GetResource();
            }
            set
            {
                if (this.currentColorTextures != value)
                {
                    this.colorTexturesVar.SetResource(value);

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
                return this.alphaMapVar.GetResource();
            }
            set
            {
                if (this.currentAlphaMap != value)
                {
                    this.alphaMapVar.SetResource(value);

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
                return this.parametersVar.GetVector<Vector4>();
            }
            set
            {
                this.parametersVar.Set(value);
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

                    this.samplerDiffuseVar.SetValue(0, sampler);
                    this.samplerNormalVar.SetValue(0, sampler);
                    this.samplerSpecularVar.SetValue(0, sampler);
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
        public EffectDefaultTerrain(Graphics graphics, byte[] effect, bool compile)
            : base(graphics, effect, compile)
        {
            this.TerrainAlphaMapForward = this.Effect.GetTechniqueByName("TerrainAlphaMapForward");
            this.TerrainSlopesForward = this.Effect.GetTechniqueByName("TerrainSlopesForward");
            this.TerrainFullForward = this.Effect.GetTechniqueByName("TerrainFullForward");

            //Globals
            this.materialPaletteWidthVar = this.Effect.GetVariableScalar("gMaterialPaletteWidth");
            this.materialPaletteVar = this.Effect.GetVariableTexture("gMaterialPalette");
            this.lodVar = this.Effect.GetVariableVector("gLOD");

            //Per frame
            this.worldVar = this.Effect.GetVariableMatrix("gVSWorld");
            this.worldViewProjectionVar = this.Effect.GetVariableMatrix("gVSWorldViewProjection");
            this.textureResolutionVar = this.Effect.GetVariableScalar("gVSTextureResolution");

            this.eyePositionWorldVar = this.Effect.GetVariableVector("gPSEyePositionWorld");
            this.lightCountVar = this.Effect.GetVariableVector("gPSLightCount");
            this.fogColorVar = this.Effect.GetVariableVector("gPSFogColor");
            this.fogStartVar = this.Effect.GetVariableScalar("gPSFogStart");
            this.fogRangeVar = this.Effect.GetVariableScalar("gPSFogRange");
            this.hemiLightVar = this.Effect.GetVariable("gPSHemiLight");
            this.dirLightsVar = this.Effect.GetVariable("gPSDirLights");
            this.pointLightsVar = this.Effect.GetVariable("gPSPointLights");
            this.spotLightsVar = this.Effect.GetVariable("gPSSpotLights");
            this.shadowMapDirectionalVar = this.Effect.GetVariableTexture("gPSShadowMapDir");
            this.shadowMapPointVar = this.Effect.GetVariableTexture("gPSShadowMapPoint");
            this.shadowMapSpotVar = this.Effect.GetVariableTexture("gPSShadowMapSpot");

            //Per object
            this.parametersVar = this.Effect.GetVariableVector("gPSParams");
            this.useColorDiffuseVar = this.Effect.GetVariableScalar("gPSUseColorDiffuse");
            this.useColorSpecularVar = this.Effect.GetVariableScalar("gPSUseColorSpecular");
            this.materialIndexVar = this.Effect.GetVariableScalar("gPSMaterialIndex");
            this.normalMapVar = this.Effect.GetVariableTexture("gPSNormalMapArray");
            this.specularMapVar = this.Effect.GetVariableTexture("gPSSpecularMapArray");
            this.colorTexturesVar = this.Effect.GetVariableTexture("gPSColorTextureArray");
            this.alphaMapVar = this.Effect.GetVariableTexture("gPSAlphaTexture");
            this.diffuseMapLRVar = this.Effect.GetVariableTexture("gPSDiffuseMapLRArray");
            this.diffuseMapHRVar = this.Effect.GetVariableTexture("gPSDiffuseMapHRArray");

            //Samplers
            this.samplerDiffuseVar = this.Effect.GetVariableSampler("SamplerDiffuse");
            this.samplerSpecularVar = this.Effect.GetVariableSampler("SamplerSpecular");
            this.samplerNormalVar = this.Effect.GetVariableSampler("SamplerNormal");

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

            var bHemiLight = BufferLightHemispheric.Default;
            var bDirLights = new BufferLightDirectional[BufferLightDirectional.MAX];
            var bPointLights = new BufferLightPoint[BufferLightPoint.MAX];
            var bSpotLights = new BufferLightSpot[BufferLightSpot.MAX];
            var lCount = new[] { 0, 0, 0 };

            var lights = context.Lights;

            if (lights != null)
            {
                this.EyePositionWorld = context.EyePosition;

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
            this.UseColorDiffuse = this.ColorTextures != null;

            this.DiffuseMapLR = state.DiffuseMapLR;
            this.DiffuseMapHR = state.DiffuseMapHR;

            this.Parameters = new Vector4(0, state.Proportion, state.SlopeRanges.X, state.SlopeRanges.Y);

            this.MaterialIndex = state.MaterialIndex;
        }
    }
}
