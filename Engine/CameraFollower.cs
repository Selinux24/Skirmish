using SharpDX;

namespace Engine
{
    public class CameraFollower : IFollower
    {
        private readonly Manipulator3D manipulator;

        private readonly Vector3 positionOffset = Vector3.Zero;
        private readonly Vector3 viewOffset = Vector3.ForwardLH;

        public Vector3 Position
        {
            get
            {
                return Vector3.TransformCoordinate(this.positionOffset, this.manipulator.LocalTransform);
            }
        }

        public Vector3 Interest
        {
            get
            {
                return this.Position + Vector3.TransformNormal(this.viewOffset, this.manipulator.LocalTransform);
            }
        }

        public CameraFollower(Manipulator3D manipulator)
        {
            this.manipulator = manipulator;
        }

        public CameraFollower(Manipulator3D manipulator, Vector3 position, Vector3 view)
        {
            this.manipulator = manipulator;
            this.positionOffset = position;
            this.viewOffset = view;
        }
    }
}
