using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
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
        /// Animation dictionary by bone
        /// </summary>
        private Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();
        /// <summary>
        /// Bone offsets collection
        /// </summary>
        private Matrix[] boneOffsets = null;
        /// <summary>
        /// Bone hierarchy collection
        /// </summary>
        private int[] boneHierarchy = null;
        /// <summary>
        /// Transformation current to parent list
        /// </summary>
        private Matrix[] toParentTransforms = null;
        /// <summary>
        /// Transformation current to root list
        /// </summary>
        private Matrix[] toRootTransforms = null;

        /// <summary>
        /// Skin list
        /// </summary>
        public string[] Skins = null;
        //TODO: diccionario de clips calculados
        /// <summary>
        /// Final transform list
        /// </summary>
        public Matrix[] FinalTransforms = null;
        /// <summary>
        /// Number of bones
        /// </summary>
        public int BoneCount
        {
            get
            {
                return this.boneOffsets.Length;
            }
        }
        /// <summary>
        /// Default clip
        /// </summary>
        public AnimationClip Default
        {
            get
            {
                return this.animations[DefaultClip];
            }
        }
        /// <summary>
        /// Clip name list
        /// </summary>
        public string[] Clips { get; private set; }
        /// <summary>
        /// Gets clip by name
        /// </summary>
        /// <param name="clipName"></param>
        /// <returns>Returns clip by name</returns>
        public AnimationClip this[string clipName]
        {
            get
            {
                return this.animations[clipName];
            }
        }
        /// <summary>
        /// Animation velocity modifier
        /// </summary>
        public float AnimationVelocity = 1f;

        /// <summary>
        /// Current clip name
        /// </summary>
        public string ClipName { get; private set; }
        /// <summary>
        /// Clip position
        /// </summary>
        public float Time { get; set; }
        /// <summary>
        /// Gets or sets if clip must be looped
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// Generates skinning data from animation info
        /// </summary>
        /// <param name="skins">Skins</param>
        /// <param name="boneHierarchy">Bone hierarchy</param>
        /// <param name="boneOffsets">Bone offsets</param>
        /// <param name="animations">Animation dictionary</param>
        /// <returns>Returns skinning data</returns>
        public static SkinningData Create(string[] skins, int[] boneHierarchy, Matrix[] boneOffsets, Dictionary<string, AnimationClip> animations)
        {
            List<string> clipNames = new List<string>();

            foreach (string key in animations.Keys)
            {
                clipNames.Add(key);
            }

            return new SkinningData()
            {
                animations = animations,
                boneOffsets = boneOffsets,
                boneHierarchy = boneHierarchy,
                toParentTransforms = Helper.CreateArray(boneOffsets.Length, Matrix.Identity),
                toRootTransforms = Helper.CreateArray(boneOffsets.Length, Matrix.Identity),

                Skins = skins,
                FinalTransforms = Helper.CreateArray(boneOffsets.Length, Matrix.Identity),

                Clips = clipNames.ToArray(),
                ClipName = DefaultClip,
                Time = 0f,
                Loop = true,
            };
        }

        /// <summary>
        /// Update state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public virtual void Update(GameTime gameTime)
        {
            if (this.ClipName != null)
            {
                AnimationClip clip = this[this.ClipName];
                float endTime = clip.EndTime;

                if (this.Time == endTime && this.Loop == false)
                {
                    //Do Nothing
                    return;
                }
                else
                {
                    this.Time += gameTime.ElapsedSeconds * this.AnimationVelocity;

                    this.UpdateFinalTransforms(clip, this.Time);

                    if (this.Time > endTime)
                    {
                        if (this.Loop)
                        {
                            //Loop
                            this.Time -= endTime;
                        }
                        else
                        {
                            //Stop
                            this.Time = endTime;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Update final transform list
        /// </summary>
        /// <param name="clip">Clip</param>
        /// <param name="time">Time</param>
        private void UpdateFinalTransforms(AnimationClip clip, float time)
        {
            int numBones = this.boneOffsets.Length;

            //Get relative transformations from each bone to his parent
            clip.Interpolate(time, ref this.toParentTransforms);

            //Compute transformations from each bone to root

            //First bone has no parents. Share transform
            this.toRootTransforms[0] = this.toParentTransforms[0];

            //Next bones multiply transforms from tail to root
            for (int i = 1; i < numBones; i++)
            {
                int parentIndex = this.boneHierarchy[i];

                Matrix toParent = this.toParentTransforms[i];
                Matrix parentToRoot = this.toRootTransforms[parentIndex];

                this.toRootTransforms[i] = toParent * parentToRoot;
            }

            //Apply bone offsets (rest pose)
            for (int i = 0; i < numBones; i++)
            {
                Matrix offset = this.boneOffsets[i];
                Matrix toRoot = this.toRootTransforms[i];

                this.FinalTransforms[i] = offset * toRoot;
            }
        }
        /// <summary>
        /// Sets clip to play
        /// </summary>
        /// <param name="clipName">Clip name</param>
        public void SetClip(string clipName)
        {
            this.ClipName = clipName;
        }

        /// <summary>
        /// Gets text representation of skinning data instance
        /// </summary>
        /// <returns>Returns text representation of skinning data instance</returns>
        public override string ToString()
        {
            string desc = "";

            Array.ForEach(boneHierarchy, (b) => { desc += string.Format("Index: {0}", b) + Environment.NewLine; });
            desc += Environment.NewLine;
            Array.ForEach(boneOffsets, (b) => { desc += string.Format("Offset: {0}", b.GetDescription()) + Environment.NewLine; });
            desc += Environment.NewLine;

            foreach (string key in this.animations.Keys)
            {
                AnimationClip clip = this.animations[key];

                Array.ForEach(clip.BoneAnimations, (b) =>
                {
                    Array.ForEach(b.Keyframes, (k) => { desc += string.Format("Key: {0}; Transform: {1}", key, k.Transform.GetDescription()) + Environment.NewLine; });
                    desc += Environment.NewLine;
                });
            }

            return desc;
        }
    }
}
