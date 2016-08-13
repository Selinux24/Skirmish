using SharpDX;
using System;
using System.Threading.Tasks;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// A Heightfield represents a "voxel" grid represented as a 2-dimensional grid of <see cref="HeightFieldCell"/>s.
    /// </summary>
    public class HeightField
    {
        /// <summary>
        /// Cells array
        /// </summary>
        private HeightFieldCell[] cells;

        /// <summary>
        /// Gets the number of cells in the X direction.
        /// </summary>
        /// <value>The width.</value>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the number of cells in the Y (up) direction.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; private set; }
        /// <summary>
        /// Gets the number of cells in the Z direction.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the bounding box of the heightfield.
        /// </summary>
        public BoundingBox Bounds { get; private set; }
        /// <summary>
        /// Gets the world-space minimum.
        /// </summary>
        /// <value>The minimum.</value>
        public Vector3 Minimum
        {
            get
            {
                return this.Bounds.Minimum;
            }
        }
        /// <summary>
        /// Gets the world-space maximum.
        /// </summary>
        /// <value>The maximum.</value>
        public Vector3 Maximum
        {
            get
            {
                return this.Bounds.Maximum;
            }
        }

        /// <summary>
        /// Gets the size of a cell on the X and Z axes.
        /// </summary>
        public float CellSizeXZ { get; private set; }
        /// <summary>
        /// Gets the size of a cell on the Y axis.
        /// </summary>
        public float CellHeight { get; private set; }
        /// <summary>
        /// Gets the size of a cell (voxel).
        /// </summary>
        /// <value>The size of the cell.</value>
        public Vector3 CellSize
        {
            get
            {
                return new Vector3(this.CellSizeXZ, this.CellHeight, this.CellSizeXZ);
            }
        }

        /// <summary>
        /// Gets the total number of spans.
        /// </summary>
        public int SpanCount
        {
            get
            {
                int count = 0;

                if (this.cells != null && this.cells.Length > 0)
                {
                    Array.ForEach(this.cells, c => count += c.WalkableSpanCount);
                }

                return count;
            }
        }
        /// <summary>
        /// Gets the <see cref="HeightFieldCell"/> at the specified coordinate.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The cell at [x, y].</returns>
        public HeightFieldCell this[int x, int y]
        {
            get
            {
                return this.cells[y * this.Width + x];
            }
        }
        /// <summary>
        /// Gets the <see cref="HeightFieldCell"/> at the specified index.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The cell at index i.</returns>
        public HeightFieldCell this[int i]
        {
            get
            {
                return this.cells[i];
            }
        }

        /// <summary>
        /// Gets the <see cref="HeightFieldSpan"/> at the reference.
        /// </summary>
        /// <param name="spanRef">A reference to a span.</param>
        /// <returns>The span at the reference.</returns>
        public HeightFieldSpan this[HeightFieldSpanReference spanRef]
        {
            get
            {
                return cells[spanRef.Y * this.Width + spanRef.X].Spans[spanRef.Index];
            }
        }

        /// <summary>
        /// Builds a new height field
        /// </summary>
        /// <param name="bbox">Bounding box</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="settings">Generation settings</param>
        /// <returns>Returns the new generated height field</returns>
        public static HeightField Build(BoundingBox bbox, Triangle[] triangles, NavigationMeshGenerationSettings settings)
        {
            var fh = new HeightField(bbox, settings.CellSize, settings.CellHeight);
            fh.RasterizeTriangles(triangles, Area.Default);
            fh.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            fh.FilterLowHangingWalkableObstacles(settings.VoxelMaxClimb);
            fh.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

            return fh;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightField"/> class.
        /// </summary>
        /// <param name="bbox">The world-space bounds.</param>
        /// <param name="cellSize">The world-space size of each cell in the XZ plane.</param>
        /// <param name="cellHeight">The world-space height of each cell.</param>
        public HeightField(BoundingBox bbox, float cellSize, float cellHeight)
        {
            this.CellSizeXZ = cellSize;
            this.CellHeight = cellHeight;
            this.Bounds = bbox;

            //Make sure the bbox contains all the possible voxels.
            this.Width = (int)Math.Ceiling((bbox.Maximum.X - bbox.Minimum.X) / cellSize);
            this.Height = (int)Math.Ceiling((bbox.Maximum.Y - bbox.Minimum.Y) / cellHeight);
            this.Length = (int)Math.Ceiling((bbox.Maximum.Z - bbox.Minimum.Z) / cellSize);

            Vector3 min = this.Bounds.Minimum;
            Vector3 max = new Vector3();
            max.X = min.X + this.Width * cellSize;
            max.Y = min.Y + this.Height * cellHeight;
            max.Z = min.Z + this.Length * cellSize;
            this.Bounds = new BoundingBox(min, max);

            this.cells = new HeightFieldCell[this.Width * this.Length];
            for (int i = 0; i < this.cells.Length; i++)
            {
                this.cells[i] = new HeightFieldCell(this.Height);
            }
        }

        /// <summary>
        /// Filters the heightmap to allow two neighboring spans have a small difference in maximum height (such as
        /// stairs) to be walkable.
        /// </summary>
        /// <remarks>
        /// This filter may override the results of <see cref="FilterLedgeSpans"/>.
        /// </remarks>
        /// <param name="walkableClimb">The maximum difference in height to filter.</param>
        public void FilterLowHangingWalkableObstacles(int walkableClimb)
        {
            //Loop through every cell in the Heightfield
            for (int i = 0; i < this.cells.Length; i++)
            {
                var c = this.cells[i];

                //Store the first span's data as the "previous" data
                var spans = c.MutableSpans;
                Area prevArea = Area.Null;
                bool prevWalkable = prevArea != Area.Null;
                int prevMax = 0;

                //Iterate over all the spans in the cell
                for (int j = 0; j < spans.Count; j++)
                {
                    var span = spans[j];
                    bool walkable = span.Area != Area.Null;

                    //If the current span isn't walkable but there's a walkable span right below it, mark this span as walkable too.
                    if (!walkable && prevWalkable)
                    {
                        if (Math.Abs(span.Maximum - prevMax) < walkableClimb)
                        {
                            span.Area = prevArea;
                        }
                    }

                    //Save changes back to the span list.
                    spans[j] = span;

                    //Set the previous data for the next iteration
                    prevArea = span.Area;
                    prevWalkable = walkable;
                    prevMax = span.Maximum;
                }
            }
        }
        /// <summary>
        /// If two spans have little vertical space in between them, 
        /// then span is considered unwalkable
        /// </summary>
        /// <param name="walkableHeight">The clearance.</param>
        public void FilterWalkableLowHeightSpans(int walkableHeight)
        {
            for (int i = 0; i < this.cells.Length; i++)
            {
                var c = this.cells[i];

                var spans = c.MutableSpans;

                //Iterate over all spans
                for (int j = 0; j < spans.Count - 1; j++)
                {
                    HeightFieldSpan currentSpan = spans[j];

                    //Too low, not enough space to walk through
                    if ((spans[j + 1].Minimum - currentSpan.Maximum) <= walkableHeight)
                    {
                        currentSpan.Area = Area.Null;
                        spans[j] = currentSpan;
                    }
                }
            }
        }
        /// <summary>
        /// A ledge is unwalkable because the difference between the maximum height of two spans
        /// is too large of a drop (i.e. greater than walkableClimb).
        /// </summary>
        /// <param name="walkableHeight">The maximum walkable height to filter.</param>
        /// <param name="walkableClimb">The maximum walkable climb to filter.</param>
        public void FilterLedgeSpans(int walkableHeight, int walkableClimb)
        {
            //Mark border spans.
            Parallel.For(0, this.Length, y =>
            {
                for (int x = 0; x < this.Width; x++)
                {
                    var c = this.cells[x + y * this.Width];

                    var spans = c.MutableSpans;

                    //Examine all the spans in each cell
                    for (int i = 0; i < spans.Count; i++)
                    {
                        HeightFieldSpan currentSpan = spans[i];

                        // Process only walkable spans.
                        if (currentSpan.Area != Area.Null)
                        {
                            int bottom = (int)currentSpan.Maximum;
                            int top = (i == spans.Count - 1) ? int.MaxValue : spans[i + 1].Minimum;

                            // Find neighbors minimum height.
                            int minHeight = int.MaxValue;

                            // Min and max height of accessible neighbors.
                            int accessibleMin = currentSpan.Maximum;
                            int accessibleMax = currentSpan.Maximum;

                            for (var dir = Direction.West; dir <= Direction.South; dir++)
                            {
                                int dx = x + dir.GetHorizontalOffset();
                                int dy = y + dir.GetVerticalOffset();

                                // Skip neighbors which are out of bounds.
                                if (dx < 0 || dy < 0 || dx >= this.Width || dy >= this.Length)
                                {
                                    minHeight = Math.Min(minHeight, -walkableClimb - bottom);
                                    continue;
                                }

                                // From minus infinity to the first span.
                                HeightFieldCell neighborCell = cells[dy * this.Width + dx];
                                var neighborSpans = neighborCell.MutableSpans;
                                int neighborBottom = -walkableClimb;
                                int neighborTop = neighborSpans.Count > 0 ? neighborSpans[0].Minimum : int.MaxValue;

                                // Skip neighbor if the gap between the spans is too small.
                                if (Math.Min(top, neighborTop) - Math.Max(bottom, neighborBottom) > walkableHeight)
                                {
                                    minHeight = Math.Min(minHeight, neighborBottom - bottom);
                                }

                                // Rest of the spans.
                                for (int j = 0; j < neighborSpans.Count; j++)
                                {
                                    HeightFieldSpan currentNeighborSpan = neighborSpans[j];

                                    neighborBottom = currentNeighborSpan.Maximum;
                                    neighborTop = (j == neighborSpans.Count - 1) ? int.MaxValue : neighborSpans[j + 1].Minimum;

                                    // Skip neighbor if the gap between the spans is too small.
                                    if (Math.Min(top, neighborTop) - Math.Max(bottom, neighborBottom) > walkableHeight)
                                    {
                                        minHeight = Math.Min(minHeight, neighborBottom - bottom);

                                        // Find min/max accessible neighbor height.
                                        if (Math.Abs(neighborBottom - bottom) <= walkableClimb)
                                        {
                                            if (neighborBottom < accessibleMin) accessibleMin = neighborBottom;
                                            if (neighborBottom > accessibleMax) accessibleMax = neighborBottom;
                                        }
                                    }
                                }
                            }

                            // The current span is close to a ledge if the drop to any neighbor span is less than the walkableClimb.
                            if (minHeight < -walkableClimb)
                            {
                                currentSpan.Area = Area.Null;
                            }

                            // If the difference between all neighbors is too large, we are at steep slope, mark the span as ledge.
                            if ((accessibleMax - accessibleMin) > walkableClimb)
                            {
                                currentSpan.Area = Area.Null;
                            }

                            //save span data
                            spans[i] = currentSpan;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Rasterizes several triangles at once.
        /// </summary>
        /// <param name="tris">An array of triangles.</param>
        /// <param name="area">The area flags for all of the triangles.</param>
        public void RasterizeTriangles(Triangle[] tris, Area area)
        {
            this.RasterizeTriangles(tris, 0, tris.Length, area);
        }
        /// <summary>
        /// Rasterizes several triangles at once.
        /// </summary>
        /// <param name="tris">An array of triangles.</param>
        /// <param name="triOffset">An offset into the array.</param>
        /// <param name="triCount">The number of triangles to rasterize, starting from the offset.</param>
        /// <param name="area">The area flags for all of the triangles.</param>
        public void RasterizeTriangles(Triangle[] tris, int triOffset, int triCount, Area area)
        {
            int triEnd = triOffset + triCount;
            int numBatches = 8;
            int threads = (triCount / numBatches) + 1;

            Parallel.For(0, threads, i =>
            {
                int start = triOffset + i * numBatches;
                int end = triOffset + (i + 1) * numBatches;
                
                if (end > triEnd) end = triEnd;

                for (int j = start; j < end; j++)
                {
                    Triangle t = tris[j];
                    this.RasterizeTriangle(ref t.Point1, ref t.Point2, ref t.Point3, area);
                }
            });
        }
        /// <summary>
        /// Rasterizes a triangle using conservative voxelization.
        /// </summary>
        /// <param name="a">The first vertex of the triangle.</param>
        /// <param name="b">The second vertex of the triangle.</param>
        /// <param name="c">The third vertex of the triangle.</param>
        /// <param name="area">The area flags for the triangle.</param>
        public void RasterizeTriangle(ref Vector3 a, ref Vector3 b, ref Vector3 c, Area area)
        {
            //distances buffer for ClipPolygonToBounds
            float[] distances = new float[12];

            float invCellSize = 1f / this.CellSizeXZ;
            float invCellHeight = 1f / this.CellHeight;
            float boundHeight = this.Bounds.Maximum.Y - this.Bounds.Minimum.Y;

            //calculate the triangle's bounding box
            BoundingBox bbox = BoundingBox.FromPoints(new[] { a, b, c });

            //make sure that the triangle is at least in one cell.
            if (this.Bounds.Contains(ref bbox) == ContainmentType.Disjoint)
            {
                return;
            }

            //figure out which rows.
            int z0 = (int)((bbox.Minimum.Z - this.Bounds.Minimum.Z) * invCellSize);
            int z1 = (int)((bbox.Maximum.Z - this.Bounds.Minimum.Z) * invCellSize);

            //clamp to the field boundaries.
            z0 = MathUtil.Clamp(z0, 0, this.Length - 1);
            z1 = MathUtil.Clamp(z1, 0, this.Length - 1);

            for (int z = z0; z <= z1; z++)
            {
                //copy the original vertices to the array.
                Polygon pIn = new Polygon(a, b, c);

                //clip the triangle to the row
                float cz = this.Bounds.Minimum.Z + z * this.CellSizeXZ;

                Polygon pOut;
                int nvrow = Polygon.ClipPolygonToPlane(pIn, 0, 1, -cz, out pOut);
                if (nvrow < 3) continue;

                Polygon pInRow;
                nvrow = Polygon.ClipPolygonToPlane(pOut, 0, -1, cz + this.CellSizeXZ, out pInRow);
                if (nvrow < 3) continue;

                float minX = pInRow[0].X, maxX = minX;
                for (int i = 1; i < nvrow; i++)
                {
                    float vx = pInRow[i].X;
                    if (minX > vx) minX = vx;
                    if (maxX < vx) maxX = vx;
                }

                int x0 = (int)((minX - this.Bounds.Minimum.X) * invCellSize);
                int x1 = (int)((maxX - this.Bounds.Minimum.X) * invCellSize);

                x0 = MathUtil.Clamp(x0, 0, this.Width - 1);
                x1 = MathUtil.Clamp(x1, 0, this.Width - 1);

                for (int x = x0; x <= x1; x++)
                {
                    //clip the triangle to the column
                    int nv = nvrow;
                    float cx = this.Bounds.Minimum.X + x * this.CellSizeXZ;
                    nv = Polygon.ClipPolygonToPlane(pInRow, 1, 0, -cx, out pOut);
                    if (nv < 3) continue;
                    nv = Polygon.ClipPolygonToPlane(pOut, -1, 0, cx + this.CellSizeXZ, out pIn);
                    if (nv < 3) continue;

                    //calculate the min/max of the polygon
                    float polyMin = pIn[0].Y, polyMax = polyMin;
                    for (int i = 1; i < nv; i++)
                    {
                        float y = pIn[i].Y;
                        polyMin = Math.Min(polyMin, y);
                        polyMax = Math.Max(polyMax, y);
                    }

                    //normalize span bounds to bottom of heightfield
                    float boundMinY = this.Bounds.Minimum.Y;
                    polyMin -= boundMinY;
                    polyMax -= boundMinY;

                    //if the spans are outside the heightfield, skip.
                    if (polyMax < 0f || polyMin > boundHeight)
                        continue;

                    //clamp the span to the heightfield.
                    if (polyMin < 0) polyMin = 0;
                    if (polyMax > boundHeight) polyMax = boundHeight;

                    //snap to grid
                    int spanMin = (int)(polyMin * invCellHeight);
                    int spanMax = (int)Math.Ceiling(polyMax * invCellHeight);

                    //add the span
                    this.cells[z * this.Width + x].AddSpan(new HeightFieldSpan(spanMin, spanMax, area));
                }
            }
        }
    }
}
