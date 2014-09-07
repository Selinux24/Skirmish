using SharpDX.Direct3D11;

namespace Common.Utils
{
    public class EffectInfo : System.IDisposable
    {
        public Effect Effect { get; private set; }
        public InputLayout Layout { get; private set; }

        internal EffectInfo(Effect effect, InputLayout layout)
        {
            this.Effect = effect;
            this.Layout = layout;
        }
        public void Dispose()
        {
            if (this.Effect != null)
            {
                this.Effect.Dispose();
                this.Effect = null;
            }

            if (this.Layout != null)
            {
                this.Layout.Dispose();
                this.Layout = null;
            }
        }
    }
}
