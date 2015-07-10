using System;
using System.Runtime.InteropServices;
using SharpDX;
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
        #region Buffers

        /// <summary>
        /// Per frame update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix WorldViewProjection;
            public Matrix ShadowTransform;
            public BufferLights Lights;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerFrameBuffer));
                }
            }
        }
        /// <summary>
        /// Per model object update buffer
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct PerObjectBuffer
        {
            public BufferMaterials Material;
            public uint TextureCount;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerObjectBuffer));
                }
            }
        }

        #endregion

        /// <summary>
        /// Billboard drawing technique
        /// </summary>
        public readonly EffectTechnique Billboard = null;
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
        private EffectVariable pointLight = null;
        /// <summary>
        /// Spot light effect variable
        /// </summary>
        private EffectVariable spotLight = null;
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
        /// Enable shados effect variable
        /// </summary>
        private EffectScalarVariable enableShadows = null;
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
        /// Enable shadows
        /// </summary>
        protected float EnableShadows
        {
            get
            {
                return this.enableShadows.GetFloat();
            }
            set
            {
                this.enableShadows.Set(value);
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
        /// Per frame buffer structure
        /// </summary>
        public EffectBillboard.PerFrameBuffer FrameBuffer = new EffectBillboard.PerFrameBuffer();
        /// <summary>
        /// Per model object buffer structure
        /// </summary>
        public EffectBillboard.PerObjectBuffer ObjectBuffer = new EffectBillboard.PerObjectBuffer();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect code</param>
        /// <param name="compile">Compile code</param>
        public EffectBillboard(Device device, byte[] effect, bool compile)
            : base(device, effect, compile)
        {
            this.Billboard = this.Effect.GetTechniqueByName("Billboard");
            this.ShadowMapBillboard = this.Effect.GetTechniqueByName("ShadowMapBillboard");

            this.AddInputLayout(this.Billboard, VertexBillboard.GetInput());
            this.AddInputLayout(this.ShadowMapBillboard, VertexBillboard.GetInput());

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.shadowTransform = this.Effect.GetVariableByName("gShadowTransform").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.enableShadows = this.Effect.GetVariableByName("gEnableShadows").AsScalar();
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
                    return this.Billboard;
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
        /// <param name="shadowMap">Shadow map texture</param>
        public void UpdatePerFrame(ShaderResourceView shadowMap)
        {
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
            this.ShadowTransform = this.FrameBuffer.ShadowTransform;

            this.DirLights = new BufferDirectionalLight[]
            {
                this.FrameBuffer.Lights.DirectionalLight1,
                this.FrameBuffer.Lights.DirectionalLight2,
                this.FrameBuffer.Lights.DirectionalLight3,
            };
            this.PointLight = this.FrameBuffer.Lights.PointLight;
            this.SpotLight = this.FrameBuffer.Lights.SpotLight;
            this.EyePositionWorld = this.FrameBuffer.Lights.EyePositionWorld;

            this.FogStart = this.FrameBuffer.Lights.FogStart;
            this.FogRange = this.FrameBuffer.Lights.FogRange;
            this.FogColor = this.FrameBuffer.Lights.FogColor;
            this.EnableShadows = shadowMap != null ? this.FrameBuffer.Lights.EnableShadows : 0;

            this.ShadowMap = shadowMap;
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(ShaderResourceView texture)
        {
            this.Material = this.ObjectBuffer.Material;
            this.TextureCount = this.ObjectBuffer.TextureCount;
            this.Textures = texture;
        }
    }
}
