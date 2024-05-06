using Engine.Common;
using System;

namespace Engine
{
    /// <summary>
    /// Map grid shape Id
    /// </summary>
    struct TerrainGridShapeId : IEquatable<TerrainGridShapeId>
    {
        /// <summary>
        /// Level of detail
        /// </summary>
        public LevelOfDetail LevelOfDetail { get; set; }
        /// <summary>
        /// Shape
        /// </summary>
        public IndexBufferShapes Shape { get; set; }

        /// <summary>
        /// Equal to operator
        /// </summary>
        /// <param name="mgShape1">Shape 1</param>
        /// <param name="mgShape2">Shape 2</param>
        /// <returns>Returns true if both instances are equal</returns>
        public static bool operator ==(TerrainGridShapeId mgShape1, TerrainGridShapeId mgShape2)
        {
            return mgShape1.Equals(mgShape2);
        }
        /// <summary>
        /// Not equal operator
        /// </summary>
        /// <param name="mgShape1">Shape 1</param>
        /// <param name="mgShape2">Shape 2</param>
        /// <returns>Returns true if both instances are different</returns>
        public static bool operator !=(TerrainGridShapeId mgShape1, TerrainGridShapeId mgShape2)
        {
            return !(mgShape1.Equals(mgShape2));
        }
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type
        /// </summary>
        /// <param name="other">An object to compare with this object</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
        public readonly bool Equals(TerrainGridShapeId other)
        {
            if (LevelOfDetail == other.LevelOfDetail && Shape == other.Shape)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type
        /// </summary>
        /// <param name="other">An object to compare with this object</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
        public override readonly bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is TerrainGridShapeId shape)
            {
                return Equals(shape);
            }

            return false;
        }
        /// <summary>
        /// Serves as the default hash function
        /// </summary>
        /// <returns>A hash code for the current object</returns>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(LevelOfDetail, Shape);
        }
    }
}
