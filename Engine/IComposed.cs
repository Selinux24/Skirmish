using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Component composition interface
    /// </summary>
    /// <typeparam name="T">Type of result component</typeparam>
    public interface IComposed
    {
        /// <summary>
        /// Component count
        /// </summary>
        int InstanceCount { get; }
        /// <summary>
        /// Gets components
        /// </summary>
        /// <returns>Returns a collection of components of the specified type</returns>
        IEnumerable<T> GetComponents<T>();
    }
}
