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
        /// Culled objects dictionary
        /// </summary>
        protected Dictionary<ICullable, List<bool>> Objects = new Dictionary<ICullable, List<bool>>();

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
                var value = item.Cull(frustum);

                this.SetCullValue(value, index, item);

                if (!value) res = true;
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
                var value = item.Cull(bbox);

                this.SetCullValue(value, index, item);

                if (!value) res = true;
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
                var value = item.Cull(bsph);

                this.SetCullValue(value, index, item);

                if (!value) res = true;
            }

            return res;
        }
        /// <summary>
        /// Sets the specified cull value to the object list at results index
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="index">Results index</param>
        /// <param name="objects">Object list</param>
        public void SetCullValue(bool value, int index, IEnumerable<ICullable> objects)
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
        public void SetCullValue(bool value, int index, ICullable item)
        {
            if (!this.Objects.ContainsKey(item))
            {
                this.Objects.Add(item, new List<bool>(index + 1));
            }

            var values = this.Objects[item];

            if (values.Count <= index)
            {
                var valuesToAdd = new bool[index - values.Count + 1];

                values.AddRange(valuesToAdd);
            }

            values[index] = value;
        }
        /// <summary>
        /// Gets if the specified object is visible in the results index
        /// </summary>
        /// <param name="index">Results index</param>
        /// <param name="item">Object</param>
        /// <returns>Returns true if the especified object is visible at index</returns>
        public bool IsVisible(int index, ICullable item)
        {
            if (this.Objects.ContainsKey(item))
            {
                var values = this.Objects[item];

                if (index < values.Count)
                {
                    return values[index];
                }
            }

            return false;
        }
    }
}
