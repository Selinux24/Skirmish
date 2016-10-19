using SharpDX;
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
        public Matrix BindShapeMatrix { get; set; }
        /// <summary>
        /// Inverse bind matrix list by joint name
        /// </summary>
        public Dictionary<string, Matrix> InverseBindMatrix { get; set; }
        /// <summary>
        /// Weight information by joint name
        /// </summary>
        public Weight[] Weights { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerContent()
        {
            this.BindShapeMatrix = Matrix.Identity;
            this.InverseBindMatrix = new Dictionary<string, Matrix>();
            this.Weights = new Weight[] { };
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            string text = null;

            if (this.Skin != null) text += string.Format("Skin: {0}; ", this.Skin);
            if (this.InverseBindMatrix != null) text += string.Format("InverseBindMatrix: {0}; ", this.InverseBindMatrix.Count);
            if (this.Weights != null) text += string.Format("Weights: {0}; ", this.Weights.Length);
            if (this.BindShapeMatrix != null) text += string.Format("BindShapeMatrix: {0}; ", this.BindShapeMatrix);

            return text;
        }
    }
}
