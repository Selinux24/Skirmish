
namespace Engine
{
    /// <summary>
    /// Particle manager description
    /// </summary>
    public class ParticleManagerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleManagerDescription()
            : base()
        {
            this.DeferredEnabled = false;
            this.AlphaEnabled = true;
        }
    }
}
