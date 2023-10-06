
namespace Engine.Modular
{
    /// <summary>
    /// Modular scenery action
    /// </summary>
    public class ItemAction
    {
        /// <summary>
        /// Item Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Animation plan name
        /// </summary>
        public string Action { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id} => {Action}";
        }
    }
}
