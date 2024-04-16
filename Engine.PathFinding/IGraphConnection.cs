using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph connection interface
    /// </summary>
    public interface IGraphConnection
    {
        /// <summary>
        /// Connection Id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Start point
        /// </summary>
        Vector3 Start { get; set; }
        /// <summary>
        /// End point
        /// </summary>
        Vector3 End { get; set; }
        /// <summary>
        /// Points radius
        /// </summary>
        float Radius { get; set; }
        /// <summary>
        /// Connection direction
        /// </summary>
        int Direction { get; set; }

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
        /// Gets the flag type
        /// </summary>
        int GetFlagType();
        /// <summary>
        /// Gets the flag type enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        T GetFlagType<T>() where T : Enum;
        /// <summary>
        /// Sets the flag type
        /// </summary>
        /// <param name="flag">Flag value</param>
        void SetFlagType(int flag);
        /// <summary>
        /// Gets the flag type enum
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="flag">Flag value</param>
        void SetFlagType<T>(T flag) where T : Enum;
    }
}
