using SharpDX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A set of contours around the regions of a <see cref="CompactHeightField"/>, used as the edges of a
    /// <see cref="PolyMesh"/>.
    /// </summary>
    public class ContourSet : ICollection<Contour>
    {
        /// <summary>
        /// Internal list
        /// </summary>
        private List<Contour> contours;

        /// <summary>
        /// Gets the world-space bounding box of the set.
        /// </summary>
        public BoundingBox Bounds { get; private set; }
        /// <summary>
        /// Gets the width of the set, not including the border size specified in <see cref="CompactHeightField"/>.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the height of the set, not including the border size specified in <see cref="CompactHeightField"/>.
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the <see cref="ContourSet"/> is read-only.
        /// </summary>
        bool ICollection<Contour>.IsReadOnly
        {
            get { return true; }
        }
        /// <summary>
        /// Gets the number of <see cref="Contour"/>s in the set.
        /// </summary>
        public int Count
        {
            get
            {
                return contours.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContourSet"/> class.
        /// </summary>
        /// <param name="contours">A collection of <see cref="Contour"/>s.</param>
        /// <param name="bounds">The bounding box that contains all of the <see cref="Contour"/>s.</param>
        /// <param name="width">The width, in voxel units, of the world.</param>
        /// <param name="height">The height, in voxel units, of the world.</param>
        public ContourSet(IEnumerable<Contour> contours, BoundingBox bounds, int width, int height)
        {
            this.contours = contours.ToList();
            this.Bounds = bounds;
            this.Width = width;
            this.Height = height;

            //prevent null contours from ever being added to the set.
            this.contours.RemoveAll(c => c.IsNull);
        }

        /// <summary>
        /// Calculates the maximum number of vertices, triangles, and vertices per contour in the
        /// set of contours.
        /// </summary>
        /// <param name="maxVertices">The maximum number of vertices possible from this contour set.</param>
        /// <param name="maxTris">The maximum number of triangles possible from this contour set.</param>
        /// <param name="maxVertsPerContour">The maximum number of vertices per contour within the set.</param>
        public void GetVertexLimits(out int maxVertices, out int maxTris, out int maxVertsPerContour)
        {
            //TODO refactor name of function?
            maxVertices = 0;
            maxTris = 0;
            maxVertsPerContour = 0;

            foreach (var c in this.contours)
            {
                int vertCount = c.Vertices.Length;

                maxVertices += vertCount;
                maxTris += vertCount - 2;
                maxVertsPerContour = Math.Max(maxVertsPerContour, vertCount);
            }
        }
        /// <summary>
        /// Add a new contour to the set
        /// </summary>
        /// <param name="item">The contour to add</param>
        public void Add(Contour item)
        {
            if (item != null && !item.IsNull)
            {
                this.contours.Add(item);
            }
        }
        /// <summary>
        /// Clear the set of contours.
        /// </summary>
        public void Clear()
        {
            this.contours.Clear();
        }
        /// <summary>
        /// Checks if a specified <see cref="ContourSet"/> is contained in the <see cref="ContourSet"/>.
        /// </summary>
        /// <param name="item">A contour.</param>
        /// <returns>A value indicating whether the set contains the specified contour.</returns>
        public bool Contains(Contour item)
        {
            return this.contours.Contains(item);
        }
        /// <summary>
        /// Copies the <see cref="Contour"/>s in the set to an array.
        /// </summary>
        /// <param name="array">The array to copy to.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Contour[] array, int arrayIndex)
        {
            this.contours.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the entire <see cref="ContourSet"/>.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<Contour> GetEnumerator()
        {
            return this.contours.GetEnumerator();
        }
        /// <summary>
        /// (Not implemented) Remove a contour from the set
        /// </summary>
        /// <param name="item">The contour to remove</param>
        /// <returns>throw InvalidOperatorException</returns>
        bool ICollection<Contour>.Remove(Contour item)
        {
            return this.contours.Remove(item);
        }
        /// <summary>
        /// Gets an enumerator that iterates through the set
        /// </summary>
        /// <returns>The enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
