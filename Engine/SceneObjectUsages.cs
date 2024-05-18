using System;

namespace Engine
{
    /// <summary>
    /// Scene object usages enumeration
    /// </summary>
    [Flags]
    public enum SceneObjectUsages
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Object
        /// </summary>
        Object = 1,
        /// <summary>
        /// Scene ground
        /// </summary>
        Ground = 2,
        /// <summary>
        /// Scene agent
        /// </summary>
        Agent = 4,
        /// <summary>
        /// User interface
        /// </summary>
        UI = 8,
    }
}
