using SharpDX;
using System;

namespace Engine.Animation
{
    /// <summary>
    /// Mesh skin info
    /// </summary>
    public class SkinInfo
    {
        /// <summary>
        /// Joint name list
        /// </summary>
        private string[] jointNames;
        /// <summary>
        /// Offsets
        /// </summary>
        private Matrix[] offsets;
        /// <summary>
        /// Inverse bind matrix list
        /// </summary>
        private Matrix[] ibmList;
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
        /// <param name="jointNames">Joint names</param>
        /// <param name="boneOffsets">Bone offsets</param>
        /// <param name="ibmList">Inverse bind matrix list</param>
        public SkinInfo(string[] jointNames, Matrix[] boneOffsets, Matrix[] ibmList)
        {
            this.jointNames = jointNames;
            this.offsets = boneOffsets;
            this.ibmList = ibmList;
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
            int numBones = boneHierarchy.Length;

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
                Matrix ibm = this.ibmList[i];
                Matrix offset = this.offsets[i];
                Matrix toRoot = this.toRootTransforms[i];

                this.FinalTransforms[i] = ibm * offset * toRoot;
            }

            string d = this.GetState();
        }

        /// <summary>
        /// Gets animation state
        /// </summary>
        /// <returns>Returns animation state</returns>
        public virtual string GetState()
        {
            string desc = "";

            int numBones = this.offsets.Length;

            for (int i = 0; i < numBones; i++)
            {
                desc += this.jointNames[i] + Environment.NewLine;
                desc += string.Format("ToParent : {0}", this.toParentTransforms[i].GetDescription()) + Environment.NewLine;
                desc += string.Format("Offset   : {0}", this.offsets[i].GetDescription()) + Environment.NewLine;
                desc += string.Format("ToRoot   : {0}", this.toRootTransforms[i].GetDescription()) + Environment.NewLine;
                desc += string.Format("Final    : {0}", this.FinalTransforms[i].GetDescription()) + Environment.NewLine;
                desc += "=======================================" + Environment.NewLine;
            }

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
