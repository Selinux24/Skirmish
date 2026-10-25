using System.Numerics;

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
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
        /// <summary>
        /// Sub mesh names
        /// </summary>
        public string[] SubMeshes { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString()
        {
            if (SubMeshes.Length != 0)
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
