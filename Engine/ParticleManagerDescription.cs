
namespace Engine
{
    /// <summary>
    /// Particle manager description
    /// </summary>
    public class ParticleManagerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Gets the default particle manager descriptor
        /// </summary>
        public static ParticleManagerDescription Default()
        {
            return new ParticleManagerDescription();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleManagerDescription()
            : base()
        {
            DeferredEnabled = false;
            BlendMode = BlendModes.Default;
        }
    }
}
