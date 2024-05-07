using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Scene cull manager
    /// </summary>
    public class SceneCullManager
    {
        /// <summary>
        /// Culled objects dictionary
        /// </summary>
        protected ConcurrentDictionary<ICullable, List<SceneCullData>> Objects = new();

        /// <summary>
        /// Performs cull test in the object list against the culling volume
        /// </summary>
        /// <param name="cullIndex">Cull index</param>
        /// <param name="volume">Culling volume</param>
        /// <param name="objects">Objects list</param>
        /// <returns>Returns true if any object results inside the volume</returns>
        public bool Cull(int cullIndex, ICullingVolume volume, IEnumerable<ICullable> objects)
        {
            bool res = false;

            foreach (var item in objects)
            {
                var cull = item.Cull(cullIndex, volume, out float distance);
                var cullData = new SceneCullData
                {
                    Culled = cull,
                    Distance = distance,
                };

                SetCullValue(cullData, cullIndex, item, false);

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
        private void SetCullValue(SceneCullData value, int index, ICullable item, bool force)
        {
            var values = Objects.AddOrUpdate(item, [], (k, v) => v);

            int count = values.Count;
            if (count <= index)
            {
                int length = index - count + 1;
                var cullData = new SceneCullData[length];
                values.AddRange(cullData);
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
        public SceneCullData GetCullValue(int index, ICullable item)
        {
            if (Objects.TryGetValue(item, out var values) && index < values.Count)
            {
                return values[index];
            }

            return SceneCullData.Empty;
        }
    }
}
