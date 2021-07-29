using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Engine.Content.Persistence
{
    /// <summary>
    /// Animation description class
    /// </summary>
    public class AnimationFile
    {
        /// <summary>
        /// Clips
        /// </summary>
        public List<AnimationClipFile> Clips { get; set; } = new List<AnimationClipFile>();
        /// <summary>
        /// Transitions
        /// </summary>
        public List<TransitionFile> Transitions { get; set; } = new List<TransitionFile>();
        /// <summary>
        /// Time step
        /// </summary>
        public float TimeStep { get; set; }

        /// <summary>
        /// Adds a clip to clip list
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        public void AddClip(string clipName, int startTime, int endTime)
        {
            Clips.Add(new AnimationClipFile()
            {
                Name = clipName,
                From = startTime,
                To = endTime,
            });
        }
        /// <summary>
        /// Adds a transition between two clips into de transition list
        /// </summary>
        /// <param name="clipFrom">Clip from name</param>
        /// <param name="clipTo">Clip to name</param>
        /// <param name="startFrom">Start time of the "from" clip</param>
        /// <param name="startTo">Start time of the "to" clip</param>
        public void AddTransition(string clipFrom, string clipTo, float startFrom, float startTo)
        {
            Transitions.Add(new TransitionFile()
            {
                ClipFrom = clipFrom,
                ClipTo = clipTo,
                StartFrom = startFrom,
                StartTo = startTo,
            });
        }
    }
}
