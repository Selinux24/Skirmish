using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Graph connection
    /// </summary>
    public class GraphConnection : IGraphConnection
    {
        /// <summary>
        /// Id Counter
        /// </summary>
        private static int ID = 1000;
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
        /// <summary>
        /// Flag type
        /// </summary>
        private int flagType;

        /// <inheritdoc/>
        public int Id { get; private set; }
        /// <inheritdoc/>
        public Vector3 Start { get; set; }
        /// <inheritdoc/>
        public Vector3 End { get; set; }
        /// <inheritdoc/>
        public float Radius { get; set; }
        /// <inheritdoc/>
        public bool BiDirectional { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GraphConnection()
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
        public int GetFlagType()
        {
            return flagType;
        }
        /// <inheritdoc/>
        public T GetFlagType<T>() where T : Enum
        {
            return (T)(object)flagType;
        }
        /// <inheritdoc/>
        public void SetFlagType(int flag)
        {
            flagType = flag;
        }
        /// <inheritdoc/>
        public void SetFlagType<T>(T flag) where T : Enum
        {
            flagType = (int)(object)flag;
        }
    }
}
