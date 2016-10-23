using SharpDX;
using System;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Animation
{
    /// <summary>
    /// Skinning data
    /// </summary>
    public class SkinningData
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
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="animations">Animation list</param>
        public SkinningData(Skeleton skeleton, JointAnimation[] animations)
        {
            this.animations = new List<AnimationClip>();
            this.transitions = new List<Transition>();
            this.clips = new List<string>();
            this.offsets = new List<int>();
            this.skeleton = skeleton;

            this.animations.Add(new AnimationClip(SkinningData.DefaultClip, animations));
            this.clips.Add(SkinningData.DefaultClip);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="animations">Animation dictionary</param>
        public SkinningData(Skeleton skeleton, Dictionary<string, JointAnimation[]> animations)
        {
            this.animations = new List<AnimationClip>();
            this.transitions = new List<Transition>();
            this.clips = new List<string>();
            this.offsets = new List<int>();
            this.skeleton = skeleton;

            foreach (var key in animations.Keys)
            {
                this.animations.Add(new AnimationClip(key, animations[key]));
                this.clips.Add(key);
            }
        }

        /// <summary>
        /// Adds a transition between two clips to the internal collection
        /// </summary>
        /// <param name="clipFrom">Clip from</param>
        /// <param name="clipTo">Clip to</param>
        /// <param name="startTimeFrom">Starting time in clipFrom to begin to interpolate</param>
        /// <param name="startTimeTo">Starting time in clipTo to begin to interpolate</param>
        public void AddTransition(string clipFrom, string clipTo, float startTimeFrom, float startTimeTo)
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
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip name</param>
        /// <param name="animationOffset">Animation offset</param>
        public void GetAnimationOffset(float time, string clipName, out int animationOffset)
        {
            int clipIndex = this.GetClipIndex(clipName);
            int offset = this.GetClipOffset(clipIndex);
            float duration = this.GetClipDuration(clipIndex);
            int clipLength = (int)(duration / TimeStep);

            float percent = time / duration;
            int percentINT = (int)percent;
            percent -= (float)percentINT;
            int index = (int)((float)clipLength * percent);

            animationOffset = offset + (4 * this.skeleton.JointCount * index);
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
        public int GetClipOffset(int clipIndex)
        {
            if (clipIndex >= 0)
            {
                return this.offsets[clipIndex];
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
                return new Matrix[this.skeleton.JointCount];
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
        /// Creates the animation palette
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="palette">Gets the generated palette</param>
        /// <param name="width">Gets the palette width</param>
        public void CreateAnimationTexture(Game game, out ShaderResourceView palette, out uint width)
        {
            List<Vector4> values = new List<Vector4>();

            for (int i = 0; i < this.animations.Count; i++)
            {
                this.offsets.Add(values.Count);

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
                this.offsets.Add(values.Count);

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

            int pixelCount = values.Count;
            int texWidth = (int)Math.Sqrt((float)pixelCount) + 1;
            int texHeight = 1;
            while (texHeight < texWidth)
            {
                texHeight = texHeight << 1;
            }
            texWidth = texHeight;

            palette = game.ResourceManager.CreateTexture2D(Guid.NewGuid(), values.ToArray(), texWidth);
            width = (uint)texWidth;
        }
    }
}
