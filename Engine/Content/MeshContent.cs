using SharpDX;

namespace Engine.Content
{
    /// <summary>
    /// Mesh content
    /// </summary>
    public class MeshContent
    {
        /// <summary>
        /// Intial transformation
        /// </summary>
        public Matrix Transform { get; set; }
        /// <summary>
        /// Sub mesh names
        /// </summary>
        public string[] SubMeshes { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MeshContent()
        {
            this.Transform = Matrix.Identity;
        }

        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            if (this.SubMeshes != null && this.SubMeshes.Length == 1)
            {
                return string.Format("{0}", this.SubMeshes[0]);
            }
            else if (this.SubMeshes != null && this.SubMeshes.Length > 1)
            {
                return string.Format("{0}", string.Join(", ", this.SubMeshes));
            }
            else
            {
                return "Empty Mesh;";
            }
        }
    }
}
