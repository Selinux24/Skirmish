using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using Effect = SharpDX.Direct3D11.Effect;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectPassDescription = SharpDX.Direct3D11.EffectPassDescription;
using EffectScalarVariable = SharpDX.Direct3D11.EffectScalarVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using EffectVariable = SharpDX.Direct3D11.EffectVariable;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    public class EffectBillboard : Drawer
    {
        #region Buffers

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

        private Device device = null;
        private Effect effect = null;
        private Dictionary<string, InputLayout> layouts = new Dictionary<string, InputLayout>();

        private EffectVariable dirLights = null;
        private EffectVariable pointLight = null;
        private EffectVariable spotLight = null;
        private EffectVectorVariable eyePositionWorld = null;
        private EffectScalarVariable fogStart = null;
        private EffectScalarVariable fogRange = null;
        private EffectVectorVariable fogColor = null;
        private EffectMatrixVariable worldViewProjection = null;
        private EffectVariable material = null;
        private EffectShaderResourceVariable texture = null;

        protected BufferDirectionalLight[] DirLights
        {
            get
            {
                using (DataStream ds = this.dirLights.GetRawValue(BufferDirectionalLight.SizeInBytes * 3))
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

                    this.dirLights.SetRawValue(ds, BufferDirectionalLight.SizeInBytes * 3);
                }
            }
        }
        protected BufferPointLight PointLight
        {
            get
            {
                using (DataStream ds = this.pointLight.GetRawValue(BufferPointLight.SizeInBytes))
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

                    this.pointLight.SetRawValue(ds, BufferPointLight.SizeInBytes);
                }
            }
        }
        protected BufferSpotLight SpotLight
        {
            get
            {
                using (DataStream ds = this.spotLight.GetRawValue(BufferSpotLight.SizeInBytes))
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

                    this.spotLight.SetRawValue(ds, BufferSpotLight.SizeInBytes);
                }
            }
        }
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
        protected BufferMaterials Material
        {
            get
            {
                using (DataStream ds = this.material.GetRawValue(BufferMaterials.SizeInBytes))
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

                    this.material.SetRawValue(ds, BufferMaterials.SizeInBytes);
                }
            }
        }
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

        public EffectBillboard.PerFrameBuffer FrameBuffer = new EffectBillboard.PerFrameBuffer();
        public EffectBillboard.PerObjectBuffer ObjectBuffer = new EffectBillboard.PerObjectBuffer();

        public EffectBillboard(Device device)
            : base()
        {
            this.device = device;
            this.effect = device.LoadEffect(Resources.ShaderBillboard);

            this.dirLights = this.effect.GetVariableByName("gDirLights");
            this.pointLight = this.effect.GetVariableByName("gPointLight");
            this.spotLight = this.effect.GetVariableByName("gSpotLight");
            this.eyePositionWorld = this.effect.GetVariableByName("gEyePositionWorld").AsVector();
            this.fogStart = this.effect.GetVariableByName("gFogStart").AsScalar();
            this.fogRange = this.effect.GetVariableByName("gFogRange").AsScalar();
            this.fogColor = this.effect.GetVariableByName("gFogColor").AsVector();
            this.worldViewProjection = this.effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.material = this.effect.GetVariableByName("gMaterial");
            this.texture = this.effect.GetVariableByName("gTreeMapArray").AsShaderResource();
        }
        public void Dispose()
        {
            if (this.effect != null)
            {
                this.effect.Dispose();
                this.effect = null;
            }
        }
        public EffectTechnique GetTechnique(string technique)
        {
            return this.effect.GetTechniqueByName(technique);
        }
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
        public void UpdatePerObject(ShaderResourceView texture)
        {
            this.Material = this.ObjectBuffer.Material;
            this.Texture = texture;
        }
        public string AddInputLayout(VertexTypes vertexType)
        {
            string technique = null;
            InputLayout layout = null;

            if (vertexType == VertexTypes.Billboard)
            {
                technique = "Billboard";

                InputElement[] input = VertexBillboard.GetInput();

                EffectPassDescription desc = effect.GetTechniqueByName(technique).GetPassByIndex(0).Description;

                layout = new InputLayout(
                    this.device,
                    desc.Signature,
                    input);

                if (!this.layouts.ContainsKey(technique))
                {
                    this.layouts.Add(technique, layout);
                }
                else
                {
                    this.layouts[technique] = layout;
                }

                return technique;
            }
            else
            {
                throw new Exception("VertexType unknown");
            }
        }
        public InputLayout GetInputLayout(string techniqueName)
        {
            return this.layouts[techniqueName];
        }
    }
}
