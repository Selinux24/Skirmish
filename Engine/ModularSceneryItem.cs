
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
        public ModularSceneryObjectReference Object { get; set; }
        /// <summary>
        /// Item
        /// </summary>
        public ModelInstance Item { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="item">Scene object</param>
        public ModularSceneryItem(ModularSceneryObjectReference obj, ModelInstance item)
        {
            this.Object = obj;
            this.Item = item;
        }
    }
}
