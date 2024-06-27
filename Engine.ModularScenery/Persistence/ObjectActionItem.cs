
namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Object action item
    /// </summary>
    /// <remarks>Designates the action by name to activate in the referenced object by id</remarks>
    public class ObjectActionItem
    {
        /// <summary>
        /// Object Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Action name
        /// </summary>
        public string Action { get; set; }
    }
}
