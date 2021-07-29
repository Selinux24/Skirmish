using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    using Engine.Content.Persistence;

    /// <summary>
    /// Scenery level
    /// </summary>
    public class ModularSceneryLevel
    {
        /// <summary>
        /// Objects auto identifier counter
        /// </summary>
        private static int ObjectsAutoId = 1;
        /// <summary>
        /// Objects base string for identification build
        /// </summary>
        private static readonly string ObjectsAutoString = "__objauto__";
        /// <summary>
        /// Gets the next Id
        /// </summary>
        /// <returns>Returns the next Id</returns>
        private static int GetNextId()
        {
            return ++ObjectsAutoId;
        }

        /// <summary>
        /// Level name
        /// </summary>
        public string Name { get; set; } = null;
        /// <summary>
        /// Position
        /// </summary>
        public Position3 StartPosition { get; set; } = Position3.Zero;
        /// <summary>
        /// Looking vector
        /// </summary>
        public Direction3 LookingVector { get; set; } = Vector3.ForwardLH;
        /// <summary>
        /// Assets map
        /// </summary>
        public ModularSceneryAssetReference[] Map { get; set; } = new ModularSceneryAssetReference[] { };
        /// <summary>
        /// Map objects
        /// </summary>
        public ModularSceneryObjectReference[] Objects { get; set; } = new ModularSceneryObjectReference[] { };

        /// <summary>
        /// Populate objects empty ids
        /// </summary>
        public void PopulateObjectIds()
        {
            foreach (var item in Objects)
            {
                if (string.IsNullOrEmpty(item.Id))
                {
                    item.Id = string.Format("{0}_{1}", ObjectsAutoString, GetNextId());
                }
            }
        }
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, ModularSceneryAssetInstanceInfo> GetMapInstanceCounters(ModularSceneryAsset[] assets)
        {
            Dictionary<string, ModularSceneryAssetInstanceInfo> res = new Dictionary<string, ModularSceneryAssetInstanceInfo>();

            foreach (var item in this.Map)
            {
                var asset = assets
                    .FirstOrDefault(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase));

                if (asset != null)
                {
                    var assetInstances = asset.GetInstanceCounters();
                    foreach (var key in assetInstances.Keys)
                    {
                        if (!res.ContainsKey(key))
                        {
                            res.Add(key, new ModularSceneryAssetInstanceInfo { Count = 0 });
                        }

                        res[key].Count += assetInstances[key];
                    }
                }
            }

            return res;
        }
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, ModularSceneryObjectInstanceInfo> GetObjectsInstanceCounters()
        {
            Dictionary<string, ModularSceneryObjectInstanceInfo> res = new Dictionary<string, ModularSceneryObjectInstanceInfo>();

            foreach (var item in this.Objects)
            {
                if (string.IsNullOrEmpty(item.AssetName))
                {
                    continue;
                }

                if (!res.ContainsKey(item.AssetName))
                {
                    res.Add(item.AssetName, new ModularSceneryObjectInstanceInfo { Count = 0, PathFinding = item.PathFinding });
                }

                res[item.AssetName].Count += 1;
            }

            return res;
        }

        /// <summary>
        /// Finds the asset reference by asset map id and asset id
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <param name="mapId">Asset map id</param>
        /// <param name="id">Asset id</param>
        /// <returns>Returns the asset reference</returns>
        public ModularSceneryAssetReference FindAssetInstance(ModularSceneryAsset[] assets, string mapId, string id)
        {
            var res = assets
                .Where(a => this.Map.Any(m =>
                    string.Equals(m.Id, mapId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.AssetName, a.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(a => a.References.FirstOrDefault(d => string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

            return res;
        }
        /// <summary>
        /// Gets the first index of the asset in the current configuration
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <param name="assetName">Asset name</param>
        /// <param name="assetMapId">Asset map id</param>
        /// <param name="assetId">Asset id</param>
        /// <returns>Returns the first index</returns>
        public int GetMapInstanceIndex(ModularSceneryAsset[] assets, string assetName, string assetMapId, string assetId)
        {
            int index = 0;

            foreach (var item in this.Map)
            {
                var asset = assets
                    .FirstOrDefault(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase));

                if (asset == null)
                {
                    continue;
                }

                foreach (var a in asset.References)
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

    /// <summary>
    /// Asset instance info
    /// </summary>
    public class ModularSceneryAssetInstanceInfo
    {
        /// <summary>
        /// Instance count
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Object instance info
    /// </summary>
    public class ModularSceneryObjectInstanceInfo
    {
        /// <summary>
        /// Instance count
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Use of path finding
        /// </summary>
        public bool PathFinding { get; set; }
    }
}
