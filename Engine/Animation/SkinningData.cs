using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

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
        /// Skeleton
        /// </summary>
        public Skeleton Skeleton = null;
        /// <summary>
        /// Animation state description, updated every frame
        /// </summary>
        private StringBuilder animationStateDescription = new StringBuilder();

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
        /// <param name="boneHierarchy">Bone hierarchy</param>
        /// <param name="boneNames">Bone name list</param>
        /// <param name="animations">Animation dictionary</param>
        /// <param name="meshSkinInfo">Skinning data</param>
        /// <returns>Returns skinning data</returns>
        public static SkinningData Create(Skeleton skeleton)
        {
            return new SkinningData()
            {
                Skeleton = skeleton,
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
                this.Skeleton.Update(this.Time, this.ClipName);

                //if (this.Time == endTime && this.Loop == false)
                //{
                //    //Do Nothing
                //    return;
                //}
                //else
                //{
                //    //this.Time += gameTime.ElapsedSeconds * this.AnimationVelocity;

                //    if (this.Time > endTime)
                //    {
                //        if (this.Loop)
                //        {
                //            //Loop
                //            this.Time -= endTime;
                //        }
                //        else
                //        {
                //            //Stop
                //            this.Time = endTime;
                //        }
                //    }
                //}
            }

            string state = this.animationStateDescription.ToString();
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
            return this.Skeleton.FinalTransforms;
        }

        /// <summary>
        /// Gest animation state
        /// </summary>
        /// <returns>Returns animation state in current time</returns>
        public string GetState()
        {
            return this.animationStateDescription.ToString();
        }
        /// <summary>
        /// Gest animation state
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Returns animation state in specified time</returns>
        public string GetState(float time)
        {
            StringBuilder desc = new StringBuilder();

            //if (this.ClipName != null)
            //{
            //    AnimationClip clip = this[this.ClipName];

            //    clip.GetDescription(ref desc);

            //    foreach (SkinInfo sk in this.meshSkinInfo.Values)
            //    {
            //        sk.Update(clip, time, this.boneHierarchy, this.boneNames);
            //        sk.GetDescription(ref desc);
            //    }
            //}

            return desc.ToString();
        }
        /// <summary>
        /// Gets text representation of skinning data instance
        /// </summary>
        /// <returns>Returns text representation of skinning data instance</returns>
        public override string ToString()
        {
            string desc = "";

            //desc += "BoneIndex: " + string.Join(",", this.boneHierarchy) + Environment.NewLine;

            //foreach (string key in this.meshSkinInfo.Keys)
            //{
            //    SkinInfo info = this.meshSkinInfo[key];

            //    desc += key + Environment.NewLine;
            //    desc += string.Format("Offset: {0}", info) + Environment.NewLine;
            //}

            //int index = 0;
            //foreach (string key in this.animations.Keys)
            //{
            //    AnimationClip clip = this.animations[key];

            //    desc += key + Environment.NewLine + Environment.NewLine;

            //    Array.ForEach(clip.BoneAnimations, (b) =>
            //    {
            //        desc += string.Format("Bone: {0}", index++) + Environment.NewLine;

            //        Array.ForEach(b.Keyframes, (k) =>
            //        {
            //            desc += string.Format("Transform: {0}", k.Transform.GetDescription()) + Environment.NewLine;
            //        });
            //        desc += Environment.NewLine;
            //    });
            //}

            return desc;
        }

        public void Test(float time, string meshName, out Matrix[] trns, out string[] jointNames)
        {
            trns = null;
            jointNames = null;

            //if (this.ClipName != null)
            //{
            //    AnimationClip clip = this[this.ClipName];

            //    SkinInfo sk = this.meshSkinInfo[meshName];

            //    sk.Update(clip, time, this.boneHierarchy, this.boneNames);

            //    trns = this.GetFinalTransforms(meshName);
            //    jointNames = this.boneNames;
            //}
        }
    }
}
