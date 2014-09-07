using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using PixelShader = SharpDX.Direct3D11.PixelShader;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using VertexShader = SharpDX.Direct3D11.VertexShader;

namespace Common.Utils
{
    public abstract class ShaderBase : Drawer
    {
        protected DeviceContext deviceContext;
        protected VertexShaderInfo vertexShader = null;
        protected PixelShaderInfo pixelShader = null;
        protected Buffer perObjectBuffer = null;
        protected Buffer perFrameBuffer = null;

        public VertexTypes VertexType = VertexTypes.Unknown;
        public bool Textured
        {
            get
            {
                return
                    this.VertexType == VertexTypes.PositionTexture ||
                    this.VertexType == VertexTypes.PositionNormalTexture;
            }
        }
        public VertexShader VertexShader
        {
            get
            {
                return this.vertexShader.Shader;
            }
        }
        public InputLayout Layout
        {
            get
            {
                return this.vertexShader.Layout;
            }
        }
        public PixelShader PixelShader
        {
            get
            {
                return this.pixelShader.Shader;
            }
        }

        public ShaderBase(Device device, VertexTypes vertexType)
        {
            this.deviceContext = device.ImmediateContext;
            this.VertexType = vertexType;

            this.perObjectBuffer = device.CreateConstantBuffer<BufferMatrix>();
            this.perFrameBuffer = device.CreateConstantBuffer<BufferLights>();
        }
        public void Dispose()
        {
            if (this.perObjectBuffer != null)
            {
                this.perObjectBuffer.Dispose();
                this.perObjectBuffer = null;
            }

            if (this.perFrameBuffer != null)
            {
                this.perFrameBuffer.Dispose();
                this.perFrameBuffer = null;
            }

            if (this.pixelShader != null)
            {
                this.pixelShader.Dispose();
                this.pixelShader = null;
            }

            if (this.vertexShader != null)
            {
                this.vertexShader.Dispose();
                this.vertexShader = null;
            }
        }

        public abstract void UpdatePerFrame(BufferLights lBuffer);
        public abstract void UpdatePerObject(BufferMatrix mBuffer, ShaderResourceView texture);
    }
}
