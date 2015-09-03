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
    /// Billboard effect
    /// </summary>
    public class EffectBillboard : Drawer
    {
        /// <summary>
        /// Billboard drawing technique
        /// </summary>
        public readonly EffectTechnique ForwardBillboard = null;
        /// <summary>
        /// Billboard drawing technique
        /// </summary>
        public readonly EffectTechnique DeferredBillboard = null;
        /// <summary>
        /// Billboard shadow map drawing technique
        /// </summary>
        public readonly EffectTechnique ShadowMapBillboard = null;

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
        /// Bounding sphere
        /// </summary>
        private EffectScalarVariable radius = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Shadow transform
        /// </summary>
        private EffectMatrixVariable shadowTransform = null;
        /// <summary>
        /// Material effect variable
        /// </summary>
        private EffectVariable material = null;
        /// <summary>
        /// Texture count variable
        /// </summary>
        private EffectScalarVariable textureCount = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable textures = null;
        /// <summary>
        /// Shadow map effect variable
        /// </summary>
        private EffectShaderResourceVariable shadowMap = null;

        /// <summary>
        /// Directional lights
        /// </summary>
        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(default(BufferDirectionalLight).Stride * 3))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferDirectionalLight>(3);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferDirectionalLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.dirLights.SetRawValue(ds, default(BufferDirectionalLight).Stride * 3);
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
                using (DataStream ds = this.pointLights.GetRawValue(default(BufferPointLight).Stride * 4))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferPointLight>(4);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferPointLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.pointLights.SetRawValue(ds, default(BufferPointLight).Stride * 4);
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
                using (DataStream ds = this.spotLights.GetRawValue(default(BufferSpotLight).Stride * 4))
                {
                    ds.Position = 0;

                    return ds.ReadRange<BufferSpotLight>(4);
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferSpotLight>(value, true, false))
                {
                    ds.Position = 0;

                    this.spotLights.SetRawValue(ds, default(BufferSpotLight).Stride * 4);
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
        /// Bounding sphere
        /// </summary>
        protected float Radius
        {
            get
            {
                return this.radius.GetFloat();
            }
            set
            {
                this.radius.Set(value);
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
        /// Shadow transform
        /// </summary>
        protected Matrix ShadowTransform
        {
            get
            {
                return this.shadowTransform.GetMatrix();
            }
            set
            {
                this.shadowTransform.SetMatrix(value);
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
        /// Texture count
        /// </summary>
        protected uint TextureCount
        {
            get
            {
                return (uint)this.textureCount.GetInt();
            }
            set
            {
                this.textureCount.Set(value);
            }
        }
        /// <summary>
        /// Texture
        /// </summary>
        protected ShaderResourceView Textures
        {
            get
            {
                return this.textures.GetResource();
            }
            set
            {
                this.textures.SetResource(value);
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
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectBillboard(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ForwardBillboard = this.Effect.GetTechniqueByName("ForwardBillboard");
            this.DeferredBillboard = this.Effect.GetTechniqueByName("DeferredBillboard");
            this.ShadowMapBillboard = this.Effect.GetTechniqueByName("ShadowMapBillboard");

            this.AddInputLayout(this.ForwardBillboard, VertexBillboard.GetInput());
            this.AddInputLayout(this.DeferredBillboard, VertexBillboard.GetInput());
            this.AddInputLayout(this.ShadowMapBillboard, VertexBillboard.GetInput());

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.shadowTransform = this.Effect.GetVariableByName("gShadowTransform").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPointLights");
            this.spotLights = this.Effect.GetVariableByName("gSpotLights");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.radius = this.Effect.GetVariableByName("gRadius").AsScalar();
            this.textureCount = this.Effect.GetVariableByName("gTextureCount").AsScalar();
            this.textures = this.Effect.GetVariableByName("gTextureArray").AsShaderResource();
            this.shadowMap = this.Effect.GetVariableByName("gShadowMap").AsShaderResource();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="stage">Stage</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public override EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage)
        {
            if (stage == DrawingStages.Drawing)
            {
                if (vertexType == VertexTypes.Billboard)
                {
                    return this.ForwardBillboard;
                }
                else
                {
                    throw new Exception(string.Format("Bad vertex type for effect and stage: {0} - {1}", vertexType, stage));
                }
            }
            else
            {
                throw new Exception(string.Format("Bad stage for effect: {0}", stage));
            }
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
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, null, null, Matrix.Identity);
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        /// <param name="world">World</param>
        /// <param name="viewProjection">View * projection</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="lights">Scene ligths</param>
        /// <param name="shadowMap">Shadow map texture</param>
        /// <param name="shadowTransform">Shadow transform</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            SceneLights lights,
            ShaderResourceView shadowMap,
            Matrix shadowTransform)
        {
            this.WorldViewProjection = world * viewProjection;

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                this.DirLights = new[]
                {
                    lights.DirectionalLights.Length > 0 ? new BufferDirectionalLight(lights.DirectionalLights[0]) : new BufferDirectionalLight(),
                    lights.DirectionalLights.Length > 1 ? new BufferDirectionalLight(lights.DirectionalLights[1]) : new BufferDirectionalLight(),
                    lights.DirectionalLights.Length > 2 ? new BufferDirectionalLight(lights.DirectionalLights[2]) : new BufferDirectionalLight(),
                };
                this.PointLights = new[]
                {
                    lights.PointLights.Length > 0 ? new BufferPointLight(lights.PointLights[0]) : new BufferPointLight(),
                    lights.PointLights.Length > 1 ? new BufferPointLight(lights.PointLights[1]) : new BufferPointLight(),
                    lights.PointLights.Length > 2 ? new BufferPointLight(lights.PointLights[2]) : new BufferPointLight(),
                    lights.PointLights.Length > 3 ? new BufferPointLight(lights.PointLights[3]) : new BufferPointLight(),
                };
                this.SpotLights = new[]
                {
                    lights.SpotLights.Length > 0 ? new BufferSpotLight(lights.SpotLights[0]) : new BufferSpotLight(),
                    lights.SpotLights.Length > 1 ? new BufferSpotLight(lights.SpotLights[1]) : new BufferSpotLight(),
                    lights.SpotLights.Length > 2 ? new BufferSpotLight(lights.SpotLights[2]) : new BufferSpotLight(),
                    lights.SpotLights.Length > 3 ? new BufferSpotLight(lights.SpotLights[3]) : new BufferSpotLight(),
                };

                this.FogStart = lights.FogStart;
                this.FogRange = lights.FogRange;
                this.FogColor = lights.FogColor;

                this.ShadowTransform = shadowTransform;
                this.ShadowMap = shadowMap;
            }
            else
            {
                this.EyePositionWorld = Vector3.Zero;

                this.DirLights = new BufferDirectionalLight[3];
                this.PointLights = new BufferPointLight[4];
                this.SpotLights = new BufferSpotLight[4];

                this.FogStart = 0;
                this.FogRange = 0;
                this.FogColor = Color.Transparent;

                this.ShadowTransform = Matrix.Identity;
                this.ShadowMap = null;
            }
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="material">Material</param>
        /// <param name="radius">Drawing radius</param>
        /// <param name="textureCount">Texture count</param>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(
            Material material,
            float radius,
            uint textureCount,
            ShaderResourceView texture)
        {
            this.Material = new BufferMaterials(material);
            this.Radius = radius;
            this.TextureCount = textureCount;
            this.Textures = texture;
        }
    }
}
