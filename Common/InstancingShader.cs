using System;
using System.Collections.Generic;
using Device = SharpDX.Direct3D11.Device;
using InputElement = SharpDX.Direct3D11.InputElement;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common
{
    using Common.Utils;

    public class InstancingShader : ShaderBase
    {
        public InstancingShader(Device device, VertexTypes vertexType)
            : base(device, vertexType)
        {
            string vsEntryPoint;
            string psEntryPoint;
            List<InputElement> inputElements = new List<InputElement>();
            if (this.VertexType == VertexTypes.PositionColor)
            {
                vsEntryPoint = "VSPositionColor";
                psEntryPoint = "PSPositionColor";
                inputElements.AddRange(VertexPositionColor.GetInput());
            }
            else if (this.VertexType == VertexTypes.PositionNormalColor)
            {
                vsEntryPoint = "VSPositionNormalColor";
                psEntryPoint = "PSPositionNormalColor";
                inputElements.AddRange(VertexPositionNormalColor.GetInput());
            }
            else if (this.VertexType == VertexTypes.PositionTexture)
            {
                vsEntryPoint = "VSPositionTexture";
                psEntryPoint = "PSPositionTexture";
                inputElements.AddRange(VertexPositionTexture.GetInput());
            }
            else if (this.VertexType == VertexTypes.PositionNormalTexture)
            {
                vsEntryPoint = "VSPositionNormalTexture";
                psEntryPoint = "PSPositionNormalTexture";
                inputElements.AddRange(VertexPositionNormalTexture.GetInput());
            }
            else
            {
                throw new Exception("Formato de vértice no válido");
            }

            inputElements.AddRange(BufferInstancingData.GetInput());

            this.vertexShader = device.LoadVertexShader(
                Properties.Resources.InstancingShader,
                vsEntryPoint,
                inputElements.ToArray());

            this.pixelShader = device.LoadPixelShader(
                Properties.Resources.InstancingShader,
                psEntryPoint);
        }

        public override void UpdatePerFrame(
            BufferLights lBuffer)
        {
            this.deviceContext.WriteConstantBuffer(this.perFrameBuffer, lBuffer, 0);

            this.deviceContext.VertexShader.SetConstantBuffer(0, this.perFrameBuffer);
            this.deviceContext.PixelShader.SetConstantBuffer(0, this.perFrameBuffer);
        }
        public override void UpdatePerObject(
            BufferMatrix mBuffer,
            ShaderResourceView texture)
        {
            mBuffer.World.Transpose();
            mBuffer.WorldInverse.Transpose();
            mBuffer.WorldViewProjection.Transpose();

            this.deviceContext.WriteConstantBuffer(this.perObjectBuffer, mBuffer, 0);

            this.deviceContext.VertexShader.SetConstantBuffer(1, this.perObjectBuffer);
            this.deviceContext.PixelShader.SetConstantBuffer(1, this.perObjectBuffer);

            this.deviceContext.PixelShader.SetShaderResource(0, this.Textured ? texture : null);
        }
    }
}
