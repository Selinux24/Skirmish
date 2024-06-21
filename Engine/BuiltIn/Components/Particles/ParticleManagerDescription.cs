
namespace Engine.BuiltIn.Components.Particles
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
            return new();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ParticleManagerDescription()
            : base()
        {
            BlendMode = BlendModes.OpaqueAlpha;
            DeferredEnabled = false;
        }
    }
}
