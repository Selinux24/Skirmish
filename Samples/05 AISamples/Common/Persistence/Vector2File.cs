using SharpDX;

namespace AISamples.Common.Persistence
{
    struct Vector2File
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Vector2File FromVector2(Vector2 vector)
        {
            return new()
            {
                X = vector.X,
                Y = vector.Y,
            };
        }

        public static Vector2 FromVector2File(Vector2File vector)
        {
            return new(vector.X, vector.Y);
        }
    }
}
