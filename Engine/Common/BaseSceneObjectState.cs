using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Engine.Common
{
    /// <summary>
    /// Base scene object state
    /// </summary>
    public abstract class BaseSceneObjectState : ISceneObjectState
    {
        /// <summary>
        /// Object id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Object name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Usage
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SceneObjectUsages Usage { get; set; }
        /// <summary>
        /// Layer
        /// </summary>
        public int Layer { get; set; }
        /// <summary>
        /// Owner id
        /// </summary>
        public string OwnerId { get; set; }
    }
}
