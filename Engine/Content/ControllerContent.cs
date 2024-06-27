using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Controller content
    /// </summary>
    public class ControllerContent
    {
        /// <summary>
        /// Skin name
        /// </summary>
        public string Skin { get; set; }
        /// <summary>
        /// Skeleton name
        /// </summary>
        public string Armature { get; set; }
        /// <summary>
        /// Bind shape matrix
        /// </summary>
        public Matrix BindShapeMatrix { get; set; } = Matrix.Identity;
        /// <summary>
        /// Inverse bind matrix list by joint name
        /// </summary>
        public Dictionary<string, Matrix> InverseBindMatrix { get; set; } = new Dictionary<string, Matrix>();
        /// <summary>
        /// Weight information by joint name
        /// </summary>
        public Weight[] Weights { get; set; } = Array.Empty<Weight>();

        /// <inheritdoc/>
        public override string ToString()
        {
            string text = null;

            if (Skin != null) text += $"Skin: {Skin}; ";
            if (InverseBindMatrix != null) text += $"InverseBindMatrix: {InverseBindMatrix.Count}; ";
            if (Weights != null) text += $"Weights: {Weights.Length}; ";
            text += $"BindShapeMatrix: {BindShapeMatrix};";

            return text;
        }
    }
}
