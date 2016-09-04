using SharpDX;
using System;
using System.Collections.Generic;

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
                ClipName = animations.Count > 0 ? DefaultClip : null,
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
}
