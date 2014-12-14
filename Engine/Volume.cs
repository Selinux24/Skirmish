using SharpDX;

namespace Engine
{
    using Engine.Content;

    public class VolumeBox : Model
    {
        public VolumeBox(Game game, Scene3D scene, Color color)
            : base(game, scene, ModelContent.GenerateBoundingBox(color))
        {

        }
    }

    public class VolumeSphere : Model
    {
        public VolumeSphere(Game game, Scene3D scene, uint slices, uint stacks, Color color)
            : base(game, scene, ModelContent.GenerateBoundingSphere(slices, stacks, color))
        {

        }
    }
}
