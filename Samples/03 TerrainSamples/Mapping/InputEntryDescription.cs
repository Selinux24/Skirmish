using Engine;

namespace TerrainSamples.Mapping
{
    /// <summary>
    /// Input entry description
    /// </summary>
    public readonly struct InputEntryDescription
    {
        /// <summary>
        /// Name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Input entry
        /// </summary>
        public readonly object InputEntry;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputEntryDescription(string name, Keys keys)
        {
            Name = name;
            InputEntry = keys;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public InputEntryDescription(string name, MouseButtons btn)
        {
            Name = name;
            InputEntry = btn;
        }
    }
}
