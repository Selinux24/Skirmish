using SharpDX;

namespace Engine.Content
{
    using Engine.Common;

    /// <summary>
    /// Skinning content
    /// </summary>
    public class SkinningContent
    {
        /// <summary>
        /// Initial transformation
        /// </summary>
        public Matrix Transform { get; set; }
        /// <summary>
        /// Controller name
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// Skeleton information
        /// </summary>
        public Joint Skeleton { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkinningContent()
        {
            this.Transform = Matrix.Identity;
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            if (this.Controller != null && this.Controller.Length == 1)
            {
                return string.Format("{0}", this.Controller[0]);
            }
            else if (this.Controller != null && this.Controller.Length > 1)
            {
                return string.Format("{0}", string.Join(", ", this.Controller));
            }
            else
            {
                return "Empty Controller;";
            }
        }
    }
}
