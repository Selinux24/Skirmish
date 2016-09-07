using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Animation
{
    public class Frame
    {
        public string Name; // the frame or "bone" name
        public Matrix TransformationMatrix; // to be used for local animation matrix
        //MeshContainer MeshData; // perhaps only one or two frames will have mesh data
        public Frame[] Children; // pointers or references to each child frame of this frame
        public Matrix ToParent; // the local transform from bone-space to bone's parent-space
        public Matrix ToRoot; // from bone-space to root-frame space

        public static void CalcToRootMatrix(Frame frame, Matrix parentMatrix)
        {
            // transform from frame-space to root-frame-space through the parent's ToRoot matrix
            frame.ToRoot = frame.ToParent * parentMatrix;

            foreach (var Child in frame.Children)
            {
                CalcToRootMatrix(Child, frame.ToRoot);
            }
        }

        public static void CalcCombinedMatrix(Frame frame, Matrix parentMatrix)
        {
            // transform from frame-space to root-frame-space through the parent's ToRoot matrix
            frame.TransformationMatrix = frame.TransformationMatrix * parentMatrix;

            foreach (var Child in frame.Children)
            {
                CalcCombinedMatrix(Child, frame.TransformationMatrix);
            }
        }

        public static Frame FindFrame(Frame frame, string frameName)
        {
            Frame tmpFrame;

            if (frame.Name == frameName) return frame;
            foreach (var Child in frame.Children)
            {
                if ((tmpFrame = FindFrame(Child, frameName)) != null) return tmpFrame;
            }

            return null;
        }
    }

    public class SkinInfo2
    {
        public Frame RootFrame;
        public Matrix[] offsetMatrix;
        public Matrix[] FinalMatrix;

        private string GetBoneName(int boneIndex)
        {
            throw new NotImplementedException();
        }

        public SkinInfo2()
        {
            Frame.CalcToRootMatrix(this.RootFrame, Matrix.Identity);

            for (int i = 0; i < this.NumBones(); i++)
            {
                this.CalculateOffsetMatrix(i);
            }
        }

        private int NumBones()
        {
            throw new NotImplementedException();
        }

        public void CalculateOffsetMatrix(int boneIndex)
        {
            string boneName = this.GetBoneName(boneIndex);
            Frame boneFrame = Frame.FindFrame(RootFrame, boneName);

            //offsetMatrix[boneIndex] = MeshFrame.ToRoot * Matrix.Invert(boneFrame.ToRoot);
        }

        public void CalculateFinalMatrix(int boneIndex)
        {
            string boneName = this.GetBoneName(boneIndex);
            Frame boneFrame = Frame.FindFrame(RootFrame, boneName);

            FinalMatrix[boneIndex] = offsetMatrix[boneIndex] * boneFrame.TransformationMatrix;
        }

        public void Render(float time)
        {
            /*
             * 1. For each frame, a timed-key-matrix is calculated from the frame's keys. 
             *    If the tick count is "between" two keys, the matrix calculated is an interpolation of the key with the 
             *    next lowest tick count, and the key with the next higher tick count. Those matrices are stored in a key array.
             */


            /*
             * 2. When all the frame hierarchy timed-key-matrices have been calculated, the timed-key-matrix for each frame 
             *    is combined with the timed-key-matrix for its parent.
             */


            /*
             * 3. Final transforms are calculated and stored in an array.
             */


            /*
             * 4. The next operation is commonly performed in a vertex shader as GPU hardware is more efficient at performing 
             *    the required calculations, though it can be done in system memory by the application.             
             */

        }
    }
}
