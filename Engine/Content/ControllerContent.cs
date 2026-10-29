using System.Collections.Generic;

namespace Engine.Content
{
    using Engine.Common;
    using Engine.Mathematics;

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
        public Mathematics.Matrix4x4 BindShapeMatrix { get; set; } = Mathematics.Matrix4x4.Identity;
        /// <summary>
        /// Inverse bind matrix list by joint name
        /// </summary>
        public Dictionary<string, Mathematics.Matrix4x4> InverseBindMatrix { get; set; } = [];
        /// <summary>
        /// Weight information by joint name
        /// </summary>
        public Weight[] Weights { get; set; } = [];

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
