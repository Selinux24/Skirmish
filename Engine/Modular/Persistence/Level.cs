using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Scenery level
    /// </summary>
    /// <remarks>
    /// Defines the assets (rooms, corridors, stairs, etc.) of the level, and their objects to interact.
    /// </remarks>
    public class Level
    {
        /// <summary>
        /// Level name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Position
        /// </summary>
        public Position3 StartPosition { get; set; } = Position3.Zero;
        /// <summary>
        /// Looking vector
        /// </summary>
        public Direction3 LookingVector { get; set; } = Direction3.ForwardLH;
        /// <summary>
        /// Assets map
        /// </summary>
        public IEnumerable<AssetReference> Map { get; set; } = Enumerable.Empty<AssetReference>();
        /// <summary>
        /// Map objects
        /// </summary>
        public IEnumerable<ObjectReference> Objects { get; set; } = Enumerable.Empty<ObjectReference>();
    }
}
