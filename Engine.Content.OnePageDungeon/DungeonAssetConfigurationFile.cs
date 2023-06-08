
namespace Engine.Content.OnePageDungeon
{
    /// <summary>
    /// Dungeon asset configuration file
    /// </summary>
    public static class DungeonAssetConfigurationFile
    {
        /// <summary>
        /// Loads an asset configuration from the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        public static DungeonAssetConfiguration Load(string fileName)
        {
            return SerializationHelper.DeserializeJsonFromFile<DungeonAssetConfiguration>(fileName);
        }
        /// <summary>
        /// Saves an asset configuration to the specified file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="configuration">Configuration to save</param>
        public static void Save(string fileName, DungeonAssetConfiguration configuration)
        {
            SerializationHelper.SerializeJsonToFile(configuration, fileName);
        }
    }
}
