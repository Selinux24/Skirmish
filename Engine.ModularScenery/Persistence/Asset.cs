using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Modular.Persistence
{
    /// <summary>
    /// Asset map class
    /// </summary>
    /// <remarks>
    /// Defines a complex asset, like a corridor or a room, with doors and connection points with other complex assets
    /// </remarks>
    public class Asset
    {
        /// <summary>
        /// Asset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Assets list
        /// </summary>
        public IEnumerable<AssetReference> References { get; set; } = Enumerable.Empty<AssetReference>();
        /// <summary>
        /// Connections list
        /// </summary>
        public IEnumerable<AssetConnection> Connections { get; set; } = Enumerable.Empty<AssetConnection>();

        /// <summary>
        /// Gets the instance transforms dictionary
        /// </summary>
        /// <returns>Returns a dictionary that contains the instance transform list by asset name</returns>
        public Dictionary<string, Matrix[]> GetInstanceTransforms()
        {
            Dictionary<string, Matrix[]> res = new();

            var assetNames = References.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assetNames)
            {
                var transforms = References
                    .Where(a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase))
                    .Select(a => GeometryUtil.Transformation(a.Position, a.Rotation, a.Scale))
                    .ToArray();

                res.Add(assetName, transforms);
            }

            return res;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {References?.Count() ?? 0} parts. {Connections?.Count() ?? 0} connections.";
        }
    }
}
