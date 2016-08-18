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
        /// Low resolution textures effect variable
        /// </summary>
        private EffectShaderResourceVariable texturesLR = null;
        /// <summary>
        /// High resolution textures effect variable
        /// </summary>
        private EffectShaderResourceVariable texturesHR = null;
        /// <summary>
        /// Normal map effect variable
        /// </summary>
        private EffectShaderResourceVariable normalMaps = null;
        /// <summary>
        /// Shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMap = null;
        /// <summary>
        /// Slope ranges effect variable
        /// </summary>
        private EffectVectorVariable slopeRanges = null;

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
        /// Low resolution textures
        /// </summary>
        protected ShaderResourceView TexturesLR
        {
            get
            {
                return this.texturesLR.GetResource();
            }
            set
            {
                this.texturesLR.SetResource(value);
            }
        }
        /// <summary>
        /// High resolution textures
        /// </summary>
        protected ShaderResourceView TexturesHR
        {
            get
            {
                return this.texturesHR.GetResource();
            }
            set
            {
                this.texturesHR.SetResource(value);
            }
        }
        /// <summary>
        /// Normal map
        /// </summary>
        protected ShaderResourceView NormalMaps
        {
            get
            {
                return this.normalMaps.GetResource();
            }
            set
            {
                this.normalMaps.SetResource(value);
            }
        }
        /// <summary>
        /// Shadow map
        /// </summary>
        protected ShaderResourceView ShadowMap
        {
            get
            {
                return this.shadowMap.GetResource();
            }
            set
            {
                this.shadowMap.SetResource(value);
            }
        }
        /// <summary>
        /// Slope ranges
        /// </summary>
        protected Vector2 SlopeRanges
        {
            get
            {
                Vector4 v = this.slopeRanges.GetFloatVector();

                return new Vector2(v.X, v.Y);
            }
            set
            {
                Vector4 v4 = new Vector4(value.X, value.Y, 0f, 1f);

                this.slopeRanges.Set(v4);
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
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPointLights");
            this.spotLights = this.Effect.GetVariableByName("gSpotLights");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.texturesLR = this.Effect.GetVariableByName("gTextureLRArray").AsShaderResource();
            this.texturesHR = this.Effect.GetVariableByName("gTextureHRArray").AsShaderResource();
            this.normalMaps = this.Effect.GetVariableByName("gNormalMapArray").AsShaderResource();
            this.shadowMap = this.Effect.GetVariableByName("gShadowMap").AsShaderResource();
            this.slopeRanges = this.Effect.GetVariableByName("gSlopeRanges").AsVector();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage, DrawerModesEnum mode)
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
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, new BoundingFrustum(), null, null, Matrix.Identity);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="viewFrustum">Camera frustum</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMap">Shadow map texture</param>
        /// <param name="fromLightViewProjection">From light View * Projection transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            BoundingFrustum viewFrustum,
            SceneLights lights,
            ShaderResourceView shadowMap,
            Matrix fromLightViewProjection)
        {
            this.World = world;
            this.WorldInverse = Matrix.Invert(world);
            this.WorldViewProjection = world * viewProjection;

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                var dirLights = lights.GetVisibleDirectionalLights(viewFrustum);
                var pointLights = lights.GetVisiblePointLights(viewFrustum);
                var spotLights = lights.GetVisibleSpotLights(viewFrustum);

                this.DirLights = new[]
                {
                    dirLights.Length > 0 ? new BufferDirectionalLight(dirLights[0]) : new BufferDirectionalLight(),
                    dirLights.Length > 1 ? new BufferDirectionalLight(dirLights[1]) : new BufferDirectionalLight(),
                    dirLights.Length > 2 ? new BufferDirectionalLight(dirLights[2]) : new BufferDirectionalLight(),
                };
                this.PointLights = new[]
                {
                    pointLights.Length > 0 ? new BufferPointLight(pointLights[0]) : new BufferPointLight(),
                    pointLights.Length > 1 ? new BufferPointLight(pointLights[1]) : new BufferPointLight(),
                    pointLights.Length > 2 ? new BufferPointLight(pointLights[2]) : new BufferPointLight(),
                    pointLights.Length > 3 ? new BufferPointLight(pointLights[3]) : new BufferPointLight(),
                };
                this.SpotLights = new[]
                {
                    spotLights.Length > 0 ? new BufferSpotLight(spotLights[0]) : new BufferSpotLight(),
                    spotLights.Length > 1 ? new BufferSpotLight(spotLights[1]) : new BufferSpotLight(),
                    spotLights.Length > 2 ? new BufferSpotLight(spotLights[2]) : new BufferSpotLight(),
                    spotLights.Length > 3 ? new BufferSpotLight(spotLights[3]) : new BufferSpotLight(),
                };

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.FromLightViewProjection = fromLightViewProjection;
                this.ShadowMap = shadowMap;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.DirLights = new BufferDirectionalLight[BufferDirectionalLight.MAX];
                this.PointLights = new BufferPointLight[BufferPointLight.MAX];
                this.SpotLights = new BufferSpotLight[BufferSpotLight.MAX];

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.FromLightViewProjection = Matrix.Identity;
                this.ShadowMap = null;
            }
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="material">Material</param>
        /// <param name="texturesLR">Low resolution textures</param>
        /// <param name="texturesHR">High resolution textures</param>
        /// <param name="normalMap">Normal map</param>
        /// <param name="slopeRanges">Slope ranges</param>
        public void UpdatePerObject(
            Material material,
            ShaderResourceView texturesLR,
            ShaderResourceView texturesHR,
            ShaderResourceView normalMap,
            Vector2 slopeRanges)
        {
            this.Material = new BufferMaterials(material);
            this.TexturesLR = texturesLR;
            this.TexturesHR = texturesHR;
            this.NormalMaps = normalMap;
            this.SlopeRanges = slopeRanges;
        }
    }
}
