
namespace Engine.Content.Persistence
{
    /// <summary>
    /// Animation clip description
    /// </summary>
    public class AnimationClipFile
    {
        /// <summary>
        /// Clip name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Index from
        /// </summary>
        public int From { get; set; }
        /// <summary>
        /// Index to
        /// </summary>
        public int To { get; set; }
        /// <summary>
        /// Skeleton name
        /// </summary>
        public string Skeleton { get; set; }
    }
}
