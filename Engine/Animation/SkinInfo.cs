using SharpDX;
using System;
using System.Text;

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
        /// Resolves animation transforms
        /// </summary>
        /// <param name="clip">Animation clip</param>
        /// <param name="time">Animation time</param>
        /// <param name="boneHierarchy">Bone hierarchy</param>
        /// <param name="boneNames">Bone names</param>
        /// <param name="offsets">Offset list</param>
        /// <param name="toParentTransforms">To parent transforms list</param>
        /// <param name="toRootTransforms">To root transforms list</param>
        /// <param name="finalTransforms">Final transforms list</param>
        private static void Resolve(
            AnimationClip clip, float time, ref int[] boneHierarchy, ref string[] boneNames, 
            ref Matrix[] offsets,
            ref Matrix[] toParentTransforms, ref Matrix[] toRootTransforms,
            ref Matrix[] finalTransforms)
        {
            //Get relative transformations from each bone to his parent
            clip.Interpolate(time, ref toParentTransforms);

            int numBones = boneHierarchy.Length;

            toRootTransforms[0] = toParentTransforms[0];

            for (int i = 1; i < numBones; i++)
            {
                int parentIndex = boneHierarchy[i];

                Matrix toParent = toParentTransforms[i];
                Matrix toRoot = toRootTransforms[parentIndex];

                toRootTransforms[i] = toRoot * toParent;
            }

            for (int i = 0; i < numBones; i++)
            {
                Matrix currentGlobalTransform = toRootTransforms[i];
                Matrix offset = offsets[i];

                finalTransforms[i] = currentGlobalTransform * offset;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="jointNames">Joint names</param>
        /// <param name="boneOffsets">Bone offsets</param>
        public SkinInfo(string[] jointNames, Matrix[] boneOffsets)
        {
            this.jointNames = jointNames;
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
        /// <param name="boneNames">Bone names</param>
        public void Update(AnimationClip clip, float time, int[] boneHierarchy, string[] boneNames)
        {
            Resolve(clip, time, ref boneHierarchy, ref boneNames,
                ref this.offsets,
                ref this.toParentTransforms, ref this.toRootTransforms,
                ref this.FinalTransforms);
        }
        /// <summary>
        /// Fills skin info description into the specified StringBuilder
        /// </summary>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(ref StringBuilder desc)
        {
            for (int i = 0; i < this.jointNames.Length; i++)
            {
                desc.AppendLine(this.jointNames[i]);
                desc.AppendLine("To Parent:");
                desc.AppendLine(this.toParentTransforms[i].GetDescription());
                desc.AppendLine("To Root:");
                desc.AppendLine(this.toRootTransforms[i].GetDescription());
                desc.AppendLine("Offset:");
                desc.AppendLine(this.offsets[i].GetDescription());
                desc.AppendLine("Final:");
                desc.AppendLine(this.FinalTransforms[i].GetDescription());
                desc.AppendLine();
            }
        }
        /// <summary>
        /// Fills skin info description into the specified StringBuilder
        /// </summary>
        /// <param name="clip">Animation clip</param>
        /// <param name="time">Time</param>
        /// <param name="boneHierarchy">Bone hierarchy</param>
        /// <param name="boneNames">Bone names</param>
        /// <param name="desc">Description to fill</param>
        public void GetDescription(AnimationClip clip, float time, int[] boneHierarchy, string[] boneNames, ref StringBuilder desc)
        {
            Matrix[] toParentList = new Matrix[this.offsets.Length];
            Matrix[] toRootList = new Matrix[this.offsets.Length];
            Matrix[] finalList = new Matrix[this.offsets.Length];
            Resolve(clip, time, ref boneHierarchy, ref boneNames,
                ref this.offsets,
                ref toParentList, ref toRootList,
                ref finalList);

            for (int i = 0; i < this.jointNames.Length; i++)
            {
                desc.AppendLine(this.jointNames[i]);
                desc.AppendLine("To Parent:");
                desc.AppendLine(toParentList[i].GetDescription());
                desc.AppendLine("To Root:");
                desc.AppendLine(toRootList[i].GetDescription());
                desc.AppendLine("Offset:");
                desc.AppendLine(this.offsets[i].GetDescription());
                desc.AppendLine("Final:");
                desc.AppendLine(finalList[i].GetDescription());
                desc.AppendLine();
            }
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
