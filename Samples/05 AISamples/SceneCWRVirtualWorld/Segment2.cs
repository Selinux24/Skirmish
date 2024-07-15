using SharpDX;
using System;

namespace AISamples.SceneCWRVirtualWorld
{
    struct Segment2(Vector2 p1, Vector2 p2) : IEquatable<Segment2>
    {
        public Vector2 P1 { get; set; } = p1;
        public Vector2 P2 { get; set; } = p2;

        public static bool operator ==(Segment2 left, Segment2 right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Segment2 left, Segment2 right)
        {
            return !left.Equals(right);
        }

        public readonly bool Equals(Segment2 other)
        {
            return
                (P1 == other.P1 && P2 == other.P2) ||
                (P1 == other.P2 && P2 == other.P1);
        }
        public override readonly bool Equals(object obj)
        {
            return obj is Segment2 segment && Equals(segment);
        }
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(P1, P2);
        }
    }
}
