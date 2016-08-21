using SharpDX;

namespace Engine
{
    public interface IFollower
    {
        Vector3 Position { get; }

        Vector3 Interest { get; }
    }
}
