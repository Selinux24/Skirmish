using System;
using System.Collections.Generic;

namespace Engine.Content
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Animation content loader interface
    /// </summary>
    public interface IAnimationLoader
    {
        /// <summary>
        /// Gets the loader delegate
        /// </summary>
        /// <returns>Returns a delegate wich creates a loader</returns>
        Func<IAnimationLoader> GetLoaderDelegate();
        /// <summary>
        /// Gets the extensions list which this loader is valid
        /// </summary>
        /// <returns>Returns a extension array list</returns>
        IEnumerable<string> GetExtensions();
        /// <summary>
        /// Loads animation from a collada file
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Conten description</param>
        /// <returns>Returns the loaded contents</returns>
        AnimationLibContentData Load(string contentFolder, AnimationLibContentDataFile content);
    }
}
