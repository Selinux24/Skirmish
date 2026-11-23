using System;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// Animation clip description
    /// </summary>
    public readonly struct AnimationClipFile(string name, int from, int to, string skeleton = null)
    {
        /// <summary>
        /// Clip name
        /// </summary>
        public readonly string Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Must specify a name.", nameof(name));
        /// <summary>
        /// Index from
        /// </summary>
        public readonly int From = from >= 0 ? from : throw new ArgumentException("Must specify the value 'from' Equal or Greater than 0.", nameof(from));
        /// <summary>
        /// Index to
        /// </summary>
        public readonly int To = to >= 0 ? to : throw new ArgumentException("Must specify the value 'to' Equal or Greater than 0.", nameof(to));
        /// <summary>
        /// Skeleton name
        /// </summary>
        public readonly string Skeleton = skeleton;
    }
}
