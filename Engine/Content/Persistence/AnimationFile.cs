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
    }
}
