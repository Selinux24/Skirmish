using AISamples.SceneCWRVirtualWorld.Markings;
using System;

namespace AISamples.SceneCWRVirtualWorld.Content
{
    struct ParkingFile : IMarkingFile
    {
        public string Type { get; set; }
        public Vector2File Position { get; set; }
        public Vector2File Direction { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Is3D { get; set; }

        public readonly Marking FromMarkingFile()
        {
            if (Type != nameof(Parking))
            {
                throw new ArgumentException("Invalid marking file type");
            }

            return new Parking(
                Vector2File.FromVector2File(Position),
                Vector2File.FromVector2File(Direction),
                Width,
                Height);
        }
    }
}
