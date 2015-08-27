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
    /// Font effect
    /// </summary>
    public class EffectDeferred : Drawer
    {
        /// <summary>
        /// Directional light technique
        /// </summary>
        public readonly EffectTechnique DeferredDirectionalLight = null;
        /// <summary>
        /// Point light technique
        /// </summary>
        public readonly EffectTechnique DeferredPointLight = null;
        /// <summary>
        /// Spot light technique
        /// </summary>
        public readonly EffectTechnique DeferredSpotLight = null;
        /// <summary>
        /// Technique to combine all light sources
        /// </summary>
        public readonly EffectTechnique DeferredCombineLights = null;

        /// <summary>
        /// World matrix effect variable
        /// </summary>
        private EffectMatrixVariable world = null;
        /// <summary>
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Eye position effect variable
        /// </summary>
        private EffectVectorVariable eyePositionWorld = null;
        /// <summary>
        /// Ambient light color
        /// </summary>
        private EffectVectorVariable ambientColor = null;
        /// <summary>
        /// Directional light effect variable
        /// </summary>
        private EffectVariable directionalLight = null;
        /// <summary>
        /// Point light effect variable
        /// </summary>
        private EffectVariable pointLight = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EffectVariable spotLight = null;
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
        /// Color Map effect variable
        /// </summary>
        private EffectShaderResourceVariable colorMap = null;
        /// <summary>
        /// Normal Map effect variable
        /// </summary>
        private EffectShaderResourceVariable normalMap = null;
        /// <summary>
        /// Depth Map effect variable
        /// </summary>
        private EffectShaderResourceVariable depthMap = null;
        /// <summary>
        /// Light Map effect variable
        /// </summary>
        private EffectShaderResourceVariable lightMap = null;

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
        /// Ambient light color
        /// </summary>
        protected Color4 AmbientColor
        {
            get
            {
                return new Color4(this.ambientColor.GetFloatVector());
            }
            set
            {
                this.ambientColor.Set(value);
            }
        }
        /// <summary>
        /// Directional light
        /// </summary>
        protected BufferDirectionalLight DirectionalLight
        {
            get
            {
                using (DataStream ds = this.directionalLight.GetRawValue(default(BufferDirectionalLight).Stride))
                {
                    ds.Position = 0;

                    return ds.Read<BufferDirectionalLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferDirectionalLight>(new BufferDirectionalLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.directionalLight.SetRawValue(ds, default(BufferDirectionalLight).Stride);
                }
            }
        }
        /// <summary>
        /// Point light
        /// </summary>
        protected BufferPointLight PointLight
        {
            get
            {
                using (DataStream ds = this.pointLight.GetRawValue(default(BufferPointLight).Stride))
                {
                    ds.Position = 0;

                    return ds.Read<BufferPointLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferPointLight>(new BufferPointLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.pointLight.SetRawValue(ds, default(BufferPointLight).Stride);
                }
            }
        }
        /// <summary>
        /// Spot light
        /// </summary>
        protected BufferSpotLight SpotLight
        {
            get
            {
                using (DataStream ds = this.spotLight.GetRawValue(default(BufferSpotLight).Stride))
                {
                    ds.Position = 0;

                    return ds.Read<BufferSpotLight>();
                }
            }
            set
            {
                using (DataStream ds = DataStream.Create<BufferSpotLight>(new BufferSpotLight[] { value }, true, false))
                {
                    ds.Position = 0;

                    this.spotLight.SetRawValue(ds, default(BufferSpotLight).Stride);
                }
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
        /// Color Map
        /// </summary>
        protected ShaderResourceView ColorMap
        {
            get
            {
                return this.colorMap.GetResource();
            }
            set
            {
                this.colorMap.SetResource(value);
            }
        }
        /// <summary>
        /// Normal Map
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
        /// Depth Map
        /// </summary>
        protected ShaderResourceView DepthMap
        {
            get
            {
                return this.depthMap.GetResource();
            }
            set
            {
                this.depthMap.SetResource(value);
            }
        }
        /// <summary>
        /// Light Map
        /// </summary>
        protected ShaderResourceView LightMap
        {
            get
            {
                return this.lightMap.GetResource();
            }
            set
            {
                this.lightMap.SetResource(value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectDeferred(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.DeferredDirectionalLight = this.Effect.GetTechniqueByName("DeferredDirectionalLight");
            this.DeferredPointLight = this.Effect.GetTechniqueByName("DeferredPointLight");
            this.DeferredSpotLight = this.Effect.GetTechniqueByName("DeferredSpotLight");
            this.DeferredCombineLights = this.Effect.GetTechniqueByName("DeferredCombineLights");

            this.AddInputLayout(this.DeferredDirectionalLight, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.DeferredPointLight, VertexPosition.GetInput());
            this.AddInputLayout(this.DeferredSpotLight, VertexPosition.GetInput());
            this.AddInputLayout(this.DeferredCombineLights, VertexPositionTexture.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.ambientColor = this.Effect.GetVariableByName("gAmbientColor").AsVector();
            this.directionalLight = this.Effect.GetVariableByName("gDirLight");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.colorMap = this.Effect.GetVariableByName("gColorMap").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gNormalMap").AsShaderResource();
            this.depthMap = this.Effect.GetVariableByName("gDepthMap").AsShaderResource();
            this.lightMap = this.Effect.GetVariableByName("gLightMap").AsShaderResource();
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
                    return this.DeferredSpotLight;
                }
                else if (vertexType == VertexTypes.PositionTexture)
                {
                    return this.DeferredDirectionalLight;
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
        /// Updates per frame variables
        /// </summary>
        /// <param name="world">World matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        /// <param name="eyePositionWorld">Eye position in world coordinates</param>
        /// <param name="colorMap">Color map texture</param>
        /// <param name="normalMap">Normal map texture</param>
        /// <param name="depthMap">Depth map texture</param>
        public void UpdatePerFrame(
            Matrix world,
            Matrix viewProjection,
            Vector3 eyePositionWorld,
            ShaderResourceView colorMap,
            ShaderResourceView normalMap,
            ShaderResourceView depthMap)
        {
            this.World = world;
            this.WorldViewProjection = world * viewProjection;
            this.EyePositionWorld = eyePositionWorld;
            this.ColorMap = colorMap;
            this.NormalMap = normalMap;
            this.DepthMap = depthMap;
        }
        /// <summary>
        /// Updates per directional light variables
        /// </summary>
        /// <param name="light">Light</param>
        public void UpdatePerLight(SceneLightDirectional light)
        {
            this.DirectionalLight = new BufferDirectionalLight(light);
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerLight(SceneLightPoint light, Matrix transform, Matrix viewProjection)
        {
            this.PointLight = new BufferPointLight(light);
            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;
        }
        /// <summary>
        /// Updates per spot light variables
        /// </summary>
        /// <param name="light">Light</param>
        /// <param name="transform">Translation and rotation matrix</param>
        /// <param name="viewProjection">View * projection matrix</param>
        public void UpdatePerLight(SceneLightSpot light, Matrix transform, Matrix viewProjection)
        {
            this.SpotLight = new BufferSpotLight(light);
            this.World = transform;
            this.WorldViewProjection = transform * viewProjection;
        }
        /// <summary>
        /// Updates composer variables
        /// </summary>
        /// <param name="ambientColor">Ambient color</param>
        /// <param name="fogStart">Fog start</param>
        /// <param name="fogRange">Fog range</param>
        /// <param name="fogColor">Fog color</param>
        /// <param name="lightMap">Light map</param>
        public void UpdateComposer(Color4 ambientColor, float fogStart, float fogRange, Color4 fogColor, ShaderResourceView lightMap)
        {
            this.AmbientColor = ambientColor;

            this.FogStart = fogStart;
            this.FogRange = fogRange;
            this.FogColor = fogColor;

            this.LightMap = lightMap;
        }
    }
}
