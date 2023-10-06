
namespace Engine.Modular
{
    using Engine.Modular.Persistence;

    /// <summary>
    /// Modular scenery item
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Object
        /// </summary>
        public ObjectReference Object { get; private set; }
        /// <summary>
        /// Instance
        /// </summary>
        public ModelInstance Instance { get; private set; }
        /// <summary>
        /// Particle emitters
        /// </summary>
        public ParticleEmitter[] Emitters { get; private set; }
        /// <summary>
        /// Current state
        /// </summary>
        public string CurrentState { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="instance">Scene object</param>
        /// <param name="emitters">Particle emitters list</param>
        /// <param name="state">Initial state</param>
        public Item(ObjectReference obj, ModelInstance instance, ParticleEmitter[] emitters, string state)
        {
            Object = obj;
            Instance = instance;
            Emitters = emitters;
            CurrentState = state;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Object?.Name} - {Instance?.Id}. State: {CurrentState}; {Emitters?.Length ?? 0} emitters.";
        }
    }
}
