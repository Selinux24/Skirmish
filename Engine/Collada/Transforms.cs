using System;
using SharpDX;

namespace Engine.Collada
{
    public struct Transforms
    {
        /// <summary>
        /// Translation matrix
        /// </summary>
        public Matrix Translation;
        /// <summary>
        /// Rotation matrix
        /// </summary>
        public Matrix Rotation;
        /// <summary>
        /// Scale matrix
        /// </summary>
        public Matrix Scale;
        /// <summary>
        /// Transform matrix (scale * rotation * translation)
        /// </summary>
        public Matrix Matrix
        {
            get
            {
                return this.Scale * this.Rotation * this.Translation;
            }
        }
    }
}
