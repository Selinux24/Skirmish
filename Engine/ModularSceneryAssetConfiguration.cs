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
        /// Gets the instance counter dictionary
        /// </summary>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, int> GetInstanceCounter()
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            foreach (var item in Map)
            {
                var asset = this.Assets
                    .Where(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                if (asset != null)
                {
                    var assetInstanceCount = asset.GetInstanceCounter();
                    foreach (var key in assetInstanceCount.Keys)
                    {
                        if (!res.ContainsKey(key))
                        {
                            res.Add(key, 0);
                        }

                        res[key] += assetInstanceCount[key];
                    }
                }
            }

            return res;
        }
    }
}
