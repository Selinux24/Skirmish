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
            public Matrix WorldInverse;
            public Matrix WorldViewProjection;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerFrameBuffer));
                }
            }
        }
        /// <summary>
        /// Per directional light update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerDirectionalLightBuffer
        {
            public BufferDirectionalLight DirectionalLight;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerDirectionalLightBuffer));
                }
            }
        }
        /// <summary>
        /// Per point light update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerPointLightBuffer
        {
            public BufferPointLight PointLight;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerPointLightBuffer));
                }
            }
        }
        /// <summary>
        /// Per spot light update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerSpotLightBuffer
        {
            public BufferSpotLight SpotLight;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerSpotLightBuffer));
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
        /// Per frame buffer structure
        /// </summary>
        public EffectDeferred.PerFrameBuffer FrameBuffer = new EffectDeferred.PerFrameBuffer();

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

            this.AddInputLayout(this.DeferredDirectionalLight, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.DeferredPointLight, VertexPositionTexture.GetInput());
            this.AddInputLayout(this.DeferredSpotLight, VertexPositionTexture.GetInput());

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldInverse = this.Effect.GetVariableByName("gWorldInverse").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.directionalLight = this.Effect.GetVariableByName("gDirLight");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
            this.colorMap = this.Effect.GetVariableByName("gColorMap").AsShaderResource();
            this.normalMap = this.Effect.GetVariableByName("gNormalMap").AsShaderResource();
            this.depthMap = this.Effect.GetVariableByName("gDepthMap").AsShaderResource();
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
                if (vertexType == VertexTypes.PositionTexture)
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
        /// Update per frame data
        /// </summary>
        public void UpdatePerFrame(ShaderResourceView colors, ShaderResourceView normals, ShaderResourceView depth)
        {
            this.World = this.FrameBuffer.World;
            this.WorldInverse = this.FrameBuffer.WorldInverse;
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;

            this.ColorMap = colors;
            this.NormalMap = normals;
            this.DepthMap = depth;
        }

        public void UpdatePerDirectionalLight(BufferDirectionalLight light)
        {
            this.DirectionalLight = light;
        }

        public void UpdatePerPointLight(BufferPointLight light)
        {
            this.PointLight = light;
        }

        public void UpdatePerSpotLight(BufferSpotLight light)
        {
            this.SpotLight = light;
        }
    }
}
