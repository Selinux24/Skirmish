using System;
using System.Collections.Generic;
using SharpDX;

namespace Engine.Common
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
        /// Bone hierarchy collection
        /// </summary>
        private int[] boneHierarchy = null;
        /// <summary>
        /// Animation dictionary by bone
        /// </summary>
        private Dictionary<string, AnimationClip> animations = new Dictionary<string, AnimationClip>();
        /// <summary>
        /// Skinning info by mesh
        /// </summary>
        private Dictionary<string, SkinInfo> meshSkinInfo = new Dictionary<string, SkinInfo>();

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
        public static SkinningData Create(int[] boneHierarchy, Dictionary<string, AnimationClip> animations, Dictionary<string, SkinInfo> meshSkinInfo)
        {
            List<string> clipNames = new List<string>();

            foreach (string key in animations.Keys)
            {
                clipNames.Add(key);
            }

            return new SkinningData()
            {
                animations = animations,
                boneHierarchy = boneHierarchy,
                meshSkinInfo = meshSkinInfo,
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

                    foreach (SkinInfo sk in this.meshSkinInfo.Values)
                    {
                        sk.Update(clip, this.Time, this.boneHierarchy);
                    }

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
        /// Sets clip to play
        /// </summary>
        /// <param name="clipName">Clip name</param>
        public void SetClip(string clipName)
        {
            this.ClipName = clipName;
        }
        /// <summary>
        /// Gets final transform collection
        /// </summary>
        /// <param name="meshName">Mesh name</param>
        /// <returns>Returns final transform collection</returns>
        public Matrix[] GetFinalTransforms(string meshName)
        {
            if (this.meshSkinInfo.ContainsKey(meshName))
            {
                return this.meshSkinInfo[meshName].FinalTransforms;
            }

            return null;
        }

        /// <summary>
        /// Gest animation state
        /// </summary>
        /// <returns>Returns animation state in current time</returns>
        public virtual string GetState()
        {
            string desc = "";

            foreach (string key in this.meshSkinInfo.Keys)
            {
                SkinInfo info = this.meshSkinInfo[key];

                desc += key + Environment.NewLine;
                desc += string.Format("Offset: {0}", info.GetState()) + Environment.NewLine;
            }

            return desc;
        }
        /// <summary>
        /// Gest animation state
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns animation state in specified time</returns>
        public virtual string GetState(float time)
        {
            foreach (SkinInfo sk in this.meshSkinInfo.Values)
            {
                sk.Update(this[this.ClipName], time, this.boneHierarchy);
            }

            string desc = string.Format("Time: {0}", time) + Environment.NewLine + Environment.NewLine;

            desc += this.GetState();

            return desc;
        }
        /// <summary>
        /// Gets text representation of skinning data instance
        /// </summary>
        /// <returns>Returns text representation of skinning data instance</returns>
        public override string ToString()
        {
            string desc = "";

            desc += "BoneIndex: " + string.Join(",", this.boneHierarchy) + Environment.NewLine;

            foreach (string key in this.meshSkinInfo.Keys)
            {
                SkinInfo info = this.meshSkinInfo[key];

                desc += key + Environment.NewLine;
                desc += string.Format("Offset: {0}", info) + Environment.NewLine;
            }

            int index = 0;
            foreach (string key in this.animations.Keys)
            {
                AnimationClip clip = this.animations[key];

                desc += key + Environment.NewLine + Environment.NewLine;

                Array.ForEach(clip.BoneAnimations, (b) =>
                {
                    desc += string.Format("Bone: {0}", index++) + Environment.NewLine;

                    Array.ForEach(b.Keyframes, (k) =>
                    {
                        desc += string.Format("Transform: {0}", k.Transform.GetDescription()) + Environment.NewLine;
                    });
                    desc += Environment.NewLine;
                });
            }

            return desc;
        }
    }

    /// <summary>
    /// Mesh skin info
    /// </summary>
    public class SkinInfo
    {
        /// <summary>
        /// Offsets
        /// </summary>
        private Matrix[] offsets;
        /// <summary>
        /// To parent transforms cache
        /// </summary>
        private Matrix[] toParentTransforms = null;
        /// <summary>
        /// To root transforms cache
        /// </summary>
        private Matrix[] toRootTransforms = null;
        /// <summary>
        /// Final transforms
        /// </summary>
        public Matrix[] FinalTransforms = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boneOffsets">Bone offsets</param>
        public SkinInfo(Matrix[] boneOffsets)
        {
            this.offsets = boneOffsets;
            this.toParentTransforms = Helper.CreateArray(boneOffsets.Length, Matrix.Identity);
            this.toRootTransforms = Helper.CreateArray(boneOffsets.Length, Matrix.Identity);
            this.FinalTransforms = Helper.CreateArray(boneOffsets.Length, Matrix.Identity);
        }

        /// <summary>
        /// Update final transforms
        /// </summary>
        /// <param name="clip">Clip</param>
        /// <param name="time">Time</param>
        /// <param name="boneHierarchy">Bone hierarchy</param>
        public void Update(AnimationClip clip, float time, int[] boneHierarchy)
        {
            int numBones = this.offsets.Length;

            //Get relative transformations from each bone to his parent
            clip.Interpolate(time, ref this.toParentTransforms);

            //Compute transformations from each bone to root

            //First bone has no parents. Share transform
            this.toRootTransforms[0] = this.toParentTransforms[0];

            //Next bones multiply transforms from tail to root
            for (int i = 1; i < numBones; i++)
            {
                int parentIndex = boneHierarchy[i];

                Matrix toParent = this.toParentTransforms[i];
                Matrix parentToRoot = this.toRootTransforms[parentIndex];

                this.toRootTransforms[i] = toParent * parentToRoot;
            }

            //Apply bone offsets (rest pose)
            for (int i = 0; i < numBones; i++)
            {
                Matrix offset = this.offsets[i];
                Matrix toRoot = this.toRootTransforms[i];

                this.FinalTransforms[i] = offset * toRoot;
            }
        }

        /// <summary>
        /// Gets animation state
        /// </summary>
        /// <returns>Returns animation state</returns>
        public virtual string GetState()
        {
            string desc = "";

            Array.ForEach(this.FinalTransforms, (b) => { desc += string.Format("Offset: {0}", b.GetDescription()) + Environment.NewLine; });

            return desc;
        }
        /// <summary>
        /// Gets text description
        /// </summary>
        /// <returns>Returns text description</returns>
        public override string ToString()
        {
            string desc = "";

            Array.ForEach(this.offsets, (b) => { desc += string.Format("Offset: {0}", b.GetDescription()) + Environment.NewLine; });

            return desc;
        }
    }
}
