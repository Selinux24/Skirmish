
namespace Engine
{
    /// <summary>
    /// Modular scenery item
    /// </summary>
    public class ModularSceneryItem
    {
        /// <summary>
        /// Object
        /// </summary>
        public ModularSceneryObjectReference Object { get; private set; }
        /// <summary>
        /// Item
        /// </summary>
        public ModelInstance Item { get; private set; }
        /// <summary>
        /// Particle emitters
        /// </summary>
        public ParticleEmitter[] Emitters { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="item">Scene object</param>
        /// <param name="emitters">Particle emitters list</param>
        public ModularSceneryItem(ModularSceneryObjectReference obj, ModelInstance item, ParticleEmitter[] emitters)
        {
            this.Object = obj;
            this.Item = item;
            this.Emitters = emitters;
        }
    }
}
