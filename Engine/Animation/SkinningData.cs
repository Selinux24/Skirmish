using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Animation
{
    /// <summary>
    /// Skinning data
    /// </summary>
    public class SkinningData : IEquatable<SkinningData>
    {
        /// <summary>
        /// Default clip name
        /// </summary>
        public const string DefaultClip = "default";
        /// <summary>
        /// Default time step
        /// </summary>
        public const float TimeStep = 1.0f / 60.0f;

        /// <summary>
        /// Animations clip dictionary
        /// </summary>
        private List<AnimationClip> animations = null;
        /// <summary>
        /// Transition between animations list
        /// </summary>
        private List<Transition> transitions = null;
        /// <summary>
        /// Animation clip names collection
        /// </summary>
        private List<string> clips = null;
        /// <summary>
        /// Clip offsets in animation palette
        /// </summary>
        private List<int> offsets = null;
        /// <summary>
        /// Skeleton
        /// </summary>
        private Skeleton skeleton = null;

        /// <summary>
        /// Resource index
        /// </summary>
        public uint ResourceIndex = 0;
        /// <summary>
        /// Resource offset
        /// </summary>
        public uint ResourceOffset = 0;
        /// <summary>
        /// Resource size
        /// </summary>
        public uint ResourceSize = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="jointAnimations">Animation list</param>
        /// <param name="animationDescription">Animation description</param>
        public SkinningData(Skeleton skeleton, JointAnimation[] jointAnimations, AnimationDescription animationDescription)
        {
            this.animations = new List<AnimationClip>();
            this.transitions = new List<Transition>();
            this.clips = new List<string>();
            this.offsets = new List<int>();
            this.skeleton = skeleton;

            if (animationDescription != null)
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

                if (dictAnimations != null)
                {
                    foreach (var key in dictAnimations.Keys)
                    {
                        this.animations.Add(new AnimationClip(key, dictAnimations[key]));
                        this.clips.Add(key);
                    }
                }

                foreach (var transition in animationDescription.Transitions)
                {
                    this.AddTransition(
                        transition.ClipFrom,
                        transition.ClipTo,
                        transition.StartFrom,
                        transition.StartTo);
                }
            }
            else
            {
                this.animations.Add(new AnimationClip(SkinningData.DefaultClip, jointAnimations));
                this.clips.Add(SkinningData.DefaultClip);
            }

            //Initialize offsets for animation process with animation palette
            this.InitializeOffsets();
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
            int indexFrom = this.animations.FindIndex(c => c.Name == clipFrom);
            int indexTo = this.animations.FindIndex(c => c.Name == clipTo);

            float durationFrom = this.GetClipDuration(indexFrom);
            float durationTo = this.GetClipDuration(indexTo);

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

            this.transitions.Add(transition);

            this.clips.Add(clipFrom + clipTo);
        }
        /// <summary>
        /// Initialize animation offsets
        /// </summary>
        private void InitializeOffsets()
        {
            int offset = 0;

            for (int i = 0; i < this.animations.Count; i++)
            {
                this.offsets.Add(offset);

                float duration = this.animations[i].Duration;
                int clipLength = (int)(duration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    var mat = this.GetPoseAtTime(t * TimeStep, i);

                    offset += mat.Length * 4;
                }
            }

            foreach (var transition in this.transitions)
            {
                this.offsets.Add(offset);

                float totalDuration = transition.TotalDuration;
                float interDuration = transition.InterpolationDuration;

                int clipLength = (int)(totalDuration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    float time = (float)t * TimeStep;
                    float factor = Math.Min(time / interDuration, 1f);

                    var mat = this.GetPoseAtTime(
                        time,
                        transition.ClipFrom, transition.ClipTo,
                        transition.StartFrom, transition.StartTo,
                        factor);

                    offset += mat.Length * 4;
                }
            }
        }

        /// <summary>
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip name</param>
        /// <param name="animationOffset">Animation offset</param>
        public void GetAnimationOffset(float time, string clipName, out uint animationOffset)
        {
            int clipIndex = this.GetClipIndex(clipName);
            uint offset = this.GetClipOffset(clipIndex);
            float duration = this.GetClipDuration(clipIndex);
            int clipLength = (int)(duration / TimeStep);

            float percent = time / duration;
            int percentINT = (int)percent;
            percent -= (float)percentINT;
            int index = (int)((float)clipLength * percent);

            animationOffset = offset + (uint)(4 * this.skeleton.JointCount * index) + this.ResourceOffset;
        }
        /// <summary>
        /// Gets the index of the specified clip in the animation collection
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <returns>Returns the index of the clip by name</returns>
        public int GetClipIndex(string clipName)
        {
            return this.clips.IndexOf(clipName);
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
                return (uint)this.offsets[clipIndex];
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
            else if (clipIndex < this.animations.Count)
            {
                return this.animations[clipIndex].Duration;
            }
            else
            {
                return this.transitions[clipIndex - this.animations.Count].TotalDuration;
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
            int clipIndex = this.GetClipIndex(clipName);

            if (clipIndex < 0)
            {
                return Helper.CreateArray<Matrix>(this.skeleton.JointCount, Matrix.Identity);
            }
            else if (clipIndex < this.animations.Count)
            {
                return this.GetPoseAtTime(time, clipIndex);
            }
            else
            {
                var transition = this.transitions[clipIndex - this.animations.Count];

                float factor = Math.Min(time / transition.InterpolationDuration, 1f);

                return this.GetPoseAtTime(time, transition.ClipFrom, transition.ClipTo, transition.StartFrom, transition.StartTo, factor);
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
            var res = new Matrix[this.skeleton.JointCount];

            if (clipIndex >= 0)
            {
                this.skeleton.GetPoseAtTime(time, this.animations[clipIndex].Animations, ref res);
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
            return this.GetPoseAtTime(time, this.GetClipIndex(clipName1), this.GetClipIndex(clipName2), offset1, offset2, factor);
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
            var res = new Matrix[this.skeleton.JointCount];

            if (clipIndex1 >= 0 && clipIndex2 >= 0)
            {
                this.skeleton.GetPoseAtTime(
                    time + offset1, this.animations[clipIndex1].Animations,
                    time + offset2, this.animations[clipIndex2].Animations,
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

            for (int i = 0; i < this.animations.Count; i++)
            {
                float duration = this.animations[i].Duration;
                int clipLength = (int)(duration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    var mat = this.GetPoseAtTime(t * TimeStep, i);

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

            foreach (var transition in this.transitions)
            {
                float totalDuration = transition.TotalDuration;
                float interDuration = transition.InterpolationDuration;

                int clipLength = (int)(totalDuration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    float time = (float)t * TimeStep;
                    float factor = Math.Min(time / interDuration, 1f);

                    var mat = this.GetPoseAtTime(
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
                this.animations.ListIsEqual(other.animations) &&
                this.transitions.ListIsEqual(other.transitions) &&
                this.clips.ListIsEqual(other.clips) &&
                this.offsets.ListIsEqual(other.offsets) &&
                this.skeleton.Equals(other.skeleton);
        }
    }
}
