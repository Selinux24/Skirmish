using SharpDX;

namespace Engine.Physics.GJKEPA
{
    public interface ISupportMappable
    {
        void SupportMapping(Vector3 direction, out Vector3 result);
    }
}
