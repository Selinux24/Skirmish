using System;

namespace Engine.Common
{
    /// <summary>
    /// Drawer modes
    /// </summary>
	[Flags]
    public enum DrawerModesEnum
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Forward rendering (default)
        /// </summary>
        Forward = 1,
        /// <summary>
        /// Deferred rendering
        /// </summary>
        Deferred = 2,
        /// <summary>
        /// Shadow map
        /// </summary>
        ShadowMap = 4,
        /// <summary>
        /// 
        /// </summary>
        OpaqueOnly = 8,
        /// <summary>
        /// 
        /// </summary>
        TransparentOnly = 16,
    }
}
