using System.Collections.Generic;

namespace Engine.Content
{
    /// <summary>
    /// Content loader interface
    /// </summary>
    public interface ILoader
    {
        /// <summary>
        /// Loads model content from resources
        /// </summary>
        /// <param name="contentFolder">Content folder</param>
        /// <param name="content">Content description</param>
        /// <returns>Returns a list of model contents</returns>
        IEnumerable<ModelContent> Load(string contentFolder, ModelContentDescription content);
    }
}
