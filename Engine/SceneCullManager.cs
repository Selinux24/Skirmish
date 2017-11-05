using SharpDX;
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
            public bool Culled;
            /// <summary>
            /// Distance from point of view when the item is'nt culled
            /// </summary>
            public float? Distance;
        }

        /// <summary>
        /// Culled objects dictionary
        /// </summary>
        protected Dictionary<ICullable, List<CullData>> Objects = new Dictionary<ICullable, List<CullData>>();

        /// <summary>
        /// Performs cull test in the object list against the bounding volume
        /// </summary>
        /// <param name="frustum">Frustum</param>
        /// <param name="index">Results index</param>
        /// <param name="objects">Objects list</param>
        /// <returns>Returns true if any object results visible</returns>
        public bool Cull(BoundingFrustum frustum, int index, IEnumerable<ICullable> objects)
        {
            bool res = false;

            foreach (var item in objects)
            {
                float? distance;
                var cull = item.Cull(frustum, out distance);
                var cullData = new CullData
                {
                    Culled = cull,
                    Distance = distance,
                };

                this.SetCullValue(cullData, index, item);

                if (!cullData.Culled) res = true;
            }

            return res;
        }
        /// <summary>
        /// Performs cull test in the object list against the bounding volume
        /// </summary>
        /// <param name="bbox">Box</param>
        /// <param name="index">Results index</param>
        /// <param name="objects">Objects list</param>
        /// <returns>Returns true if any object results visible</returns>
        public bool Cull(BoundingBox bbox, int index, IEnumerable<ICullable> objects)
        {
            bool res = false;

            foreach (var item in objects)
            {
                float? distance;
                var cull = item.Cull(bbox, out distance);
                var cullData = new CullData
                {
                    Culled = cull,
                    Distance = distance,
                };

                this.SetCullValue(cullData, index, item);

                if (!cullData.Culled) res = true;
            }

            return res;
        }
        /// <summary>
        /// Performs cull test in the object list against the bounding volume
        /// </summary>
        /// <param name="bsph">Sphere</param>
        /// <param name="index">Results index</param>
        /// <param name="objects">Objects list</param>
        /// <returns>Returns true if any object results visible</returns>
        public bool Cull(BoundingSphere bsph, int index, IEnumerable<ICullable> objects)
        {
            bool res = false;

            foreach (var item in objects)
            {
                float? distance;
                var cull = item.Cull(bsph, out distance);
                var cullData = new CullData
                {
                    Culled = cull,
                    Distance = distance,
                };

                this.SetCullValue(cullData, index, item);

                if (!cullData.Culled) res = true;
            }

            return res;
        }
        /// <summary>
        /// Sets the specified cull value to the object list at results index
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="index">Results index</param>
        /// <param name="objects">Object list</param>
        private void SetCullValue(CullData value, int index, IEnumerable<ICullable> objects)
        {
            foreach (var item in objects)
            {
                this.SetCullValue(value, index, item);
            }
        }
        /// <summary>
        /// Sets the specified cull value to the object at results index
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="index">Results index</param>
        /// <param name="item">Object</param>
        private void SetCullValue(CullData value, int index, ICullable item)
        {
            if (!this.Objects.ContainsKey(item))
            {
                this.Objects.Add(item, new List<CullData>(index + 1));
            }

            var values = this.Objects[item];

            if (values.Count <= index)
            {
                var valuesToAdd = new CullData[index - values.Count + 1];

                values.AddRange(valuesToAdd);
            }

            values[index] = value;
        }
        /// <summary>
        /// Gets the specified object cull data in the results index
        /// </summary>
        /// <param name="index">Results index</param>
        /// <param name="item">Object</param>
        /// <returns>Returns the cull data item for the specified object and index. If not exists, returns the Empty cull data object</returns>
        public CullData GetCullValue(int index, ICullable item)
        {
            if (this.Objects.ContainsKey(item))
            {
                var values = this.Objects[item];

                if (index < values.Count)
                {
                    return values[index];
                }
            }

            return CullData.Empty;
        }
    }
}
