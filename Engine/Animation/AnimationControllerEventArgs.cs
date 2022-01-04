using System;

namespace Engine.Animation
{
    /// <summary>
    /// Animation controller event arguments
    /// </summary>
    public class AnimationControllerEventArgs : EventArgs
    {
        /// <summary>
        /// Animation is at the end
        /// </summary>
        public bool AtEnd { get; set; }
        /// <summary>
        /// Gets whether then plan is transition or not
        /// </summary>
        public bool IsTransition { get; set; }

        /// <summary>
        /// Current animation offset in the animation palette
        /// </summary>
        public uint CurrentOffset { get; set; }
        /// <summary>
        /// Current path index in the animation path collection
        /// </summary>
        public int CurrentIndex { get; set; }
        /// <summary>
        /// Current animation path
        /// </summary>
        public AnimationPath CurrentPath { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationControllerEventArgs() : base()
        {

        }
    }
}
