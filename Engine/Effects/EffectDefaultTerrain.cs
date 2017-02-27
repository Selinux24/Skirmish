using SharpDX;
using System;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

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
        public readonly EffectTechnique TerrainAlphaMapForward = null;
        /// <summary>
        /// Forward with slopes drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainSlopesForward = null;
        /// <summary>
        /// Forward full drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainFullForward = null;

        /// <summary>
        /// Directional lights effect variable
        /// </summary>
        private EffectVariable dirLights = null;
        /// <summary>
        /// Point lights effect variable
        /// </summary>
        private EffectVariable pointLights = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EffectVariable spotLights = null;
        /// <summary>
        /// Global ambient light effect variable;
        /// </summary>
        private EffectScalarVariable globalAmbient;
        /// <summary>
        /// Light count effect variable
        /// </summary>
        private EffectVectorVariable lightCount = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Fog start effect variable
        /// </summary>
        private EffectScalarVariable fogStart = null;
        /// <summary>
        /// Fog range effect variable
        /// </summary>
        private EffectScalarVariable fogRange = null;
        /// <summary>
        /// Fog color effect variable
        /// </summary>
        private EffectVectorVariable fogColor = null;
        /// <summary>
        /// Shadow maps flag effect variable
        /// </summary>
        private EffectScalarVariable shadowMaps = null;
        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Texture resolution effect variable
        /// </summary>
        private EffectScalarVariable textureResolution = null;
        /// <summary>
        /// From light View * Projection transform
        /// </summary>
        private EffectMatrixVariable fromLightViewProjection = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private EffectScalarVariable useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private EffectScalarVariable useColorSpecular = null;
        /// <summary>
        /// Material index effect variable
        /// </summary>
        private EffectScalarVariable materialIndex = null;
        /// <summary>
        /// Low resolution textures effect variable
        /// </summary>
        private EffectShaderResourceVariable diffuseMapLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private EffectShaderResourceVariable diffuseMapHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EffectShaderResourceVariable normalMap = null;
        /// <summary>
        /// Specular map effect variable
        /// </summary>
        private EffectShaderResourceVariable specularMap = null;
        /// <summary>
        /// Static shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapStatic = null;
        /// <summary>
        /// Dynamic shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMapDynamic = null;
        /// <summary>
        /// Color texture array effect variable
        /// </summary>
        private EffectShaderResourceVariable colorTextures = null;
        /// <summary>
        /// Alpha map effect variable
        /// </summary>
        private EffectShaderResourceVariable alphaMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private EffectVectorVariable parameters = null;
        /// <summary>
        /// Material palette width effect variable
        /// </summary>
        private EffectScalarVariable materialPaletteWidth = null;
        /// <summary>
        /// Material palette
        /// </summary>
        private EffectShaderResourceVariable materialPalette = null;

        /// <summary>
        /// Current diffuse map (Low resolution)
        /// </summary>
        private ShaderResourceView currentDiffuseMapLR = null;
        /// <summary>
        /// Current normal map (High resolution)
        /// </summary>
        private ShaderResourceView currentDiffuseMapHR = null;
        /// <summary>
        /// Current normal map
        /// </summary>
        private ShaderResourceView currentNormalMap = null;
        /// <summary>
        /// Current specular map
        /// </summary>
        private ShaderResourceView currentSpecularMap = null;
        /// <summary>
        /// Current static shadow map
        /// </summary>
        private ShaderResourceView currentShadowMapStatic = null;
        /// <summary>
        /// Current dynamic shadow map
        /// </summary>
        private ShaderResourceView currentShadowMapDynamic = null;
        /// <summary>
        /// Current color texure array
        /// </summary>
        private ShaderResourceView currentColorTextures = null;
        /// <summary>
        /// Current alpha map
        /// </summary>
        private ShaderResourceView currentAlphaMap = null;
        /// <summary>
        /// Current material palette
        /// </summary>
        private ShaderResourceView currentMaterialPalette = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(default(BufferDirectionalLight).GetStride() * BufferDirectionalLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferDirectionalLight>(BufferDirectionalLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferDirectionalLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.dirLights.SetRawValue(ds, default(BufferDirectionalLight).GetStride() * BufferDirectionalLight.MAX);
                }
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight[] PointLights
        {
            get
            {
                using (DataStream ds = this.pointLights.GetRawValue(default(BufferPointLight).GetStride() * BufferPointLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferPointLight>(BufferPointLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferPointLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.pointLights.SetRawValue(ds, default(BufferPointLight).GetStride() * BufferPointLight.MAX);
                }
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight[] SpotLights
        {
            get
            {
                using (DataStream ds = this.spotLights.GetRawValue(default(BufferSpotLight).GetStride() * BufferSpotLight.MAX))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferSpotLight>(BufferSpotLight.MAX);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferSpotLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.spotLights.SetRawValue(ds, default(BufferSpotLight).GetStride() * BufferSpotLight.MAX);
                }
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
        /// Light count
        /// </summary>
        protected int[] LightCount
        {
            get
            {
                Int4 v = this.lightCount.GetIntVector();

                return new int[] { v.X, v.Y, v.Z };
            }
            set
            {
                Int4 v4 = new Int4(value[0], value[1], value[2], 0);

                this.lightCount.Set(v4);
            }
        }
        /// <summary>
        /// Camera eye position
        /// </summary>
        protected Vector3 EyePositionWorld
        {
            get
            {
                Vector4 v = this.eyePositionWorld.GetFloatVector();

                return new Vector3(v.X, v.Y, v.Z);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, value.Z, 1f);

                this.eyePositionWorld.Set(v4);
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
                return new Color4(this.fogColor.GetFloatVector());
            }
            set
            {
                this.fogColor.Set(value);
            }
        }
        /// <summary>
        /// Shadow maps flag
        /// </summary>
        protected int ShadowMaps
        {
            get
            {
                return this.shadowMaps.GetInt();
            }
            set
            {
                this.shadowMaps.Set(value);
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
        /// From light View * Projection transform
        /// </summary>
        protected Matrix FromLightViewProjection
        {
            get
            {
                return this.fromLightViewProjection.GetMatrix();
            }
            set
            {
                this.fromLightViewProjection.SetMatrix(value);
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
                return (uint)this.materialIndex.GetFloat();
            }
            set
            {
                this.materialIndex.Set((float)value);
            }
        }
        /// <summary>
        /// Low resolution textures
        /// </summary>
        protected ShaderResourceView DiffuseMapLR
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
        protected ShaderResourceView DiffuseMapHR
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
        protected ShaderResourceView NormalMap
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
        protected ShaderResourceView SpecularMap
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
        /// Static shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapStatic
        {
            get
            {
                return this.shadowMapStatic.GetResource();
            }
            set
            {
                if (this.currentShadowMapStatic != value)
                {
                    this.shadowMapStatic.SetResource(value);

                    this.currentShadowMapStatic = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Dynamic shadow map
        /// </summary>
        protected ShaderResourceView ShadowMapDynamic
        {
            get
            {
                return this.shadowMapDynamic.GetResource();
            }
            set
            {
                if (this.currentShadowMapDynamic != value)
                {
                    this.shadowMapDynamic.SetResource(value);

                    this.currentShadowMapDynamic = value;

                    Counters.TextureUpdates++;
                }
            }
        }
        /// <summary>
        /// Color textures for alpha map
        /// </summary>
        protected ShaderResourceView ColorTextures
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
        protected ShaderResourceView AlphaMap
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
                return this.parameters.GetFloatVector();
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
                return (uint)this.materialPaletteWidth.GetFloat();
            }
            set
            {
                this.materialPaletteWidth.Set((float)value);
            }
        }
        /// <summary>
        /// Material palette
        /// </summary>
        protected ShaderResourceView MaterialPalette
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
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDefaultTerrain(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.TerrainAlphaMapForward = this.Effect.GetTechniqueByName("TerrainAlphaMapForward");
            this.TerrainSlopesForward = this.Effect.GetTechniqueByName("TerrainSlopesForward");
            this.TerrainFullForward = this.Effect.GetTechniqueByName("TerrainFullForward");

            //Globals
            this.materialPaletteWidth = this.Effect.GetVariableByName("gMaterialPaletteWidth").AsScalar();
            this.materialPalette = this.Effect.GetVariableByName("gMaterialPalette").AsShaderResource();

            //Per frame
            this.world = this.Effect.GetVariableByName("gVSWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gVSWorldViewProjection").AsMatrix();
            this.textureResolution = this.Effect.GetVariableByName("gVSTextureResolution").AsScalar();

            this.fromLightViewProjection = this.Effect.GetVariableByName("gPSLightViewProjection").AsMatrix();
            this.eyePositionWorld = this.Effect.GetVariableByName("gPSEyePositionWorld").AsVector();
            this.globalAmbient = this.Effect.GetVariableByName("gPSGlobalAmbient").AsScalar();
            this.lightCount = this.Effect.GetVariableByName("gPSLightCount").AsVector();
            this.shadowMaps = this.Effect.GetVariableByName("gPSShadows").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gPSFogColor").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gPSFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gPSFogRange").AsScalar();
            this.dirLights = this.Effect.GetVariableByName("gPSDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPSPointLights");
            this.spotLights = this.Effect.GetVariableByName("gPSSpotLights");
            this.shadowMapStatic = this.Effect.GetVariableByName("gPSShadowMapStatic").AsShaderResource();
            this.shadowMapDynamic = this.Effect.GetVariableByName("gPSShadowMapDynamic").AsShaderResource();

            //Per object
            this.parameters = this.Effect.GetVariableByName("gPSParams").AsVector();
            this.useColorDiffuse = this.Effect.GetVariableByName("gPSUseColorDiffuse").AsScalar();
            this.useColorSpecular = this.Effect.GetVariableByName("gPSUseColorSpecular").AsScalar();
            this.materialIndex = this.Effect.GetVariableByName("gPSMaterialIndex").AsScalar();
            this.normalMap = this.Effect.GetVariableByName("gPSNormalMapArray").AsShaderResource();
            this.specularMap = this.Effect.GetVariableByName("gPSSpecularMapArray").AsShaderResource();
            this.colorTextures = this.Effect.GetVariableByName("gPSColorTextureArray").AsShaderResource();
            this.alphaMap = this.Effect.GetVariableByName("gPSAlphaTexture").AsShaderResource();
            this.diffuseMapLR = this.Effect.GetVariableByName("gPSDiffuseMapLRArray").AsShaderResource();
            this.diffuseMapHR = this.Effect.GetVariableByName("gPSDiffuseMapHRArray").AsShaderResource();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode)
        {
            EffectTechnique technique = null;

            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Terrain)
                {
                    if (mode == DrawerModesEnum.Forward) technique = this.TerrainFullForward;
                }
            }

            if (technique == null)
            {
                throw new Exception(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
            }

            return technique;
        }

        /// <summary>
        /// Update effect globals
        /// </summary>
        /// <param name="materialPalette">Material palette texture</param>
        /// <param name="materialPaletteWidth">Material palette texture width</param>
        public void UpdateGlobals(
            ShaderResourceView materialPalette,
            uint materialPaletteWidth)
        {
            this.MaterialPalette = materialPalette;
            this.MaterialPaletteWidth = materialPaletteWidth;
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="textureResolution">Texture resolution</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapStatic">Static shadow map texture</param>
        /// <param name="shadowMapDynamic">Dynamic shadow map texture</param>
        /// <param name="fromLightViewProjection">From light View * Projection transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            float textureResolution,
            Vector3 eyePositionWorld,
            SceneLights lights,
            int shadowMaps,
            ShaderResourceView shadowMapStatic,
            ShaderResourceView shadowMapDynamic,
            Matrix fromLightViewProjection)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.TextureResolution = textureResolution;

            var globalAmbient = 0f;
            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                globalAmbient = lights.GlobalAmbientLight;

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

                this.FromLightViewProjection = fromLightViewProjection;
                this.ShadowMapStatic = shadowMapStatic;
                this.ShadowMapDynamic = shadowMapDynamic;
                this.ShadowMaps = shadowMaps;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.FromLightViewProjection = Matrix.Identity;
                this.ShadowMapStatic = null;
                this.ShadowMapDynamic = null;
                this.ShadowMaps = 0;
            }

            this.GlobalAmbient = globalAmbient;
            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="normalMap">Normal map</param>
        /// <param name="specularMap">Specular map</param>
        /// <param name="useAlphaMap">Use alpha mapping</param>
        /// <param name="alphaMap">Alpha map</param>
        /// <param name="colorTextures">Color textures</param>
        /// <param name="useSlopes">Use slope texturing</param>
        /// <param name="diffuseMapLR">Low resolution textures</param>
        /// <param name="diffuseMapHR">High resolution textures</param>
        /// <param name="slopeRanges">Slope ranges</param>
        /// <param name="proportion">Lerping proportion</param>
        /// <param name="materialIndex">Marerial index</param>
        public void UpdatePerObject(
            ShaderResourceView normalMap,
            ShaderResourceView specularMap,
            bool useAlphaMap,
            ShaderResourceView alphaMap,
            ShaderResourceView colorTextures,
            bool useSlopes,
            Vector2 slopeRanges,
            ShaderResourceView diffuseMapLR,
            ShaderResourceView diffuseMapHR,
            float proportion,
            uint materialIndex)
        {
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.UseColorSpecular = specularMap != null;

            this.AlphaMap = alphaMap;
            this.ColorTextures = colorTextures;
            this.UseColorDiffuse = colorTextures != null;

            this.DiffuseMapLR = diffuseMapLR;
            this.DiffuseMapHR = diffuseMapHR;

            this.Parameters = new Vector4(0, proportion, slopeRanges.X, slopeRanges.Y);

            this.MaterialIndex = materialIndex;
        }
    }
}
