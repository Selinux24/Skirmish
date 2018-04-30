using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Scenery levels file
    /// </summary>
    [Serializable]
    public class ModularSceneryLevels
    {
        /// <summary>
        /// Volume meshes masks
        /// </summary>
        [XmlArray("volumes")]
        [XmlArrayItem("mask", typeof(string))]
        public string[] Volumes = null;
        /// <summary>
        /// Particle systems
        /// </summary>
        [XmlArray("particles")]
        [XmlArrayItem("system", typeof(ParticleSystemDescription))]
        public ParticleSystemDescription[] ParticleSystems = null;
        /// <summary>
        /// Levels
        /// </summary>
        [XmlArray("levels")]
        [XmlArrayItem("level", typeof(ModularSceneryLevel))]
        public ModularSceneryLevel[] Levels = null;

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

            return null;
        }
    }
}
