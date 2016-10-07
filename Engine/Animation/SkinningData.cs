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
        public const float TimeStep = 1.0f / 30.0f;

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
        private string[] clips = null;
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
            this.skeleton = skeleton;

            this.animations.Add(new AnimationClip(SkinningData.DefaultClip, animations));
            this.clips = new string[] { SkinningData.DefaultClip };
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
            this.skeleton = skeleton;

            foreach (var key in animations.Keys)
            {
                this.animations.Add(new AnimationClip(key, animations[key]));
            }
            this.clips = animations.Keys.ToArray();
        }

        /// <summary>
        /// Adds a transition between two clips to the internal collection
        /// </summary>
        /// <param name="clipFrom">Clip from</param>
        /// <param name="clipTo">Clip to</param>
        /// <param name="duration">Transition duration</param>
        /// <param name="startTimeFrom">Starting time in clipFrom to begin to interpolate</param>
        /// <param name="startTimeTo">Starting time in clipTo to begin to interpolate</param>
        public void AddTransition(string clipFrom, string clipTo, float duration, float startTimeFrom, float startTimeTo)
        {
            var transition = new Transition(
                this.GetClip(clipFrom),
                this.GetClip(clipTo),
                duration,
                startTimeFrom,
                startTimeTo);

            this.transitions.Add(transition);
        }

        /// <summary>
        /// Gets the index of the specified clip in the animation collection
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <returns>Returns the index of the clip by name</returns>
        public int GetClipIndex(string clipName)
        {
            return Array.IndexOf(this.clips, clipName);
        }
        /// <summary>
        /// Gets the clip by name
        /// </summary>
        /// <param name="clipName">Clip name</param>
        /// <returns>Returns the clip by name</returns>
        public AnimationClip GetClip(string clipName)
        {
            return this.animations.Find(c => c.Name == clipName);
        }
        /// <summary>
        /// Gets the clip by index
        /// </summary>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the clip by index</returns>
        public AnimationClip GetClip(int clipIndex)
        {
            return this.animations[clipIndex];
        }
        /// <summary>
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex">Clip index</param>
        /// <param name="animationOffset">Animation offset</param>
        public void GetAnimationOffset(float time, int clipIndex, out int animationOffset)
        {
            float duration = this.animations[clipIndex].Duration;
            int clipLength = (int)(duration / TimeStep);

            float percent = time / duration;
            int percentINT = (int)percent;
            percent -= (float)percentINT;
            int index = (int)((float)clipLength * percent);

            animationOffset = 4 * this.skeleton.JointCount * index;
        }

        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="index">Clip index</param>
        public Matrix[] GetPoseAtTime(float time, int index)
        {
            return this.skeleton.GetPoseAtTime(time, this.animations[index].Animations);
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
                float duration = this.animations[i].Duration;

                for (float t = 0; t < duration; t += TimeStep)
                {
                    var mat = this.GetPoseAtTime(t, i);

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
                float duration = transition.Duration;

                for (float t = 0; t < duration; t += TimeStep)
                {
                    var mat = transition.Interpolate(t);

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
