using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Complex asset
    /// </summary>
    [Serializable]
    public class ModularSceneryAssetDescription
    {
        /// <summary>
        /// Asset name
        /// </summary>
        [XmlElement("name")]
        public string Name;
        /// <summary>
        /// Assets list
        /// </summary>
        [XmlArray("references")]
        [XmlArrayItem("asset", typeof(ModularSceneryAssetReference))]
        public ModularSceneryAssetReference[] Assets = null;

        /// <summary>
        /// Gets the instance count dictionary
        /// </summary>
        /// <returns>Returns a dictionary that contains the instance count by asset name</returns>
        public Dictionary<string, int> GetInstanceCounters()
        {
            Dictionary<string, int> res = new Dictionary<string, int>();

            var assets = this.Assets.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assets)
            {
                var count = this.Assets.Count(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase));
                if (count > 0)
                {
                    res.Add(assetName, count);
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the instance transforms dictionary
        /// </summary>
        /// <returns>Returns a dictionary that contains the instance transform list by asset name</returns>
        public Dictionary<string, Matrix[]> GetInstanceTransforms()
        {
            Dictionary<string, Matrix[]> res = new Dictionary<string, Matrix[]>();

            var assets = this.Assets.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assets)
            {
                var transforms = this.Assets
                    .FindAll(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.GetTransform()).ToArray();

                res.Add(assetName, transforms);
            }

            return res;
        }
    }
}
