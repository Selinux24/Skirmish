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
    /// Cube map effect
    /// </summary>
    public class EffectCubemap : Drawer
    {
        /// <summary>
        /// Cubemap drawing technique
        /// </summary>
        public readonly EffectTechnique ForwardCubemap = null;
        /// <summary>
        /// Cubemap drawing technique
        /// </summary>
        public readonly EffectTechnique DeferredCubemap = null;

        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
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
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable cubeTexture = null;

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
        /// Texture
        /// </summary>
        protected ShaderResourceView CubeTexture
        {
            get
            {
                return this.cubeTexture.GetResource();
            }
            set
            {
                this.cubeTexture.SetResource(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectCubemap(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.ForwardCubemap = this.Effect.GetTechniqueByName("ForwardCubemap");
            this.DeferredCubemap = this.Effect.GetTechniqueByName("DeferredCubemap");

            this.AddInputLayout(this.ForwardCubemap, VertexPosition.GetInput());
            this.AddInputLayout(this.DeferredCubemap, VertexPosition.GetInput());

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLights = this.Effect.GetVariableByName("gPointLights");
            this.spotLights = this.Effect.GetVariableByName("gSpotLights");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.cubeTexture = this.Effect.GetVariableByName("gCubemap").AsShaderResource();
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
                if (vertexType == VertexTypes.Position)
                {
                    return this.ForwardCubemap;
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
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection)
        {
            this.UpdatePerFrame(world, viewProjection, Vector3.Zero, null);
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
            SceneLights lights)
        {
            this.WorldViewProjection = world * viewProjection;

            if (lights != null)
            {
                this.EyePositionWorld = eyePositionWorld;

                var dirLights = lights.EnabledDirectionalLights;
                var pointLights = lights.EnabledPointLights;
                var spotLights = lights.EnabledSpotLights;

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
            }
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(ShaderResourceView cubeTexture)
        {
            this.CubeTexture = cubeTexture;
        }
    }
}
