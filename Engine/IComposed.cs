using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Ray pickable component composition interface
    /// </summary>
    /// <typeparam name="T">Type of pickable result</typeparam>
    public interface IComposed<T> where T : IRayIntersectable
    {
        /// <summary>
        /// Gets all ray pickable components
        /// </summary>
        /// <returns>Returns a collection of ray pickable components</returns>
        IEnumerable<IRayPickable<T>> GetComponents();
    }
}
