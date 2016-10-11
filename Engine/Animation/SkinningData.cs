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
        public const float TimeStep = 1.0f / 24.0f;

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
        /// <param name="duration">Transition duration</param>
        /// <param name="startTimeFrom">Starting time in clipFrom to begin to interpolate</param>
        /// <param name="startTimeTo">Starting time in clipTo to begin to interpolate</param>
        public void AddTransition(string clipFrom, string clipTo, float startTimeFrom, float startTimeTo, float duration)
        {
            var transition = new Transition(
                this.animations.FindIndex(c => c.Name == clipFrom),
                this.animations.FindIndex(c => c.Name == clipTo),
                startTimeFrom,
                startTimeTo,
                duration);

            this.transitions.Add(transition);

            this.clips.Add(clipFrom + clipTo);
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
        /// Gets the duration of the specified by index clip
        /// </summary>
        /// <param name="clipIndex">Clip index</param>
        /// <returns>Returns the duration of the clip</returns>
        public float GetClipDuration(int clipIndex)
        {
            if (clipIndex < this.animations.Count)
            {
                return this.animations[clipIndex].Duration;
            }
            else
            {
                return this.transitions[clipIndex - this.animations.Count].Duration;
            }
        }
        /// <summary>
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex">Clip index</param>
        /// <param name="animationOffset">Animation offset</param>
        public void GetAnimationOffset(float time, int clipIndex, out int animationOffset)
        {
            animationOffset = 0;

            int index = 0;
            for (int i = 0; i <= clipIndex; i++)
            {
                float duration = this.GetClipDuration(i);
                int clipLength = (int)(duration / TimeStep);
                if (i != clipIndex)
                {
                    index += clipLength;
                }
                else
                {
                    float percent = time / duration;
                    int percentINT = (int)percent;
                    percent -= (float)percentINT;
                    index += (int)((float)clipLength * percent);
                }
            }

            animationOffset += (4 * this.skeleton.JointCount * index);

            //animationOffset = 9672 + (52 * 10);
        }

        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="index">Clip index</param>
        /// <returns>Returns the resulting transform list</returns>
        public Matrix[] GetPoseAtTime(float time, int index)
        {
            var res = new Matrix[this.skeleton.JointCount];

            if (index >= 0)
            {
                this.skeleton.GetPoseAtTime(time, this.animations[index].Animations, ref res);
            }

            return res;
        }
        /// <summary>
        /// Gets the transform list of the pose's combination at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="index1">First clip index</param>
        /// <param name="index2">Second clip index</param>
        /// <param name="offset1">Time offset for first clip</param>
        /// <param name="offset2">Time offset from second clip</param>
        /// <param name="duration">Total duration</param>
        /// <returns>Returns the resulting transform list</returns>
        public Matrix[] GetPoseAtTime(float time, int index1, int index2, float offset1, float offset2, float duration)
        {
            var res = new Matrix[this.skeleton.JointCount];

            if (index1 >= 0 && index2 >= 0)
            {
                var mats1 = new Matrix[this.skeleton.JointCount];
                var mats2 = new Matrix[this.skeleton.JointCount];

                this.skeleton.GetPoseAtTime(time + offset1, this.animations[index1].Animations, ref mats1);
                this.skeleton.GetPoseAtTime(time + offset2, this.animations[index2].Animations, ref mats2);

                float factor = time / duration;

                for (int i = 0; i < res.Length; i++)
                {
                    Vector3 sca1;
                    Quaternion rot1;
                    Vector3 pos1;
                    mats1[i].Decompose(out sca1, out rot1, out pos1);

                    Vector3 sca2;
                    Quaternion rot2;
                    Vector3 pos2;
                    mats2[i].Decompose(out sca2, out rot2, out pos2);

                    rot1.Normalize();
                    rot2.Normalize();

                    Vector3 translation = pos1 + (pos2 - pos1) * factor;
                    Quaternion rotation = Quaternion.Slerp(rot1, rot2, factor);
                    Vector3 scale = sca1 + (sca2 - sca1) * factor;

                    res[i] =
                        Matrix.Scaling(scale) *
                        Matrix.RotationQuaternion(rotation) *
                        Matrix.Translation(translation);
                }
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
                float duration = transition.Duration;
                int clipLength = (int)(duration / TimeStep);

                for (int t = 0; t < clipLength; t++)
                {
                    var mat = this.GetPoseAtTime(
                        t * TimeStep,
                        transition.ClipFrom, transition.ClipTo,
                        transition.StartFrom, transition.StartTo,
                        transition.Duration);

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
