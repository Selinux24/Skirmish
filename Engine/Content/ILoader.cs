using System;
using System.Collections.Generic;

namespace Engine.Content
{
    /// <summary>
    /// Content loader interface
    /// </summary>
    public interface ILoader
    {
        /// <summary>
        /// Gets the loader delegate
        /// </summary>
        /// <returns>Returns a delegate wich creates a loader</returns>
        Func<ILoader> GetLoaderDelegate();
        /// <summary>
        /// Gets the extensions list which this loader is valid
        /// </summary>
        /// <returns>Returns a extension array list</returns>
        IEnumerable<string> GetExtensions();
        /// <summary>
        /// Loads model content from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Content description</param>
        /// <returns>Returns a list of model contents</returns>
        IEnumerable<ModelContent> Load(string contentFolder, ModelContentDescription content);
    }
}
