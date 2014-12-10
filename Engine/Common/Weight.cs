using System;

namespace Engine.Common
{
    [Serializable]
    public struct Weight
    {
        public int VertexIndex;
        public int BoneIndex;
        public string Joint;
        public float WeightValue;

        public override string ToString()
        {
            string text = null;

            text += string.Format("VertexIndex: {0}; ", this.VertexIndex);
            text += string.Format("BoneIndex: {0}; ", this.BoneIndex);
            text += string.Format("Joint: {0}; ", this.Joint);
            text += string.Format("Weight: {0}; ", this.WeightValue);

            return text;
        }
    }
}
