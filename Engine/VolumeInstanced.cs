using SharpDX;

namespace Engine
{
    using Engine.Content;

    public class VolumeBoxInstanced : ModelInstanced
    {
        public VolumeBoxInstanced(Game game, Scene3D scene, Color color, int instances)
            : base(game, scene, ModelContent.GenerateBoundingBox(color), instances)
        {

        }
    }

    public class VolumeSphereInstanced : ModelInstanced
    {
        public VolumeSphereInstanced(Game game, Scene3D scene, uint slices, uint stacks, Color color, int instances)
            : base(game, scene, ModelContent.GenerateBoundingSphere(slices, stacks, color), instances)
        {

        }
    }
}
