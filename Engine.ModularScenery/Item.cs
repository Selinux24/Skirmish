
namespace Engine.Modular
{
    using Engine.Modular.Persistence;

    /// <summary>
    /// Modular scenery item
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="obj">Object</param>
    /// <param name="instance">Scene object</param>
    /// <param name="emitters">Particle emitters list</param>
    /// <param name="state">Initial state</param>
    public class Item(ObjectReference obj, ModelInstance instance, ParticleEmitter[] emitters, string state)
    {
        /// <summary>
        /// Object
        /// </summary>
        public ObjectReference Object { get; private set; } = obj;
        /// <summary>
        /// Instance
        /// </summary>
        public ModelInstance Instance { get; private set; } = instance;
        /// <summary>
        /// Particle emitters
        /// </summary>
        public ParticleEmitter[] Emitters { get; private set; } = emitters;
        /// <summary>
        /// Current state
        /// </summary>
        public string CurrentState { get; set; } = state;

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Object?.Name} - {Instance?.Id}. State: {CurrentState}; {Emitters?.Length ?? 0} emitters.";
        }
    }
}
