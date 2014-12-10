using SharpDX;

namespace Engine.Content
{
    using Engine.Common;

    public class SkinningContent
    {
        public Matrix Transform { get; set; }
        public string Controller { get; set; }
        public Joint Skeleton { get; set; }

        public SkinningContent()
        {
            this.Transform = Matrix.Identity;
        }

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
