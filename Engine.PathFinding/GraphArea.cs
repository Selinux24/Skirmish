using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area base class
    /// </summary>
    public abstract class GraphArea : IGraphArea
    {
        /// <summary>
        /// Id counter
        /// </summary>
        private static int ID = 1;
        /// <summary>
        /// Gets the next id
        /// </summary>
        /// <returns>Returns the next id</returns>
        private static int GetNextId()
        {
            return ID++;
        }

        /// <summary>
        /// Area type
        /// </summary>
        private int areaType;

        /// <inheritdoc/>
        public int Id { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected GraphArea()
        {
            Id = GetNextId();
        }

        /// <inheritdoc/>
        public int GetAreaType()
        {
            return areaType;
        }
        /// <inheritdoc/>
        public T GetAreaType<T>() where T : Enum
        {
            return (T)(object)areaType;
        }
        /// <inheritdoc/>
        public void SetAreaType(int area)
        {
            areaType = area;
        }
        /// <inheritdoc/>
        public void SetAreaType<T>(T area) where T : Enum
        {
            areaType = (int)(object)area;
        }

        /// <inheritdoc/>
        public abstract BoundingBox GetBounds();
    }
}
