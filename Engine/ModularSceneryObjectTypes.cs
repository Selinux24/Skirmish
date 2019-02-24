using System;

namespace Engine
{
    /// <summary>
    /// Modular scenery object type enum
    /// </summary>
    [Flags]
    public enum ModularSceneryObjectTypes
    {
        /// <summary>
        /// Default type
        /// </summary>
        Default = 1,
        /// <summary>
        /// Dungeon entrance
        /// </summary>
        Entrance = 2,
        /// <summary>
        /// Dungeon exit
        /// </summary>
        Exit = 4,
        /// <summary>
        /// Door
        /// </summary>
        Door = 8,
        /// <summary>
        /// Furniture
        /// </summary>
        Furniture = 16,
        /// <summary>
        /// Light
        /// </summary>
        Light = 32,
        /// <summary>
        /// Floor trap
        /// </summary>
        TrapFloor = 64,
        /// <summary>
        /// Ceiling trap
        /// </summary>
        TrapCeiling = 128,
        /// <summary>
        /// Wall trap
        /// </summary>
        TrapWall = 256,
        /// <summary>
        /// Trigger
        /// </summary>
        Trigger = 512,
    }
}
