using System;

namespace Engine.Animation
{
    /// <summary>
    /// Animation controller event arguments
    /// </summary>
    public class AnimationControllerEventArgs : EventArgs
    {
        /// <summary>
        /// Animation time
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Animation is at the end
        /// </summary>
        public bool AtEnd { get; set; }

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
        /// Previous animation offset in the animation palette
        /// </summary>
        public uint PreviousOffset { get; set; }
        /// <summary>
        /// Previous path index in the animation path collection
        /// </summary>
        public int PreviousIndex { get; set; }
        /// <summary>
        /// Previous animation path
        /// </summary>
        public AnimationPath PreviousPath { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AnimationControllerEventArgs() : base()
        {

        }
    }
}
