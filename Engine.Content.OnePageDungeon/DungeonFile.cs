
namespace Engine.Content.OnePageDungeon
{
    /// <summary>
    /// Dugeon file helper
    /// </summary>
    public static class DungeonFile
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
    }
}
