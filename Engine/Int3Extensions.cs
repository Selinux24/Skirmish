using Vortice.Mathematics;

namespace Engine
{
    public static class Int3Extensions
    {
        public static void Add(this Int3 a, Int3 value)
        {
            a.X += value.X;
            a.Y += value.Y;
            a.Z += value.Z;
        }
        public static Int3 Sum(Int3 a, Int3 b)
        {
            return new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }
}
