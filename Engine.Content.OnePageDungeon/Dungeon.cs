using SharpDX;
using System.Collections.Generic;

namespace Engine.Content.OnePageDungeon
{
    public class Dungeon
    {
        public string Version { get; set; }
        public string Title { get; set; }
        public string Story { get; set; }
        public IEnumerable<Rect> Rects { get; set; }
        public IEnumerable<Door> Doors { get; set; }
        public IEnumerable<Note> Notes { get; set; }
        public IEnumerable<Column> Columns { get; set; }
        public IEnumerable<Water> Water { get; set; }
    }

    public struct Vector
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Rect
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float W { get; set; }
        public float H { get; set; }

        public RectangleF GetRectangle()
        {
            return new RectangleF(X, Y, W, H);
        }

        public override string ToString()
        {
            return $"X: {X}; Y: {Y}; Width: {W}; Height: {H};";
        }
    }

    public class Door
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vector Dir { get; set; }
        public int Type { get; set; }
    }

    public class Note
    {
        public string Text { get; set; }
        public string Ref { get; set; }
        public Vector Pos { get; set; }
    }

    public class Column
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Water
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}
