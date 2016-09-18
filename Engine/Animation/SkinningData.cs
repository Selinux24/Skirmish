﻿using SharpDX;
using System;
using System.Collections.Generic;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;

namespace Engine.Animation
{
    using Engine.Helpers;

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
        private Dictionary<string, JointAnimation[]> animations = null;
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
            this.animations = new Dictionary<string, JointAnimation[]>();
            this.skeleton = skeleton;

            this.animations.Add(SkinningData.DefaultClip, animations);
            this.clips = new string[] { SkinningData.DefaultClip };
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="skeleton">Skeleton</param>
        /// <param name="animations">Animation dictionary</param>
        public SkinningData(Skeleton skeleton, Dictionary<string, JointAnimation[]> animations)
        {
            this.skeleton = skeleton;

            this.animations = animations;
            this.clips = this.animations.Keys.ToArray();
        }

        /// <summary>
        /// Gets the transform list of the pose at specified time
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipName">Clip name</param>
        public Matrix[] GetPoseAtTime(float time, string clipName)
        {
            return this.skeleton.GetPoseAtTime(time, this.animations[clipName]);
        }
        /// <summary>
        /// Gets final transform collection
        /// </summary>
        /// <returns>Returns final transform collection</returns>
        public Matrix[] GetFinalTransforms()
        {
            return this.skeleton.FinalTransforms;
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

            foreach (var clip in this.clips)
            {
                float duration = this.animations[clip][0].Duration;

                for (float t = 0; t < duration; t += TimeStep)
                {
                    var mat = this.GetPoseAtTime(t, clip);

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

            palette = game.Graphics.Device.CreateTexture2D(texWidth, values.ToArray());
            width = (uint)texWidth;
        }
        /// <summary>
        /// Gets the specified animation offset
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="clipIndex">Clip index</param>
        /// <param name="animationOffset">Animation offset</param>
        public void GetAnimationOffset(float time, int clipIndex, out int animationOffset)
        {
            string clipName = this.animations.Keys.ToList()[clipIndex];
            float duration = this.animations[clipName][0].Duration;
            int clipLength = (int)(duration / TimeStep);

            float percent = time / duration;
            int percentINT = (int)percent;
            percent -= (float)percentINT;
            int index = (int)((float)clipLength * percent);

            animationOffset = 4 * this.skeleton.JointCount * index;
        }
    }
}
