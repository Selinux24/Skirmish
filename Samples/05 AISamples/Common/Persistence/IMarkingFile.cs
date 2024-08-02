using AISamples.Common.Markings;

namespace AISamples.Common.Persistence
{
    interface IMarkingFile
    {
        string Type { get; set; }
        Vector2File Position { get; set; }
        Vector2File Direction { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        bool Is3D { get; set; }

        Marking FromMarkingFile();
    }
}
