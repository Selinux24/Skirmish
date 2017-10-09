
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    class EngineEffectVariableTexture
    {
        private EffectShaderResourceVariable variable = null;

        internal EngineEffectVariableTexture(EffectShaderResourceVariable variable)
        {
            this.variable = variable;
        }

        public void SetResource(EngineShaderResourceView resource)
        {
            if (resource != null)
            {
                this.variable.SetResource(resource.SRV);
            }
            else
            {
                this.variable.SetResource(null);
            }
        }

        public EngineShaderResourceView GetResource()
        {
            return new EngineShaderResourceView(this.variable.GetResource());
        }
    }
}
