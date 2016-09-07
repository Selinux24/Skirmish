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

        public static string DEBUGSTR = "";

        /// <summary>
        /// Update final transforms
        /// </summary>
        /// <param name="clip">Clip</param>
        /// <param name="time">Time</param>
        /// <param name="boneHierarchy">Bone hierarchy</param>
        public void Update(AnimationClip clip, float time, int[] boneHierarchy)
        {
            //Matrix w = new Matrix(
            //    1, 0, 0, 0,
            //    0, 0, 1, 0,
            //    0, -1, 0, 0,
            //    0, 0, 0, 1);
            Matrix w = Matrix.Identity;

            int numBones = boneHierarchy.Length;

            //Get relative transformations from each bone to his parent
            clip.Interpolate(0, ref this.toParentTransforms);
            string d = this.toParentTransforms.Debug() + Environment.NewLine;

            this.toRootTransforms[0] = w * this.toParentTransforms[0];

            DEBUGSTR += "==>" + Environment.NewLine;
            DEBUGSTR += "LOCAL" + Environment.NewLine;
            DEBUGSTR += this.toParentTransforms[0].Debug() + Environment.NewLine;
            DEBUGSTR += "GLOBAL" + Environment.NewLine;
            DEBUGSTR += this.toRootTransforms[0].Debug() + Environment.NewLine;
            DEBUGSTR += Environment.NewLine;

            for (int i = 1; i < numBones; i++)
            {
                int parentIndex = boneHierarchy[i];

                Matrix toParent = this.toParentTransforms[i];
                Matrix toRoot = this.toRootTransforms[parentIndex];

                this.toRootTransforms[i] = toRoot * toParent;

                DEBUGSTR += "==>" + Environment.NewLine;
                DEBUGSTR += "LOCAL" + Environment.NewLine;
                DEBUGSTR += toParent.Debug() + Environment.NewLine;
                DEBUGSTR += "GLOBAL" + Environment.NewLine;
                DEBUGSTR += (toRoot * toParent).Debug() + Environment.NewLine;
                DEBUGSTR += Environment.NewLine;
            }

            Matrix globalInverseMeshTransform = Matrix.Invert(w);

            var tmp1 = new Matrix[this.FinalTransforms.Length];
            tmp1[0] = this.toRootTransforms[0];
            tmp1[1] = this.toRootTransforms[7];
            tmp1[2] = this.toRootTransforms[8];
            tmp1[3] = this.toRootTransforms[9];
            tmp1[4] = this.toRootTransforms[10];
            tmp1[5] = this.toRootTransforms[1];
            tmp1[6] = this.toRootTransforms[2];
            tmp1[7] = this.toRootTransforms[3];
            tmp1[8] = this.toRootTransforms[11];
            tmp1[9] = this.toRootTransforms[12];
            tmp1[10] = this.toRootTransforms[4];
            tmp1[11] = this.toRootTransforms[5];
            tmp1[12] = this.toRootTransforms[6];

            var tmp2 = new Matrix[this.FinalTransforms.Length];
            tmp2[0] = this.ibmList[0] * this.offsets[0];
            tmp2[1] = this.ibmList[7] * this.offsets[7];
            tmp2[2] = this.ibmList[8] * this.offsets[8];
            tmp2[3] = this.ibmList[9] * this.offsets[9];
            tmp2[4] = this.ibmList[10] * this.offsets[10];
            tmp2[5] = this.ibmList[1] * this.offsets[1];
            tmp2[6] = this.ibmList[2] * this.offsets[2];
            tmp2[7] = this.ibmList[3] * this.offsets[3];
            tmp2[8] = this.ibmList[11] * this.offsets[11];
            tmp2[9] = this.ibmList[12] * this.offsets[12];
            tmp2[10] = this.ibmList[4] * this.offsets[4];
            tmp2[11] = this.ibmList[5] * this.offsets[5];
            tmp2[12] = this.ibmList[6] * this.offsets[6];

            string DD = "";

            for (int i = 0; i < numBones; i++)
            {
                Matrix currentGlobalTransform = tmp1[i];

                this.FinalTransforms[i] = globalInverseMeshTransform * currentGlobalTransform * tmp2[i];
                this.FinalTransforms[i].Transpose();

                DD += this.jointNames[i] + "==>" + Environment.NewLine;
                DD += globalInverseMeshTransform.Debug() + Environment.NewLine;
                DD += currentGlobalTransform.Debug() + Environment.NewLine;
                DD += (tmp2[i]).Debug() + Environment.NewLine;
                DD += this.FinalTransforms[i].Debug() + Environment.NewLine;
                DD += Environment.NewLine;
            }

            var tmp3 = new Matrix[this.FinalTransforms.Length];
            tmp3[0] = this.FinalTransforms[0];
            tmp3[1] = this.FinalTransforms[5];
            tmp3[2] = this.FinalTransforms[6];
            tmp3[3] = this.FinalTransforms[7];
            tmp3[4] = this.FinalTransforms[10];
            tmp3[5] = this.FinalTransforms[11];
            tmp3[6] = this.FinalTransforms[12];
            tmp3[7] = this.FinalTransforms[1];
            tmp3[8] = this.FinalTransforms[2];
            tmp3[9] = this.FinalTransforms[3];
            tmp3[10] = this.FinalTransforms[4];
            tmp3[11] = this.FinalTransforms[8];
            tmp3[12] = this.FinalTransforms[9];

            //S:None
            //T:Zero
            //R:Angle: 77,33 in axis X:-0,075 Y:-0,997 Z:-0,019

            //tmp3[9] = Matrix.RotationAxis(new Vector3(0,0,1), MathUtil.DegreesToRadians(10f));
            //tmp3[10] = Matrix.Identity;
            //tmp3[11] = Matrix.Identity;
            //tmp3[12] = Matrix.Identity;

            this.FinalTransforms = tmp3;
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
                desc += string.Format("IBM      : {0}", this.ibmList[i].GetDescription()) + Environment.NewLine;
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
