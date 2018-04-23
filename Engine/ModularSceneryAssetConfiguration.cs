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
        /// Objects auto identifier counter
        /// </summary>
        private static int ObjectsAutoId = 1;
        /// <summary>
        /// Objects base string for identification build
        /// </summary>
        private static string ObjectsAutoString = "__objauto__";

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
        /// Populate objects empty ids
        /// </summary>
        public void PopulateObjectIds()
        {
            foreach (var item in Objects)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    item.Id = string.Format("{0}_{1}", ObjectsAutoString, ObjectsAutoId++);
                }
            }
        }

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
                if (string.IsNullOrEmpty(item.AssetName))
                {
                    continue;
                }

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

        /// <summary>
        /// Finds the asset reference by asset map id and asset id
        /// </summary>
        /// <param name="mapId">Asset map id</param>
        /// <param name="id">Asset id</param>
        /// <returns>Returns the asset reference</returns>
        public ModularSceneryAssetReference FindAssetInstance(string mapId, string id)
        {
            var res = this.Assets
                .Where(a => this.Map.Count(m =>
                    string.Equals(m.Id, mapId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.AssetName, a.Name, StringComparison.OrdinalIgnoreCase)) > 0)
                .Select(a => a.Assets.Where(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase)).FirstOrDefault())
                .FirstOrDefault();

            return res;
        }
        /// <summary>
        /// Gets the first index of the asset in the current configuration
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="assetMapId">Asset map id</param>
        /// <param name="assetId">Asset id</param>
        /// <returns>Returns the first index</returns>
        public int GetMapInstanceIndex(string assetName, string assetMapId, string assetId)
        {
            int index = 0;

            foreach (var item in this.Map)
            {
                var asset = this.Assets
                    .Where(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                if (asset != null)
                {
                    foreach (var a in asset.Assets)
                    {
                        if (string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.Equals(item.Id, assetMapId, StringComparison.OrdinalIgnoreCase) &&
                                string.Equals(a.Id, assetId, StringComparison.OrdinalIgnoreCase))
                            {
                                return index;
                            }

                            index++;
                        }
                    }
                }
            }

            return -1;
        }
        /// <summary>
        /// Gets the first index of the object in the current configuration
        /// </summary>
        /// <param name="assetName">Asset name</param>
        /// <param name="objectId">Object id</param>
        /// <returns>Returns the first index</returns>
        public int GetObjectInstanceIndex(string assetName, string objectId)
        {
            int index = 0;

            foreach (var item in this.Objects)
            {
                if (string.IsNullOrEmpty(item.AssetName))
                {
                    continue;
                }

                if (string.Equals(item.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(item.Id, objectId, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }

                    index++;
                }
            }

            return -1;
        }
    }
}
