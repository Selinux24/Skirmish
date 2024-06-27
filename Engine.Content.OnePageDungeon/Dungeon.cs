using SharpDX;
using System.Collections.Generic;

namespace Engine.Content.OnePageDungeon
{
    /// <summary>
    /// Dungeon descriptor
    /// </summary>
    public class Dungeon
    {
        /// <summary>
        /// Loads a dungeon from the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static Dungeon Load(string fileName)
        {
            return SerializationHelper.DeserializeJsonFromFile<Dungeon>(fileName);
        }
        /// <summary>
        /// Saves a dungeon to the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="dungeon">Dungeon to save</param>
        public static void Save(string fileName, Dungeon dungeon)
        {
            SerializationHelper.SerializeJsonToFile(dungeon, fileName);
        }

        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Story
        /// </summary>
        public string Story { get; set; }
        /// <summary>
        /// List of chambers
        /// </summary>
        public IEnumerable<Rect> Rects { get; set; }
        /// <summary>
        /// List of doors
        /// </summary>
        public IEnumerable<Door> Doors { get; set; }
        /// <summary>
        /// List of notes
        /// </summary>
        public IEnumerable<Note> Notes { get; set; }
        /// <summary>
        /// List of columns
        /// </summary>
        public IEnumerable<Column> Columns { get; set; }
        /// <summary>
        /// List of water blocks
        /// </summary>
        public IEnumerable<Water> Water { get; set; }
    }

    /// <summary>
    /// Vector
    /// </summary>
    public struct Vector
    {
        /// <summary>
        /// X position
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y position
        /// </summary>
        public float Y { get; set; }
    }

    /// <summary>
    /// Chamber
    /// </summary>
    public class Rect
    {
        /// <summary>
        /// X position
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y position
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public float W { get; set; }
        /// <summary>
        /// Height (depth)
        /// </summary>
        public float H { get; set; }
        /// <summary>
        /// The chamber is a rotunda
        /// </summary>
        public bool Rotunda { get; set; }

        /// <summary>
        /// Gets the rectangle
        /// </summary>
        public RectangleF GetRectangle()
        {
            return new RectangleF(X, Y, W, H);
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"X: {X}; Y: {Y}; Width: {W}; Height: {H};";
        }
    }

    /// <summary>
    /// Door
    /// </summary>
    public class Door
    {
        /// <summary>
        /// X position
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y position
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// Door direction
        /// </summary>
        public Vector Dir { get; set; }
        /// <summary>
        /// Door type
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets the door type enumeration value
        /// </summary>
        public DoorTypes GetDoorType()
        {
            return (DoorTypes)Type;
        }
    }

    /// <summary>
    /// Note
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Note text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Reference
        /// </summary>
        public string Ref { get; set; }
        /// <summary>
        /// Note position
        /// </summary>
        public Vector Pos { get; set; }
    }

    /// <summary>
    /// Column
    /// </summary>
    public class Column
    {
        /// <summary>
        /// X position
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y position
        /// </summary>
        public float Y { get; set; }
    }

    /// <summary>
    /// Water
    /// </summary>
    public class Water
    {
        /// <summary>
        /// X position
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y position
        /// </summary>
        public float Y { get; set; }
    }
}
