using SharpDX;

namespace Engine.Content
{
    using Engine.Common;

    public class ControllerContent
    {
        public string Skin { get; set; }
        public string Armature { get; set; }
        public Matrix BindShapeMatrix { get; set; }
        public Matrix[] InverseBindMatrix { get; set; }
        public Weight[] Weights { get; set; }

        public ControllerContent()
        {
            this.BindShapeMatrix = Matrix.Identity;
            this.InverseBindMatrix = new Matrix[] { };
        }

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
