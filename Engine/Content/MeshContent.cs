using SharpDX;
using System.Linq;

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
        public Matrix Transform { get; set; } = Matrix.Identity;
        /// <summary>
        /// Sub mesh names
        /// </summary>
        public string[] SubMeshes { get; set; } = new string[] { };

        /// <inheritdoc/>
        public override string ToString()
        {
            if (SubMeshes?.Any() == true)
            {
                return $"{string.Join(", ", SubMeshes)}";
            }
            else
            {
                return "Empty Mesh;";
            }
        }
    }
}
