using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Scene cull manager
    /// </summary>
    public class SceneCullManager
    {
        /// <summary>
        /// Cull data
        /// </summary>
        public struct CullData
        {
            /// <summary>
            /// Empty cull data
            /// </summary>
            public static CullData Empty
            {
                get
                {
                    return new CullData()
                    {
                        Culled = false,
                        Distance = float.MaxValue,
                    };
                }
            }

            /// <summary>
            /// Cull flag. If true, the item is culled
            /// </summary>
            public bool Culled { get; set; }
            /// <summary>
            /// Distance from point of view when the item is'nt culled
            /// </summary>
            public float Distance { get; set; }
        }

        /// <summary>
        /// Culled objects dictionary
        /// </summary>
        protected Dictionary<ICullable, List<CullData>> Objects = new Dictionary<ICullable, List<CullData>>();

        /// <summary>
        /// Performs cull test in the object list against the culling volume
        /// </summary>
        /// <param name="volume">Culling volume</param>
        /// <param name="index">Results index</param>
        /// <param name="objects">Objects list</param>
        /// <returns>Returns true if any object results inside the volume</returns>
        public bool Cull(ICullingVolume volume, int index, IEnumerable<ICullable> objects)
        {
            bool res = false;

            foreach (var item in objects)
            {
                var cull = item.Cull(volume, out float distance);
                var cullData = new CullData
                {
                    Culled = cull,
                    Distance = distance,
                };

                SetCullValue(cullData, index, item, false);

                if (!cullData.Culled) res = true;
            }

            return res;
        }
        /// <summary>
        /// Sets the specified cull value to the object at results index
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="index">Results index</param>
        /// <param name="item">Object</param>
        private void SetCullValue(CullData value, int index, ICullable item, bool force)
        {
            if (!Objects.ContainsKey(item))
            {
                Objects.Add(item, new List<CullData>(index + 1));
            }

            var values = Objects[item];

            if (values.Count <= index)
            {
                var valuesToAdd = new CullData[index - values.Count + 1];

                values.AddRange(valuesToAdd);
            }

            if (force)
            {
                // Culled values stay untouched
                if (values[index].Culled)
                {
                    values[index] = value;
                }
            }
            else
            {
                values[index] = value;
            }
        }
        /// <summary>
        /// Gets the specified object cull data in the results index
        /// </summary>
        /// <param name="index">Results index</param>
        /// <param name="item">Object</param>
        /// <returns>Returns the cull data item for the specified object and index. If not exists, returns the Empty cull data object</returns>
        public CullData GetCullValue(int index, ICullable item)
        {
            if (Objects.TryGetValue(item, out var values) && index < values.Count)
            {
                return values[index];
            }

            return CullData.Empty;
        }
    }
}
