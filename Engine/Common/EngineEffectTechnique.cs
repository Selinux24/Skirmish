
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Technique
    /// </summary>
    public class EngineEffectTechnique
    {
        /// <summary>
        /// Effect technique
        /// </summary>
        private EffectTechnique techinque = null;

        /// <summary>
        /// Gets the effect pass count
        /// </summary>
        public int PassCount
        {
            get
            {
                return this.techinque.Description.PassCount;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="techinque">Internal technique</param>
        public EngineEffectTechnique(EffectTechnique techinque)
        {
            this.techinque = techinque;
        }

        /// <summary>
        /// Apply the variable values to the effect technique
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="index">Index</param>
        /// <param name="flags">Flags</param>
        public void Apply(Graphics graphics, int index, int flags)
        {
            this.techinque.GetPassByIndex(index).Apply(graphics.DeviceContext, flags);
        }
        /// <summary>
        /// Creates a new input layout
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="elements">Elements</param>
        /// <returns></returns>
        public InputLayout Create(Graphics graphics, InputElement[] elements)
        {
            return new InputLayout(
                graphics.Device,
                this.techinque.GetPassByIndex(0).Description.Signature,
                elements);
        }
    }
}
