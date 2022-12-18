
namespace Engine.Content.Persistence
{
    /// <summary>
    /// Transition description
    /// </summary>
    public class TransitionFile
    {
        /// <summary>
        /// Clip from name
        /// </summary>
        public string ClipFrom { get; set; }
        /// <summary>
        /// Clip to name
        /// </summary>
        public string ClipTo { get; set; }
        /// <summary>
        /// Clip from start
        /// </summary>
        public float StartFrom { get; set; }
        /// <summary>
        /// Clip to start
        /// </summary>
        public float StartTo { get; set; }
    }
}
