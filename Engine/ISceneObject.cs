using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Scene object interface
    /// </summary>
    public interface ISceneObject : IDisposable
    {
        /// <summary>
        /// Name
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Processing order
        /// </summary>
        int Order { get; set; }
        /// <summary>
        /// Visible
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Active
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// Gets or sets whether the object cast shadow
        /// </summary>
        bool CastShadow { get; set; }
        /// <summary>
        /// Gets or sets whether the object is enabled to draw with the deferred renderer
        /// </summary>
        bool DeferredEnabled { get; set; }
        /// <summary>
        /// Uses depth info
        /// </summary>
        bool DepthEnabled { get; set; }
        /// <summary>
        /// Enables transparent blending
        /// </summary>
        bool AlphaEnabled { get; set; }
        /// <summary>
        /// Object usage
        /// </summary>
        SceneObjectUsages Usage { get; set; }
        /// <summary>
        /// Maximum instance count
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets or sets if the current object has a parent
        /// </summary>
        bool HasParent { get; }

        /// <summary>
        /// Gets the internal instance as specified type
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns the instance as type</returns>
        T Get<T>();
        /// <summary>
        /// Gets if the instance implements type
        /// </summary>
        /// <typeparam name="T">Result type</typeparam>
        /// <returns>Returns true if the instance implements the type</returns>
        bool Is<T>();

        /// <summary>
        /// Gets the current object triangle collection
        /// </summary>
        /// <param name="usage">Object usage</param>
        /// <returns>Returns the triangle list</returns>
        IEnumerable<Triangle> GetTriangles(SceneObjectUsages usage);
    }
}
