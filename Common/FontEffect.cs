using System;
using SharpDX;
using Device = SharpDX.Direct3D11.Device;
using EffectMatrixVariable = SharpDX.Direct3D11.EffectMatrixVariable;
using EffectShaderResourceVariable = SharpDX.Direct3D11.EffectShaderResourceVariable;
using InputElement = SharpDX.Direct3D11.InputElement;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common
{
    using Common.Properties;
    using Common.Utils;

    public class FontEffect : EffectBase
    {
        private EffectMatrixVariable world = null;
        private EffectMatrixVariable worldViewProjection = null;
        private EffectShaderResourceVariable texture = null;

        public Matrix World
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
        public Matrix WorldViewProjection
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
        public ShaderResourceView Texture
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

        public FontEffect(Device device)
            : base(VertexTypes.PositionTexture)
        {
            InputElement[] inputElements = VertexPositionTexture.GetInput();
            string selectedTechnique = "FontDrawer";

            this.effectInfo = ShaderUtils.LoadEffect(
                device,
                Resources.FontShader,
                selectedTechnique,
                inputElements);

            this.SelectedTechnique = this.Effect.GetTechniqueByName(selectedTechnique);
            if (this.SelectedTechnique == null)
            {
                throw new Exception("Técnica no localizada según formato de vértice.");
            }

            this.world = this.Effect.GetVariableByName("gWorld").AsMatrix();
            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.texture = this.Effect.GetVariableByName("gTexture").AsShaderResource();
        }
        public override void Dispose()
        {
            if (this.effectInfo != null)
            {
                this.effectInfo.Dispose();
                this.effectInfo = null;
            }
        }

        public override void UpdatePerFrame(
            BufferLights lBuffer)
        {

        }
        public override void UpdatePerObject(
            BufferMatrix mBuffer,
            ShaderResourceView texture)
        {
            this.World = mBuffer.World;
            this.WorldViewProjection = mBuffer.WorldViewProjection;
            this.Texture = texture;
        }
    }
}
