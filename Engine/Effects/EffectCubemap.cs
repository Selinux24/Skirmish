using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using Effect = SharpDX.Direct3D11.Effect;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectPassDescription = SharpDX.Direct3D11.EffectPassDescription;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    public class EffectCubemap : Drawer
    {
        #region Buffers

        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix WorldViewProjection;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(PerFrameBuffer));
                }
            }
        }

        #endregion

        private Device device = null;
        private Effect effect = null;
        private Dictionary<string, InputLayout> layouts = new Dictionary<string, InputLayout>();

        private EffectMatrixVariable worldViewProjection = null;
        private EffectShaderResourceVariable cubeTexture = null;

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

        public EffectCubemap.PerFrameBuffer FrameBuffer = new EffectCubemap.PerFrameBuffer();

        public EffectCubemap(Device device)
            : base()
        {
            this.device = device;
            this.effect = device.LoadEffect(Resources.ShaderCubemap);

            this.worldViewProjection = this.effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.cubeTexture = this.effect.GetVariableByName("gCubemap").AsShaderResource();
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
        }
        public void UpdatePerObject(ShaderResourceView cubeTexture)
        {
            this.CubeTexture = cubeTexture;
        }
        public string AddInputLayout(VertexTypes vertexType)
        {
            string technique = null;
            InputLayout layout = null;

            if (vertexType == VertexTypes.Position)
            {
                technique = "Cubemap";

                InputElement[] input = VertexPosition.GetInput();

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
