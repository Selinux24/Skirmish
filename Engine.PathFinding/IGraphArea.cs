using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area interface
    /// </summary>
    public interface IGraphArea
    {
        /// <summary>
        /// Area id
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets the area type
        /// </summary>
        int GetAreaType();
        /// <summary>
        /// Gets the area type enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        T GetAreaType<T>() where T : Enum;
        /// <summary>
        /// Sets the area type
        /// </summary>
        /// <param name="area">Area value</param>
        void SetAreaType(int area);
        /// <summary>
        /// Gets the area type enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="area">Area value</param>
        void SetAreaType<T>(T area) where T : Enum;

        /// <summary>
        /// Gets the area bounds
        /// </summary>
        BoundingBox GetBounds();
    }
}
