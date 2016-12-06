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
    public class EffectTerrain : Drawer
    {
        /// <summary>
        /// Forward drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainForward = null;
        /// <summary>
        /// Deferred drawing technique
        /// </summary>
        public readonly EffectTechnique TerrainDeferred = null;
        /// <summary>
        /// Shadow mapping technique
        /// </summary>
        public readonly EffectTechnique TerrainShadowMap = null;

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
        /// Inverse world matrix effect variable
        /// </summary>
        private EffectMatrixVariable worldInverse = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// From light View * Projection transform
        /// </summary>
        private EffectMatrixVariable fromLightViewProjection = null;
        /// <summary>
        /// Material effect variable
        /// </summary>
        private EffectVariable material = null;
        /// <summary>
        /// Use diffuse map color variable
        /// </summary>
        private EffectScalarVariable useColorDiffuse = null;
        /// <summary>
        /// Use specular map color variable
        /// </summary>
        private EffectScalarVariable useColorSpecular = null;
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
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(default(BufferDirectionalLight).Stride * BufferDirectionalLight.MAX))
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

                    this.dirLights.SetRawValue(ds, default(BufferDirectionalLight).Stride * BufferDirectionalLight.MAX);
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
                using (DataStream ds = this.pointLights.GetRawValue(default(BufferPointLight).Stride * BufferPointLight.MAX))
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

                    this.pointLights.SetRawValue(ds, default(BufferPointLight).Stride * BufferPointLight.MAX);
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
                using (DataStream ds = this.spotLights.GetRawValue(default(BufferSpotLight).Stride * BufferSpotLight.MAX))
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

                    this.spotLights.SetRawValue(ds, default(BufferSpotLight).Stride * BufferSpotLight.MAX);
                }
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
        /// Inverse world matrix
        /// </summary>
        protected Matrix WorldInverse
        {
            get
            {
                return this.worldInverse.GetMatrix();
            }
            set
            {
                this.worldInverse.SetMatrix(value);
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
        /// Material
        /// </summary>
        protected BufferMaterials Material
        {
            get
            {
                using (DataStream ds = this.material.GetRawValue(default(BufferMaterials).Stride))
                {
                    ds.Position = 0;

                    return ds.Read<BufferMaterials>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferMaterials>(new BufferMaterials[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.material.SetRawValue(ds, default(BufferMaterials).Stride);
                }
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
                this.diffuseMapLR.SetResource(value);
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
                this.diffuseMapHR.SetResource(value);
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
                this.normalMap.SetResource(value);
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
                this.specularMap.SetResource(value);
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
                this.shadowMapStatic.SetResource(value);
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
                this.shadowMapDynamic.SetResource(value);
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
                this.colorTextures.SetResource(value);
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
                this.alphaMap.SetResource(value);
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
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectTerrain(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.TerrainForward = this.Effect.GetTechniqueByName("TerrainForward");
            this.TerrainDeferred = this.Effect.GetTechniqueByName("TerrainDeferred");
            this.TerrainShadowMap = this.Effect.GetTechniqueByName("TerrainShadowMap");

            this.AddInputLayout(this.TerrainForward, VertexTerrain.GetInput());
            this.AddInputLayout(this.TerrainDeferred, VertexTerrain.GetInput());
            this.AddInputLayout(this.TerrainShadowMap, VertexTerrain.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.fromLightViewProjection = this.Effect.GetVariableByName("gLightViewProjection").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.useColorDiffuse = this.Effect.GetVariableByName("gUseColorDiffuse").AsScalar();
            this.useColorSpecular = this.Effect.GetVariableByName("gUseColorSpecular").AsScalar();
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPointLights");
            this.spotLights = this.Effect.GetVariableByName("gSpotLights");
            this.lightCount = this.Effect.GetVariableByName("gLightCount").AsVector();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.shadowMaps = this.Effect.GetVariableByName("gShadows").AsScalar();
            this.diffuseMapLR = this.Effect.GetVariableByName("gDiffuseMapLRArray").AsShaderResource();
            this.diffuseMapHR = this.Effect.GetVariableByName("gDiffuseMapHRArray").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gNormalMapArray").AsShaderResource();
            this.specularMap = this.Effect.GetVariableByName("gSpecularMapArray").AsShaderResource();
            this.shadowMapStatic = this.Effect.GetVariableByName("gShadowMapStatic").AsShaderResource();
            this.shadowMapDynamic = this.Effect.GetVariableByName("gShadowMapDynamic").AsShaderResource();
            this.colorTextures = this.Effect.GetVariableByName("gColorTextureArray").AsShaderResource();
            this.alphaMap = this.Effect.GetVariableByName("gAlphaTexture").AsShaderResource();
            this.parameters = this.Effect.GetVariableByName("gParams").AsVector();
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
                    if (mode == DrawerModesEnum.Forward) technique = this.TerrainForward;
                    if (mode == DrawerModesEnum.Deferred) technique = this.TerrainDeferred;
                    if (mode == DrawerModesEnum.ShadowMap) technique = this.TerrainShadowMap;
                }
            }

            if (technique == null)
            {
                throw new Exception(string.Format("Bad vertex type for effect, stage and mode: {0} - {1} - {2}", vertexType, stage, mode));
            }

            return technique;
        }

        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, null, 0, null, null, Matrix.Identity);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMaps">Shadow map flags</param>
        /// <param name="shadowMapStatic">Static shadow map texture</param>
        /// <param name="shadowMapDynamic">Dynamic shadow map texture</param>
        /// <param name="fromLightViewProjection">From light View * Projection transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            int shadowMaps,
            ShaderResourceView shadowMapStatic,
            ShaderResourceView shadowMapDynamic,
            Matrix fromLightViewProjection)
        {
            this.World = world;
            this.WorldInverse = Matrix.Invert(world);
            this.WorldViewProjection = world * viewProjection;

            var bDirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
            var bPointLights = new BufferPointLight[BufferPointLight.MAX];
            var bSpotLights = new BufferSpotLight[BufferSpotLight.MAX];
            var lCount = new[] { 0, 0, 0 };

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

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

                lCount[0] = dirLights.Length;
                lCount[1] = pointLights.Length;
                lCount[2] = spotLights.Length;

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

            this.DirLights = bDirLights;
            this.PointLights = bPointLights;
            this.SpotLights = bSpotLights;
            this.LightCount = lCount;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="material">Material</param>
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
        public void UpdatePerObject(
            Material material,
            ShaderResourceView normalMap,
            ShaderResourceView specularMap,
            bool useAlphaMap,
            ShaderResourceView alphaMap,
            ShaderResourceView colorTextures,
            bool useSlopes,
            Vector2 slopeRanges,
            ShaderResourceView diffuseMapLR,
            ShaderResourceView diffuseMapHR,
            float proportion)
        {
            this.Material = new BufferMaterials(material);
            this.NormalMap = normalMap;
            this.SpecularMap = specularMap;
            this.UseColorSpecular = specularMap != null;

            this.AlphaMap = alphaMap;
            this.ColorTextures = colorTextures;
            this.UseColorDiffuse = colorTextures != null;

            this.DiffuseMapLR = diffuseMapLR;
            this.DiffuseMapHR = diffuseMapHR;

            float usage = 0f;
            if (useAlphaMap) usage += 1;
            if (useSlopes) usage += 2;

            this.Parameters = new Vector4(usage, proportion, slopeRanges.X, slopeRanges.Y);
        }
    }
}
