
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    public class EngineEffectTechnique
    {
        private EffectTechnique techinque = null;

        internal EngineEffectTechnique(EffectTechnique techinque)
        {
            this.techinque = techinque;
        }

        public int PassCount
        {
            get
            {
                return this.techinque.Description.PassCount;
            }
        }

        public void Apply(Graphics graphics, int index, int flags)
        {
            this.techinque.GetPassByIndex(index).Apply(graphics.DeviceContext, flags);
        }

        public InputLayout Create(Graphics graphics, InputElement[] elements)
        {
            return new InputLayout(
                graphics.Device,
                this.techinque.GetPassByIndex(0).Description.Signature,
                elements);
        }
    }
}
