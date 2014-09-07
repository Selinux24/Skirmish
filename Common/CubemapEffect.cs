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

    public class CubemapEffect : EffectBase
    {
        private EffectMatrixVariable worldViewProjection = null;
        private EffectShaderResourceVariable texture = null;

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

        public CubemapEffect(Device device)
            : base(VertexTypes.Position)
        {
            InputElement[] inputElements = VertexPosition.GetInput();
            string selectedTechnique = "Cubemap";

            this.effectInfo = ShaderUtils.LoadEffect(
                device,
                Resources.CubemapShader,
                selectedTechnique,
                inputElements);

            this.SelectedTechnique = this.Effect.GetTechniqueByName(selectedTechnique);
            if (this.SelectedTechnique == null)
            {
                throw new Exception("Técnica no localizada según formato de vértice.");
            }

            this.worldViewProjection = this.Effect.GetVariableByName("gWorldViewProjection").AsMatrix();
            this.texture = this.Effect.GetVariableByName("gCubemap").AsShaderResource();
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
            this.WorldViewProjection = mBuffer.WorldViewProjection;
            this.Texture = texture;
        }
    }
}
