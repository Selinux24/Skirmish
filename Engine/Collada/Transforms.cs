using System;
using SharpDX;

namespace Engine.Collada
{
    [Serializable]
    public struct Transforms
    {
        public Matrix Translation;
        public Matrix Rotation;
        public Matrix Scale;
        public Matrix Matrix
        {
            get
            {
                return this.Scale * this.Rotation * this.Translation;
            }
        }
    }
}
