﻿
namespace Engine.Common
{
    /// <summary>
    /// Animation draw information
    /// </summary>
    public struct AnimationDrawInfo
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly AnimationDrawInfo Empty = new AnimationDrawInfo();

        /// <summary>
        /// First offset in the animation palette
        /// </summary>
        public uint Offset1 { get; set; }
        /// <summary>
        /// Second offset in the animation palette
        /// </summary>
        public uint Offset2 { get; set; }
        /// <summary>
        /// Interpolation amount between the offsets
        /// </summary>
        public float InterpolationAmount { get; set; }
    }
}
