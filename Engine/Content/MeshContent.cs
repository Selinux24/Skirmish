using SharpDX;

namespace Engine.Content
{
    public class MeshContent
    {
        public Matrix Transform { get; set; }
        public string[] SubMeshes { get; set; }

        public MeshContent()
        {
            this.Transform = Matrix.Identity;
        }

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
