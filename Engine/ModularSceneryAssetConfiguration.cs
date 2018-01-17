using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Scenery assets file configuration
    /// </summary>
    [Serializable]
    public class ModularSceneryAssetConfiguration
    {
        /// <summary>
        /// Complex assets configuration
        /// </summary>
        [XmlArray("assets")]
        [XmlArrayItem("asset", typeof(ModularSceneryAssetDescription))]
        public ModularSceneryAssetDescription[] Assets = null;
        /// <summary>
        /// Particle systems
        /// </summary>
        [XmlArray("particles")]
        [XmlArrayItem("system", typeof(ParticleSystemDescription))]
        public ParticleSystemDescription[] ParticleSystems = null;
        /// <summary>
        /// Assets map
        /// </summary>
        [XmlArray("map")]
        [XmlArrayItem("item", typeof(ModularSceneryAssetReference))]
        public ModularSceneryAssetReference[] Map = null;
        /// <summary>
        /// Maintain texture direction for ceilings and floors, avoiding asset map rotations
        /// </summary>
        [XmlAttribute("maintain_texture_direction")]
        public bool MaintainTextureDirection = true;
        /// <summary>
        /// Map objects
        /// </summary>
        [XmlArray("objects")]
        [XmlArrayItem("item", typeof(ModularSceneryObjectReference))]
        public ModularSceneryObjectReference[] Objects = null;
        /// <summary>
        /// Volume meshes masks
        /// </summary>
        [XmlArray("volumes")]
        [XmlArrayItem("mask", typeof(string))]
        public string[] Volumes = null;

        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, int> GetMapInstanceCounters()
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            foreach (var item in this.Map)
            {
                var asset = this.Assets
                    .Where(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                if (asset != null)
                {
                    var assetInstances = asset.GetInstanceCounters();
                    foreach (var key in assetInstances.Keys)
                    {
                        if (!res.ContainsKey(key))
                        {
                            res.Add(key, 0);
                        }

                        res[key] += assetInstances[key];
                    }
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, int> GetObjectsInstanceCounters()
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            foreach (var item in this.Objects)
            {
                if (!res.ContainsKey(item.AssetName))
                {
                    res.Add(item.AssetName, 0);
                }

                res[item.AssetName] += 1;
            }

            return res;
        }
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
