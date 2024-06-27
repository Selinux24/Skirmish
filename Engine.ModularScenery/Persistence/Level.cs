using Engine.Content;
using System;
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
        /// Objects auto identifier counter
        /// </summary>
        private static int ObjectsAutoId = 1;
        /// <summary>
        /// Objects base string for identification build
        /// </summary>
        private static readonly string ObjectsAutoString = "__objauto__";

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

        /// <summary>
        /// Gets the next Id
        /// </summary>
        /// <returns>Returns the next Id</returns>
        private static int GetNextObjectId()
        {
            return ++ObjectsAutoId;
        }
        /// <summary>
        /// Gets the instance count dictionary
        /// </summary>
        /// <param name="asset">Asset</param>
        /// <returns>Returns a dictionary that contains the instance count by asset name</returns>
        private static Dictionary<string, (int Count, PathFindingModes PathFinding)> GetInstanceCounters(Asset asset)
        {
            Dictionary<string, (int, PathFindingModes)> res = new();

            var assetNames = asset.References.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assetNames)
            {
                var count = asset.References.Count(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase));
                if (count > 0)
                {
                    var pf = asset.References.First(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase)).PathFinding;

                    res.Add(assetName, (count, pf));
                }
            }

            return res;
        }

        /// <summary>
        /// Finds the asset reference by asset map id and asset id
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <param name="levelAssetId">Asset reference id in the level asset reference list</param>
        /// <param name="mapAssetId">Asset id in the asset map reference list</param>
        /// <returns>Returns the asset reference</returns>
        public AssetReference FindAssetReference(IEnumerable<Asset> assets, string levelAssetId, string mapAssetId)
        {
            var res = assets
                //Search any asset which contains a reference with the specified level, by asset name or by the level asset map id
                .Where(a => Map.Any(r =>
                    string.Equals(r.AssetName, a.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.Id, levelAssetId, StringComparison.OrdinalIgnoreCase)));
            var res2 = res
                //Then, get the first reference which matching reference Id with the level asset id
                .Select(a => a.References.FirstOrDefault(r => string.Equals(r.Id, mapAssetId, StringComparison.OrdinalIgnoreCase)));
            var res3 = res2
                .FirstOrDefault();

            return res3;
        }
        /// <summary>
        /// Gets the first index of the asset in the current configuration
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <param name="assetName">Asset name</param>
        /// <param name="levelAssetId">Asset reference id in the level asset reference list</param>
        /// <param name="mapAssetId">Asset id in the asset map reference list</param>
        /// <returns>Returns the first index</returns>
        public int GetMapInstanceIndex(IEnumerable<Asset> assets, string assetName, string levelAssetId, string mapAssetId)
        {
            int index = 0;

            foreach (var item in Map)
            {
                var asset = assets
                    .FirstOrDefault(a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase));

                if (asset == null)
                {
                    continue;
                }

                foreach (var a in asset.References.Where(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (string.Equals(item.Id, levelAssetId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(a.Id, mapAssetId, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }

                    index++;
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

            foreach (var item in Objects)
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
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <param name="assets">Asset list</param>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, InstanceInfo> GetMapInstanceCounters(IEnumerable<Asset> assets)
        {
            Dictionary<string, InstanceInfo> res = new();

            var vAssets = assets.ToArray();

            foreach (var item in Map)
            {
                var asset = Array.Find(vAssets, a => string.Equals(a.Name, item.AssetName, StringComparison.OrdinalIgnoreCase));
                if (asset == null)
                {
                    continue;
                }

                var assetInstances = GetInstanceCounters(asset);
                foreach (var key in assetInstances.Keys)
                {
                    var (Count, PathFinding) = assetInstances[key];

                    if (!res.TryGetValue(key, out var value))
                    {
                        value = new() { Count = Count, PathFinding = PathFinding };
                        res.Add(key, value);

                        continue;
                    }

                    value.Count += Count;
                }
            }

            return res;
        }

        /// <summary>
        /// Populate objects empty ids
        /// </summary>
        public void PopulateObjectIds()
        {
            if (Objects?.Any() != true)
            {
                return;
            }

            foreach (var obj in Objects)
            {
                if (!string.IsNullOrEmpty(obj.Id))
                {
                    continue;
                }

                obj.Id = $"{ObjectsAutoString}_{GetNextObjectId()}";
            }
        }
        /// <summary>
        /// Gets the instance counter dictionary
        /// </summary>
        /// <returns>Returns a dictionary with the instance count by unique asset name</returns>
        public Dictionary<string, InstanceInfo> GetObjectsInstanceCounters()
        {
            Dictionary<string, InstanceInfo> res = new();

            foreach (var item in Objects)
            {
                if (string.IsNullOrEmpty(item.AssetName))
                {
                    continue;
                }

                if (!res.TryGetValue(item.AssetName, out var value))
                {
                    value = new() { Count = 0, PathFinding = item.PathFinding };
                    res.Add(item.AssetName, value);
                }

                value.Count += 1;
            }

            return res;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {Map?.Count() ?? 0} rooms. {Objects?.Count() ?? 0} items. Start: {StartPosition} => {LookingVector}";
        }
    }
}
