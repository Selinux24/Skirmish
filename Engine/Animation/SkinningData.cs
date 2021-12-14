using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Skinning data
    /// </summary>
    public sealed class SkinningData : IEquatable<SkinningData>, ISkinningData
    {
        /// <summary>
        /// Default clip name
        /// </summary>
        public const string DefaultClip = "default";
        /// <summary>
        /// Default time step
        /// </summary>
        /// <remarks>4 times per fixed frame 1/60f/4f</remarks>
        public const float FixedTimeStep = 1.0f / 60.0f / 4f;

        /// <summary>
        /// Animations clip dictionary
        /// </summary>
        private readonly List<AnimationClip> animations = new List<AnimationClip>();
        /// <summary>
        /// Transition between animations list
        /// </summary>
        private readonly List<Transition> transitions = new List<Transition>();
        /// <summary>
        /// Animation clip names collection
        /// </summary>
        private readonly List<string> clips = new List<string>();
        /// <summary>
        /// Clip offsets in animation palette
        /// </summary>
        private readonly List<int> offsets = new List<int>();
        /// <summary>
        /// Skeleton
        /// </summary>
        private readonly Skeleton skeleton = null;

        /// <inheritdoc/>
        public float TimeStep { get; private set; } = FixedTimeStep;
        /// <inheritdoc/>
        public uint ResourceIndex { get; set; } = 0;
        /// <inheritdoc/>
        public uint ResourceOffset { get; set; } = 0;
        /// <inheritdoc/>
        public uint ResourceSize { get; set; } = 0;
        /// <inheritdoc/>
        public event EventHandler OnResourcesUpdated;

        /// <summary>
        /// Initializes the animation dictionary
        /// </summary>
        /// <param name="jointAnimations">Joint list</param>
        /// <param name="animationDescription">Animation description</param>
        private static Dictionary<string, IEnumerable<JointAnimation>> InitializeAnimationDictionary(IEnumerable<JointAnimation> jointAnimations, AnimationFile animationDescription)
        {
            Dictionary<string, IEnumerable<JointAnimation>> dictAnimations = new Dictionary<string, IEnumerable<JointAnimation>>();

            foreach (var clip in animationDescription.Clips)
            {
                List<JointAnimation> ja = new List<JointAnimation>(jointAnimations.Count());

                foreach (var j in jointAnimations)
                {
                    ja.Add(j.Copy(clip.From, clip.To));
                }

                dictAnimations.Add(clip.Name, ja);
            }

            return dictAnimations;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        public SkinningData(Skeleton skeleton)
        {
            this.skeleton = skeleton;
        }

        /// <inheritdoc/>
        public void Initialize(IEnumerable<JointAnimation> jointAnimations, AnimationFile animationDescription)
        {
            if (animationDescription != null)
            {
                TimeStep = animationDescription.TimeStep > 0 ? animationDescription.TimeStep : FixedTimeStep;

                var dictAnimations = InitializeAnimationDictionary(jointAnimations, animationDescription);

                foreach (var key in dictAnimations.Keys)
                {
                    animations.Add(new AnimationClip(key, dictAnimations[key]));
                    clips.Add(key);
                }

                foreach (var transition in animationDescription.Transitions)
                {
                    AddTransition(
                        transition.ClipFrom,
                        transition.ClipTo,
                        transition.StartFrom,
                        transition.StartTo);
                }
            }
            else
            {
                animations.Add(new AnimationClip(DefaultClip, jointAnimations));
                clips.Add(DefaultClip);
            }

            //Initialize offsets for animation process with animation palette
            InitializeOffsets();
        }

        /// <summary>
        /// Adds a transition between two clips to the internal collection
        /// </summary>
        /// <param name="clipFrom">Clip from</param>
        /// <param name="clipTo">Clip to</param>
        /// <param name="startTimeFrom">Starting time in clipFrom to begin to interpolate</param>
        /// <param name="startTimeTo">Starting time in clipTo to begin to interpolate</param>
        private void AddTransition(string clipFrom, string clipTo, float startTimeFrom, float startTimeTo)
        {
            int indexFrom = animations.FindIndex(c => c.Name == clipFrom);
            int indexTo = animations.FindIndex(c => c.Name == clipTo);

            float durationFrom = GetClipDuration(indexFrom);
            float durationTo = GetClipDuration(indexTo);

            float total = 0;
            float inter = 0;
            if (durationFrom == durationTo)
            {
                total = inter = durationFrom;
            }
            else if (durationFrom > durationTo)
            {
                total = inter = durationTo;
            }
            else
            {
                inter = durationFrom;
                total = durationTo;
            }

            var transition = new Transition(
                indexFrom,
                indexTo,
                startTimeFrom,
                startTimeTo,
                total,
                inter);

            transitions.Add(transition);

            clips.Add(clipFrom + clipTo);
        }
        /// <summary>
        /// Initialize animation offsets
        /// </summary>
        private void InitializeOffsets()
        {
            int offset = 0;

            for (int i = 0; i < animations.Count; i++)
            {
                offsets.Add(offset);

                float duration = animations[i].Duration;
                int clipLength = (int)(duration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    var mat = GetPoseAtTime(t * TimeStep, i);

                    offset += mat.Count() * 4;
                }
            }

            foreach (var transition in transitions)
            {
                offsets.Add(offset);

                float totalDuration = transition.TotalDuration;
                float interDuration = transition.InterpolationDuration;

                int clipLength = (int)(totalDuration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    float time = t * TimeStep;
                    float factor = Math.Min(time / interDuration, 1f);

                    var mat = GetPoseAtTime(
                        time,
                        transition.ClipFrom, transition.ClipTo,
                        transition.StartFrom, transition.StartTo,
                        factor);

                    offset += mat.Count() * 4;
                }
            }
        }

        /// <inheritdoc/>
        public void UpdateResource(uint index, uint offset, uint size)
        {
            ResourceIndex = index;
            ResourceOffset = offset;
            ResourceSize = size;

            OnResourcesUpdated?.Invoke(this, new EventArgs());
        }

        /// <inheritdoc/>
        public void GetAnimationOffset(float time, string clipName, out uint animationOffset)
        {
            int clipIndex = GetClipIndex(clipName);
            uint offset = GetClipOffset(clipIndex);
            float duration = GetClipDuration(clipIndex);
            int clipLength = (int)(duration / TimeStep);

            float percent = time / duration;
            int percentINT = (int)percent;
            percent -= percentINT;
            int index = (int)(clipLength * percent);

            animationOffset = offset + (uint)(4 * skeleton.JointCount * index) + ResourceOffset;
        }
        /// <inheritdoc/>
        public int GetClipIndex(string clipName)
        {
            return clips.IndexOf(clipName);
        }
        /// <inheritdoc/>
        public uint GetClipOffset(int clipIndex)
        {
            if (clipIndex >= 0)
            {
                return (uint)offsets[clipIndex];
            }

            return 0;
        }
        /// <inheritdoc/>
        public float GetClipDuration(int clipIndex)
        {
            if (clipIndex < 0)
            {
                return 0;
            }
            else if (clipIndex < animations.Count)
            {
                return animations[clipIndex].Duration;
            }
            else
            {
                return transitions[clipIndex - animations.Count].TotalDuration;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Matrix> GetPoseBase()
        {
            if (animations.Any())
            {
                return GetPoseAtTime(0, 0);
            }
            else
            {
                return Helper.CreateArray(skeleton.JointCount, Matrix.Identity);
            }
        }
        /// <inheritdoc/>
        public IEnumerable<Matrix> GetPoseAtTime(float time, string clipName)
        {
            int clipIndex = GetClipIndex(clipName);

            if (clipIndex < 0)
            {
                return GetPoseBase();
            }
            else if (clipIndex < animations.Count)
            {
                return GetPoseAtTime(time, clipIndex);
            }
            else
            {
                var transition = transitions[clipIndex - animations.Count];

                float factor = Math.Min(time / transition.InterpolationDuration, 1f);

                return GetPoseAtTime(time, transition.ClipFrom, transition.ClipTo, transition.StartFrom, transition.StartTo, factor);
            }
        }
        /// <inheritdoc/>
        public IEnumerable<Matrix> GetPoseAtTime(float time, int clipIndex)
        {
            var res = new Matrix[skeleton.JointCount];

            if (clipIndex >= 0)
            {
                skeleton.GetPoseAtTime(time, animations[clipIndex].Animations, ref res);
            }

            return res;
        }
        /// <inheritdoc/>
        public IEnumerable<Matrix> GetPoseAtTime(float time, string clipName1, string clipName2, float offset1, float offset2, float factor)
        {
            return GetPoseAtTime(time, GetClipIndex(clipName1), GetClipIndex(clipName2), offset1, offset2, factor);
        }
        /// <inheritdoc/>
        public IEnumerable<Matrix> GetPoseAtTime(float time, int clipIndex1, int clipIndex2, float offset1, float offset2, float factor)
        {
            var res = new Matrix[skeleton.JointCount];

            if (clipIndex1 >= 0 && clipIndex2 >= 0)
            {
                skeleton.GetPoseAtTime(
                    time + offset1, animations.ElementAt(clipIndex1).Animations,
                    time + offset2, animations.ElementAt(clipIndex2).Animations,
                    factor,
                    ref res);
            }

            return res;
        }

        /// <inheritdoc/>
        public IEnumerable<Vector4> Pack()
        {
            List<Vector4> values = new List<Vector4>();

            for (int i = 0; i < animations.Count; i++)
            {
                float duration = animations[i].Duration;
                int clipLength = (int)(duration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    var mat = GetPoseAtTime(t * TimeStep, i);

                    for (int m = 0; m < mat.Count(); m++)
                    {
                        var matr = mat.ElementAt(m);

                        values.Add(new Vector4(matr.Row1.X, matr.Row1.Y, matr.Row1.Z, matr.Row4.X));
                        values.Add(new Vector4(matr.Row2.X, matr.Row2.Y, matr.Row2.Z, matr.Row4.Y));
                        values.Add(new Vector4(matr.Row3.X, matr.Row3.Y, matr.Row3.Z, matr.Row4.Z));
                        values.Add(new Vector4(0, 0, 0, 0));
                    }
                }
            }

            foreach (var transition in transitions)
            {
                float totalDuration = transition.TotalDuration;
                float interDuration = transition.InterpolationDuration;

                int clipLength = (int)(totalDuration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    float time = (float)t * TimeStep;
                    float factor = Math.Min(time / interDuration, 1f);

                    var mat = GetPoseAtTime(
                        time,
                        transition.ClipFrom, transition.ClipTo,
                        transition.StartFrom, transition.StartTo,
                        factor);

                    for (int m = 0; m < mat.Count(); m++)
                    {
                        var matr = mat.ElementAt(m);

                        values.Add(new Vector4(matr.Row1.X, matr.Row1.Y, matr.Row1.Z, matr.Row4.X));
                        values.Add(new Vector4(matr.Row2.X, matr.Row2.Y, matr.Row2.Z, matr.Row4.Y));
                        values.Add(new Vector4(matr.Row3.X, matr.Row3.Y, matr.Row3.Z, matr.Row4.Z));
                        values.Add(new Vector4(0, 0, 0, 0));
                    }
                }
            }

            return values.ToArray();
        }

        /// <summary>
        /// Gets whether the current instance is equal to the other instance
        /// </summary>
        /// <param name="other">The other instance</param>
        /// <returns>Returns true if both instances are equal</returns>
        public bool Equals(SkinningData other)
        {
            return
                animations.ListIsEqual(other.animations) &&
                transitions.ListIsEqual(other.transitions) &&
                clips.ListIsEqual(other.clips) &&
                offsets.ListIsEqual(other.offsets) &&
                skeleton.Equals(other.skeleton);
        }
    }
}
