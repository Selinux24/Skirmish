using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using Effect = SharpDX.Direct3D11.Effect;
using EffectVectorVariable = SharpDX.Direct3D11.EffectVectorVariable;
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

    public class EffectFont : Drawer
    {
        #region Buffers

        [StructLayout(LayoutKind.Sequential)]
        public struct PerFrameBuffer
        {
            public Matrix World;
            public Matrix WorldViewProjection;
            public Color4 Color;

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

        private EffectMatrixVariable world = null;
        private EffectMatrixVariable worldViewProjection = null;
        private EffectVectorVariable color = null;
        private EffectShaderResourceVariable texture = null;

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
        protected Color4 Color
        {
            get
            {
                return this.color.GetVector<Color4>();
            }
            set
            {
                this.color.Set(value);
            }
        }

        public EffectFont.PerFrameBuffer FrameBuffer = new EffectFont.PerFrameBuffer();

        public EffectFont(Device device)
            : base()
        {
            this.device = device;
            this.effect = device.LoadEffect(Resources.ShaderFont);

            this.world = this.effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.color = this.effect.GetVariableByName("gColor").AsVector();
            this.texture = this.effect.GetVariableByName("gTexture").AsShaderResource();
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
        public string AddInputLayout(VertexTypes vertexType)
        {
            string technique = null;
            InputLayout layout = null;

            if (vertexType == VertexTypes.PositionTexture)
            {
                technique = "FontDrawer";

                InputElement[] input = VertexPositionTexture.GetInput();

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
        public void UpdatePerFrame(ShaderResourceView texture)
        {
            this.World = this.FrameBuffer.World;
            this.WorldViewProjection = this.FrameBuffer.WorldViewProjection;
            this.Color = this.FrameBuffer.Color;
            this.Texture = texture;
        }
    }
}
