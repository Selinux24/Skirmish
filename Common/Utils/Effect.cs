using Effect = SharpDX.Direct3D11.Effect;
using EffectTechnique = SharpDX.Direct3D11.EffectTechnique;
using InputLayout = SharpDX.Direct3D11.InputLayout;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Common.Utils
{
    public abstract class EffectBase : Drawer
    {
        protected EffectInfo effectInfo = null;
        public EffectTechnique SelectedTechnique { get; set; }
        public InputLayout Layout
        {
            get
            {
                return this.effectInfo.Layout;
            }
        }
        public Effect Effect
        {
            get
            {
                return this.effectInfo.Effect;
            }
        }
        public VertexTypes VertexType = VertexTypes.Unknown;
        public bool Textured
        {
            get
            {
                return
                    this.VertexType == VertexTypes.Billboard ||
                    this.VertexType == VertexTypes.PositionTexture ||
                    this.VertexType == VertexTypes.PositionNormalTexture;
            }
        }

        public EffectBase(VertexTypes vertexType)
        {
            this.VertexType = vertexType;
        }
        public virtual void Dispose()
        {

        }

        public abstract void UpdatePerFrame(BufferLights lBuffer);
        public abstract void UpdatePerObject(BufferMatrix mBuffer, ShaderResourceView texture);
    }
}
