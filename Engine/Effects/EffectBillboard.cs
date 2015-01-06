using System;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectPassDescription = SharpDX.Direct3D11.EffectPassDescription;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Properties;

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
        /// World view projection effect variable
        /// </summary>
        private EffectMatrixVariable worldViewProjection = null;
        /// <summary>
        /// Material effect variable
        /// </summary>
        private EffectVariable material = null;
        /// <summary>
        /// Texture effect variable
        /// </summary>
        private EffectShaderResourceVariable texture = null;

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
        /// Texture
        /// </summary>
        protected ShaderResourceView Texture
        {
            get
            {
                return this.texture.GetResource();
            }
            set
            {
                this.texture.SetResource(value);
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
        public EffectBillboard(Device device)
            : base(device, Resources.ShaderBillboard)
        {
            this.dirLights = this.Effect.GetVariableByName("gDirLights");
            this.pointLight = this.Effect.GetVariableByName("gPointLight");
            this.spotLight = this.Effect.GetVariableByName("gSpotLight");
            this.eyePositionWorld = this.Effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.Effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.Effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.Effect.GetVariableByName("gFogColor").AsVector();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.material = this.Effect.GetVariableByName("gMaterial");
            this.texture = this.Effect.GetVariableByName("gTreeMapArray").AsShaderResource();
        }
        /// <summary>
        /// Finds technique and input layout for vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns technique name for specified vertex type</returns>
        public override string AddVertexType(VertexTypes vertexType)
        {
            string technique = null;
            InputLayout layout = null;

            if (vertexType == VertexTypes.Billboard)
            {
                technique = "Billboard";

                InputElement[] input = VertexBillboard.GetInput();

                EffectPassDescription desc = Effect.GetTechniqueByName(technique).GetPassByIndex(0).Description;

                layout = new InputLayout(
                    this.Device,
                    desc.Signature,
                    input);

                this.AddInputLayout(technique, layout);

                return technique;
            }
            else
            {
                throw new Exception("VertexType unknown");
            }
        }
        /// <summary>
        /// Update per frame data
        /// </summary>
        public void UpdatePerFrame()
        {
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
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
        }
        /// <summary>
        /// Update per model object data
        /// </summary>
        /// <param name="texture">Texture</param>
        public void UpdatePerObject(ShaderResourceView texture)
        {
            this.Material = this.ObjectBuffer.Material;
            this.Texture = texture;
        }
    }
}
