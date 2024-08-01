using AISamples.SceneCWRVirtualWorld.Markings;
using System;

namespace AISamples.SceneCWRVirtualWorld.Content
{
    struct LightFile : IMarkingFile
    {
        public string Type { get; set; }
        public Vector2File Position { get; set; }
        public Vector2File Direction { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Is3D { get; set; }
        public LightState LightState { get; set; }
        public float RedDuration { get; set; }
        public float YellowDuration { get; set; }
        public float GreenDuration { get; set; }

        public readonly Marking FromMarkingFile()
        {
            if (Type != nameof(Light))
            {
                throw new ArgumentException("Invalid marking file type");
            }

            return new Light(Vector2File.FromVector2File(Position), Vector2File.FromVector2File(Direction), Width, Height)
            {
                LightState = LightState,
                RedDuration = RedDuration,
                YellowDuration = YellowDuration,
                GreenDuration = GreenDuration
            };
        }
    }
}
