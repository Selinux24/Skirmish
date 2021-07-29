using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Scenery levels file
    /// </summary>
    public class ModularSceneryLevels
    {
        /// <summary>
        /// Volume meshes masks
        /// </summary>
        public string[] Volumes { get; set; } = null;
        /// <summary>
        /// Particle systems
        /// </summary>
        public ParticleSystemDescription[] ParticleSystems { get; set; } = null;
        /// <summary>
        /// Levels
        /// </summary>
        public ModularSceneryLevel[] Levels { get; set; } = null;

        /// <summary>
        /// Gets a list of masks to find volume meshes for the specified asset name
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <returns>Returns a list of masks to find volume meshes for the specified asset name</returns>
        public IEnumerable<string> GetMasksForAsset(string assetName)
        {
            if (this.Volumes != null && this.Volumes.Length > 0)
            {
                return this.Volumes.Select(v => assetName + v);
            }

            return new string[] { };
        }
    }
}
