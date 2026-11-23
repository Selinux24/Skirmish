using Engine.Mathematics;
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
        public AssetReference[] References { get; set; } = [];
        /// <summary>
        /// Connections list
        /// </summary>
        public AssetConnection[] Connections { get; set; } = [];

        /// <summary>
        /// Gets the instance transforms dictionary
        /// </summary>
        /// <returns>Returns a dictionary that contains the instance transform list by asset name</returns>
        public Dictionary<string, Matrix4x4[]> GetInstanceTransforms()
        {
            Dictionary<string, Matrix4x4[]> res = [];

            var assetNames = References.Select(a => a.AssetName).Distinct();

            foreach (var assetName in assetNames)
            {
                var refs = Array.FindAll(References, a => string.Equals(a.AssetName, assetName, StringComparison.OrdinalIgnoreCase));
                if (refs.Length == 0) continue;

                var transforms = new Matrix4x4[refs.Length];
                int idx = 0;
                Array.ForEach(refs, a => transforms[idx++] = Matrix4x4.Transformation(a.Position, a.Rotation, a.Scale));

                res.Add(assetName, transforms);
            }

            return res;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Name}. {References?.Length ?? 0} parts. {Connections?.Length ?? 0} connections.";
        }
    }
}
