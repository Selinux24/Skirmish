using SharpDX;

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
        /// Inverse bind matrix list
        /// </summary>
        public Matrix[] InverseBindMatrix { get; set; }
        /// <summary>
        /// Weight information
        /// </summary>
        public Weight[] Weights { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerContent()
        {
            this.BindShapeMatrix = Matrix.Identity;
            this.InverseBindMatrix = new Matrix[] { };
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            string text = null;

            if (this.Skin != null) text += string.Format("Skin: {0}; ", this.Skin);
            if (this.Weights != null) text += string.Format("Weights: {0}; ", this.Weights.Length);
            if (this.BindShapeMatrix != null) text += string.Format("BindShapeMatrix: {0}; ", this.BindShapeMatrix);

            return text;
        }
    }
}
