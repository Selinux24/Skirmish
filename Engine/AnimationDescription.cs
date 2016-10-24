using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Animation description class
    /// </summary>
    [Serializable]
    public class AnimationDescription
    {
        /// <summary>
        /// Clips
        /// </summary>
        [XmlArray("animations")]
        [XmlArrayItem("animation", typeof(AnimationClipDescription))]
        public List<AnimationClipDescription> Clips = new List<AnimationClipDescription>();
        /// <summary>
        /// Transitions
        /// </summary>
        [XmlArray("transitions")]
        [XmlArrayItem("transition", typeof(TransitionDescription))]
        public List<TransitionDescription> Transitions = new List<TransitionDescription>();

        /// <summary>
        /// Adds a clip to clip list
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        public void AddClip(string clipName, int startTime, int endTime)
        {
            this.Clips.Add(new AnimationClipDescription()
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
            this.Transitions.Add(new TransitionDescription()
            {
                ClipFrom = clipFrom,
                ClipTo = clipTo,
                StartFrom = startFrom,
                StartTo = startTo,
            });
        }
    }
}
