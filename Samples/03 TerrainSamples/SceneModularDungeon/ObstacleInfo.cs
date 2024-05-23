using Engine;

namespace TerrainSamples.SceneModularDungeon
{
    /// <summary>
    /// Obstable info
    /// </summary>
    class ObstacleInfo
    {
        /// <summary>
        /// Index
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Model
        /// </summary>
        public ModelInstance Item { get; set; }
        /// <summary>
        /// Bounds
        /// </summary>
        public object Bounds { get; set; }
    }
}
