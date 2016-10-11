using System;
using System.Collections.Generic;

namespace Engine.Animation
{
    /// <summary>
    /// Animation description class
    /// </summary>
    public class AnimationDescription
    {
        /// <summary>
        /// Clips
        /// </summary>
        public Dictionary<string, Tuple<int, int>> Clips = new Dictionary<string, Tuple<int, int>>();
        /// <summary>
        /// Transitions
        /// </summary>
        public List<TransitionDescription> Transitions = new List<TransitionDescription>();

        /// <summary>
        /// Adds a clip to clip list
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        public void AddClip(string clipName, int startTime, int endTime)
        {
            this.Clips.Add(clipName, new Tuple<int, int>(startTime, endTime));
        }
        /// <summary>
        /// Adds a transition between two clips into de transition list
        /// </summary>
        /// <param name="clipFrom">Clip from name</param>
        /// <param name="clipTo">Clip to name</param>
        /// <param name="duration">Total duration of the transition</param>
        /// <param name="startFrom">Start time of the "from" clip</param>
        /// <param name="startTo">Start time of the "to" clip</param>
        public void AddTransition(string clipFrom, string clipTo, float startFrom, float startTo, float duration)
        {
            this.Transitions.Add(new TransitionDescription()
            {
                ClipFrom = clipFrom,
                ClipTo = clipTo,
                StartFrom = startFrom,
                StartTo = startTo,
                Duration = duration,
            });
        }
    }
}
