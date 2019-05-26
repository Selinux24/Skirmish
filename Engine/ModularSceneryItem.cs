using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Modular scenery item
    /// </summary>
    public class ModularSceneryItem
    {
        /// <summary>
        /// Objects action dictionary
        /// </summary>
        private readonly Dictionary<string, List<ModularSceneryAction>> actions = new Dictionary<string, List<ModularSceneryAction>>();

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
        /// <param name="actions">Actions</param>
        public ModularSceneryItem(ModularSceneryObjectReference obj, ModelInstance item, ParticleEmitter[] emitters)
        {
            this.Object = obj;
            this.Item = item;
            this.Emitters = emitters;
        }


        public void ActivateTrigger(string actionName)
        {
            var actionList = actions[actionName];

            foreach (var a in actionList)
            {
                a.Start();
            }
        }
    }
}
