
namespace Engine.Modular
{
    /// <summary>
    /// Object action item
    /// </summary>
    /// <remarks>Designates the action by name to activate in the referenced object by id</remarks>
    public class ModularSceneryObjectActionItem
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
