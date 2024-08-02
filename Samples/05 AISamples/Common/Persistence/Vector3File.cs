using SharpDX;

namespace AISamples.Common.Persistence
{
    struct Vector3File
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3File FromVector3(Vector3 vector)
        {
            return new()
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z
            };
        }

        public static Vector3 FromVector3File(Vector3File vector)
        {
            return new(vector.X, vector.Y, vector.Z);
        }
    }
}
