using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Font effect
    /// </summary>
    public class EffectDeferred : Drawer
    {
        #region Buffers

        /// <summary>
        /// Per frame update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix World;
            public Matrix WorldViewProjection;
            public Matrix InvertViewProjection;
            public Vector3 EyePositionWorld;
            public float Padding;
            public BufferDirectionalLight DirectionalLight;
            public BufferPointLight PointLight;
            public BufferSpotLight SpotLight;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerFrameBuffer));
                }
            }
        }

        #endregion

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
            this.directionalLight = this.Effect.GetVariableByName("gDirLight");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
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

        public void UpdatePerDirectionalLight(
            BufferDirectionalLight light,
            Matrix world,
            Matrix worldViewProjection,
            Vector3 eyePosition,
            ShaderResourceView colors,
            ShaderResourceView normals,
            ShaderResourceView depth)
        {
            this.DirectionalLight = light;
            this.World = world;
            this.WorldViewProjection = worldViewProjection;
            this.EyePositionWorld = eyePosition;
            this.ColorMap = colors;
            this.NormalMap = normals;
            this.DepthMap = depth;
            this.LightMap = null;
        }

        public void UpdatePerPointLight(
            BufferPointLight light,
            Matrix world,
            Matrix worldViewProjection,
            Vector3 eyePosition,
            ShaderResourceView colors,
            ShaderResourceView normals,
            ShaderResourceView depth,
            ShaderResourceView lights)
        {
            this.PointLight = light;
            this.World = world;
            this.WorldViewProjection = worldViewProjection;
            this.EyePositionWorld = eyePosition;
            this.ColorMap = colors;
            this.NormalMap = normals;
            this.DepthMap = depth;
            this.LightMap = lights;
        }

        public void UpdatePerSpotLight(
            BufferSpotLight light,
            Matrix world,
            Matrix worldViewProjection,
            Vector3 eyePosition,
            ShaderResourceView colors,
            ShaderResourceView normals,
            ShaderResourceView depth,
            ShaderResourceView lights)
        {
            this.SpotLight = light;
            this.World = world;
            this.WorldViewProjection = worldViewProjection;
            this.EyePositionWorld = eyePosition;
            this.ColorMap = colors;
            this.NormalMap = normals;
            this.DepthMap = depth;
            this.LightMap = lights;
        }

        public void UpdatePerCombineLights(
            Matrix world,
            Matrix worldViewProjection,
            Vector3 eyePosition,
            ShaderResourceView colors,
            ShaderResourceView normals,
            ShaderResourceView depth,
            ShaderResourceView lights)
        {
            this.World = world;
            this.WorldViewProjection = worldViewProjection;
            this.EyePositionWorld = eyePosition;
            this.ColorMap = colors;
            this.NormalMap = normals;
            this.DepthMap = depth;
            this.LightMap = lights;
        }
    }
}
