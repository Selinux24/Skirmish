using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Animation
{
    /// <summary>
    /// Skinning data
    /// </summary>
    public sealed class SkinningData : IEquatable<SkinningData>
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

        /// <summary>
        /// Time step
        /// </summary>
        public float TimeStep { get; private set; } = FixedTimeStep;
        /// <summary>
        /// Resource index
        /// </summary>
        public uint ResourceIndex { get; set; } = 0;
        /// <summary>
        /// Resource offset
        /// </summary>
        public uint ResourceOffset { get; set; } = 0;
        /// <summary>
        /// Resource size
        /// </summary>
        public uint ResourceSize { get; set; } = 0;
        /// <summary>
        /// On resources updated event
        /// </summary>
        public EventHandler OnResourcesUpdated;

        /// <summary>
        /// Initializes the animation dictionary
        /// </summary>
        /// <param name="jointAnimations">Joint list</param>
        /// <param name="animationDescription">Animation description</param>
        private static Dictionary<string, JointAnimation[]> InitializeAnimationDictionary(JointAnimation[] jointAnimations, AnimationDescription animationDescription)
        {
            Dictionary<string, JointAnimation[]> dictAnimations = new Dictionary<string, JointAnimation[]>();

            foreach (var clip in animationDescription.Clips)
            {
                JointAnimation[] ja = new JointAnimation[jointAnimations.Length];
                for (int c = 0; c < ja.Length; c++)
                {
                    Keyframe[] kfs = new Keyframe[clip.To - clip.From + 1];
                    Array.Copy(jointAnimations[c].Keyframes, clip.From, kfs, 0, kfs.Length);

                    float dTime = kfs[0].Time;
                    for (int k = 0; k < kfs.Length; k++)
                    {
                        kfs[k].Time -= dTime;
                    }

                    ja[c] = new JointAnimation(jointAnimations[c].Joint, kfs);
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

        /// <summary>
        /// Initialize the drawing data instance
        /// </summary>
        /// <param name="jointAnimations">Joint animation list</param>
        /// <param name="animationDescription">Animation description</param>
        public void Initialize(JointAnimation[] jointAnimations, AnimationDescription animationDescription)
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

                    offset += mat.Length * 4;
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

                    offset += mat.Length * 4;
                }
            }
        }

        /// <summary>
        /// Updates the resource data
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="offset">Offset</param>
        /// <param name="size">Size</param>
        public void UpdateResource(uint index, uint offset, uint size)
        {
            ResourceIndex = index;
            ResourceOffset = offset;
            ResourceSize = size;

            OnResourcesUpdated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip name</param>
        /// <param name="animationOffset">Animation offset</param>
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
        /// <summary>
        /// Gets the index of the specified clip in the animation collection
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <returns>Returns the index of the clip by name</returns>
        public int GetClipIndex(string clipName)
        {
            return clips.IndexOf(clipName);
        }
        /// <summary>
        /// Gets the clip offset in animation palette
        /// </summary>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the clip offset in animation palette</returns>
        public uint GetClipOffset(int clipIndex)
        {
            if (clipIndex >= 0)
            {
                return (uint)offsets[clipIndex];
            }

            return 0;
        }
        /// <summary>
        /// Gets the duration of the specified by index clip
        /// </summary>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the duration of the clip</returns>
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

        /// <summary>
        /// Gets the base pose transformation list
        /// </summary>
        /// <returns>Returns the base transformation list</returns>
        public Matrix[] GetPoseBase()
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
        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip mame</param>
        /// <returns>Returns the resulting transform list</returns>
        public Matrix[] GetPoseAtTime(float time, string clipName)
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
        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the resulting transform list</returns>
        public Matrix[] GetPoseAtTime(float time, int clipIndex)
        {
            var res = new Matrix[skeleton.JointCount];

            if (clipIndex >= 0)
            {
                skeleton.GetPoseAtTime(time, animations[clipIndex].Animations, ref res);
            }

            return res;
        }
        /// <summary>
        /// Gets the transform list of the pose's combination at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName1">First clip name</param>
        /// <param name="clipName2">Second clip name</param>
        /// <param name="offset1">Time offset for first clip</param>
        /// <param name="offset2">Time offset from second clip</param>
        /// <param name="factor">Interpolation factor</param>
        /// <returns>Returns the resulting transform list</returns>
        public Matrix[] GetPoseAtTime(float time, string clipName1, string clipName2, float offset1, float offset2, float factor)
        {
            return GetPoseAtTime(time, GetClipIndex(clipName1), GetClipIndex(clipName2), offset1, offset2, factor);
        }
        /// <summary>
        /// Gets the transform list of the pose's combination at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex1">First clip index</param>
        /// <param name="clipIndex2">Second clip index</param>
        /// <param name="offset1">Time offset for first clip</param>
        /// <param name="offset2">Time offset from second clip</param>
        /// <param name="factor">Interpolation factor</param>
        /// <returns>Returns the resulting transform list</returns>
        public Matrix[] GetPoseAtTime(float time, int clipIndex1, int clipIndex2, float offset1, float offset2, float factor)
        {
            var res = new Matrix[skeleton.JointCount];

            if (clipIndex1 >= 0 && clipIndex2 >= 0)
            {
                skeleton.GetPoseAtTime(
                    time + offset1, animations[clipIndex1].Animations,
                    time + offset2, animations[clipIndex2].Animations,
                    factor,
                    ref res);
            }

            return res;
        }

        /// <summary>
        /// Packs current instance into a Vector4 array
        /// </summary>
        /// <returns>Returns the packed skinning data</returns>
        /// <remarks>This method must stay synchronized with InitializeOffsets</remarks>
        public Vector4[] Pack()
        {
            List<Vector4> values = new List<Vector4>();

            for (int i = 0; i < animations.Count; i++)
            {
                float duration = animations[i].Duration;
                int clipLength = (int)(duration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    var mat = GetPoseAtTime(t * TimeStep, i);

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Matrix matr = mat[m];

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

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Matrix matr = mat[m];

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
