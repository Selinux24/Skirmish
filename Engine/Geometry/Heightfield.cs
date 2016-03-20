using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharpDX;

namespace Engine.Geometry
{
    /// <summary>
    /// A Heightfield represents a "voxel" grid represented as a 2-dimensional grid of <see cref="Cell"/>s.
    /// </summary>
    public class Heightfield
    {
        private Cell[] cells;

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
        /// Gets the <see cref="Cell"/> at the specified coordinate.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <returns>The cell at [x, y].</returns>
        public Cell this[int x, int y]
        {
            get
            {
                return this.cells[y * this.Width + x];
            }
        }
        /// <summary>
        /// Gets the <see cref="Cell"/> at the specified index.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The cell at index i.</returns>
        public Cell this[int i]
        {
            get
            {
                return this.cells[i];
            }
        }

        /// <summary>
        /// Gets the <see cref="Span"/> at the reference.
        /// </summary>
        /// <param name="spanRef">A reference to a span.</param>
        /// <returns>The span at the reference.</returns>
        public Span this[SpanReference spanRef]
        {
            get
            {
                return cells[spanRef.Y * this.Width + spanRef.X].Spans[spanRef.Index];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Heightfield"/> class.
        /// </summary>
        /// <param name="b">The world-space bounds.</param>
        /// <param name="cellSize">The world-space size of each cell in the XZ plane.</param>
        /// <param name="cellHeight">The world-space height of each cell.</param>
        public Heightfield(BoundingBox b, float cellSize, float cellHeight)
        {
            this.CellSizeXZ = cellSize;
            this.CellHeight = cellHeight;
            this.Bounds = b;

            //Make sure the bbox contains all the possible voxels.
            this.Width = (int)Math.Ceiling((b.Maximum.X - b.Minimum.X) / cellSize);
            this.Height = (int)Math.Ceiling((b.Maximum.Y - b.Minimum.Y) / cellHeight);
            this.Length = (int)Math.Ceiling((b.Maximum.Z - b.Minimum.Z) / cellSize);

            Vector3 min = this.Bounds.Minimum;
            Vector3 max = new Vector3();
            max.X = min.X + this.Width * cellSize;
            max.Y = min.Y + this.Height * cellHeight;
            max.Z = min.Z + this.Length * cellSize;
            this.Bounds = new BoundingBox(min, max);

            cells = new Cell[this.Width * this.Length];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell(this.Height);
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
                Cell c = this.cells[i];

                //Store the first span's data as the "previous" data
                var spans = c.MutableSpans;
                Area prevArea = Area.Null;
                bool prevWalkable = prevArea != Area.Null;
                int prevMax = 0;

                //Iterate over all the spans in the cell
                for (int j = 0; j < spans.Count; j++)
                {
                    Span span = spans[j];
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
                Cell c = cells[i];

                var spans = c.MutableSpans;

                //Iterate over all spans
                for (int j = 0; j < spans.Count - 1; j++)
                {
                    Span currentSpan = spans[j];

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
                    Cell c = cells[x + y * this.Width];

                    var spans = c.MutableSpans;

                    //Examine all the spans in each cell
                    for (int i = 0; i < spans.Count; i++)
                    {
                        Span currentSpan = spans[i];

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
                                Cell neighborCell = cells[dy * this.Width + dx];
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
                                    Span currentNeighborSpan = neighborSpans[j];

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
            RasterizeTriangles(tris, 0, tris.Length, area);
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
                    RasterizeTriangle(ref t.Point1, ref t.Point2, ref t.Point3, area);
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
                int nvrow = 3;
                float cz = this.Bounds.Minimum.Z + z * this.CellSizeXZ;
                Polygon pOut;
                nvrow = Polygon.ClipPolygonToPlane(pIn, 0, 1, -cz, out pOut);
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
                    cells[z * this.Width + x].AddSpan(new Span(spanMin, spanMax, area));
                }
            }
        }
    }

    /// <summary>
    /// A cell is a column of voxels represented in <see cref="Span"/>s.
    /// </summary>
    public class Cell
    {
        private List<Span> spans = new List<Span>();

        /// <summary>
        /// Gets the height of the cell in number of voxels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the number of spans in the cell.
        /// </summary>
        public int SpanCount
        {
            get
            {
                return this.spans.Count;
            }
        }
        /// <summary>
        /// Gets the number of spans that are in walkable <see cref="Area"/>s.
        /// </summary>
        public int WalkableSpanCount
        {
            get
            {
                return this.spans.Count(s => s.Area.IsWalkable);
            }
        }
        /// <summary>
        /// Gets a readonly list of all the <see cref="Span"/>s contained in the cell.
        /// </summary>
        /// <value>A readonly list of spans.</value>
        public ReadOnlyCollection<Span> Spans
        {
            get
            {
                return this.spans.AsReadOnly();
            }
        }
        /// <summary>
        /// Gets a modifiable list of all the <see cref="Span"/>s contained in the cell.
        /// Should only be used for filtering in <see cref="Heightfield"/>.
        /// </summary>
        /// <value>A list of spans for modification.</value>
        internal List<Span> MutableSpans
        {
            get
            {
                return this.spans;
            }
        }
        /// <summary>
        /// Gets the <see cref="Span"/> that contains the specified voxel.
        /// </summary>
        /// <param name="location">The voxel to search for.</param>
        /// <returns>The span containing the voxel. Null if the voxel is empty.</returns>
        public Span? this[int location]
        {
            get
            {
                //Iterate the list of spans
                foreach (Span s in this.spans)
                {
                    if (s.Minimum > location)
                    {
                        break;
                    }
                    else if (s.Maximum >= location)
                    {
                        return s;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cell"/> class.
        /// </summary>
        /// <param name="height">The number of voxels in the column.</param>
        public Cell(int height)
        {
            this.Height = height;
        }

        /// <summary>
        /// Adds a <see cref="Span"/> to the cell.
        /// </summary>
        /// <param name="span">A span.</param>
        /// <exception cref="ArgumentException">Thrown if an invalid span is provided.</exception>
        public void AddSpan(Span span)
        {
            if (span.Minimum > span.Maximum)
            {
                int tmp = span.Minimum;
                span.Minimum = span.Maximum;
                span.Maximum = tmp;
            }

            //Clamp the span to the cell's range of [0, maxHeight]
            span.Minimum = MathUtil.Clamp(span.Minimum, 0, this.Height);
            span.Maximum = MathUtil.Clamp(span.Maximum, 0, this.Height);

            lock (this.spans)
            {
                for (int i = 0; i < this.spans.Count; i++)
                {
                    //Check whether the current span is below, or overlapping existing spans.
                    //If the span is completely above the current span the loop will continue.
                    Span cur = this.spans[i];
                    if (cur.Minimum > span.Maximum)
                    {
                        //The new span is below the current one and is not intersecting.
                        this.spans.Insert(i, span);
                        return;
                    }
                    else if (cur.Maximum >= span.Minimum)
                    {
                        //The new span is colliding with the current one, merge them together.
                        if (cur.Minimum < span.Minimum)
                        {
                            span.Minimum = cur.Minimum;
                        }

                        if (cur.Maximum == span.Maximum)
                        {
                            //In the case that both spans end at the same voxel, the area gets merged. The new span's area
                            //has priority if both spans are walkable, so the only case where the area gets set is when
                            //the new area isn't walkable and the old one is.
                            if (!span.Area.IsWalkable && cur.Area.IsWalkable)
                            {
                                span.Area = cur.Area;
                            }
                        }
                        else if (cur.Maximum > span.Maximum)
                        {
                            span.Maximum = cur.Maximum;
                            span.Area = cur.Area;
                        }

                        //Remove the current span and adjust i.
                        //We do this to avoid duplicating the current span.
                        this.spans.RemoveAt(i);
                        i--;
                    }
                }

                //If the span is not inserted, it is the highest span and will be added to the end.
                this.spans.Add(span);
            }
        }
    }

    /// <summary>
    /// A span is a range of integers which represents a range of voxels in a <see cref="Cell"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Span
    {
        /// <summary>
        /// The lowest value in the span.
        /// </summary>
        public int Minimum;
        /// <summary>
        /// The highest value in the span.
        /// </summary>
        public int Maximum;
        /// <summary>
        /// The span area id
        /// </summary>
        public Area Area;
        /// <summary>
        /// Gets the height of the span.
        /// </summary>
        public int Height
        {
            get
            {
                return this.Maximum - this.Minimum;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Span"/> struct.
        /// </summary>
        /// <param name="min">The lowest value in the span.</param>
        /// <param name="max">The highest value in the span.</param>
        public Span(int min, int max)
        {
            this.Minimum = min;
            this.Maximum = max;
            this.Area = Area.Null;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Span"/> struct.
        /// </summary>
        /// <param name="min">The lowest value in the span.</param>
        /// <param name="max">The highest value in the span.</param>
        /// <param name="area">The area flags for the span.</param>
        public Span(int min, int max, Area area)
        {
            this.Minimum = min;
            this.Maximum = max;
            this.Area = area;
        }
    }

    /// <summary>
    /// References a <see cref="Span"/> within a <see cref="Heightfield"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SpanReference
    {
        private int x;
        private int y;
        private int index;

        /// <summary>
        /// Gets the X coordinate of the <see cref="Cell"/> that contains the referenced <see cref="Span"/>.
        /// </summary>
        public int X
        {
            get
            {
                return this.x;
            }
        }
        /// <summary>
        /// Gets the Y coordinate of the <see cref="Cell"/> that contains the referenced <see cref="Span"/>.
        /// </summary>
        public int Y
        {
            get
            {
                return this.y;
            }
        }
        /// <summary>
        /// Gets the index of the <see cref="Span"/> within the <see cref="Cell"/> it is contained in.
        /// </summary>
        public int Index
        {
            get
            {
                return this.index;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanReference"/> struct.
        /// </summary>
        /// <param name="x">The X coordinate of the <see cref="Cell"/> the <see cref="Span"/> is contained in.</param>
        /// <param name="y">The Y coordinate of the <see cref="Cell"/> the <see cref="Span"/> is contained in.</param>
        /// <param name="i">The index of the <see cref="Span"/> within the specified <see cref="Cell"/>.</param>
        public SpanReference(int x, int y, int i)
        {
            this.x = x;
            this.y = y;
            this.index = i;
        }
    }

    /// <summary>
    /// An area groups together pieces of data through the navmesh generation process.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Area : IEquatable<Area>, IEquatable<byte>
    {
        /// <summary>
        /// The null area is one that is considered unwalkable.
        /// </summary>
        public static readonly Area Null = new Area(0);
        /// <summary>
        /// This is a default <see cref="Area"/> in the event that the user does not provide one.
        /// </summary>
        /// <remarks>
        /// If a user only applies IDs to some parts of a <see cref="Heightfield"/>, they will most likely choose low
        /// integer values. Choosing the maximum value makes it unlikely for the "default" area to collide with any
        /// user-defined areas.
        /// </remarks>
        public static readonly Area Default = new Area(0xff);

        /// <summary>
        /// Implicitly casts a byte to an Area. This is included since an Area is a very thin wrapper around a byte.
        /// </summary>
        /// <param name="value">The identifier for an area.</param>
        /// <returns>An area with the specified identifier.</returns>
        public static implicit operator Area(byte value)
        {
            return new Area(value);
        }
        /// <summary>
        /// Compares two areas for equality.
        /// </summary>
        /// <param name="left">An <see cref="Area"/>.</param>
        /// <param name="right">Another <see cref="Area"/></param>
        /// <returns>A value indicating whether the two <see cref="Area"/>s are equal.</returns>
        public static bool operator ==(Area left, Area right)
        {
            return left.Equals(right);
        }
        /// <summary>
        /// Compares two areas for inequality.
        /// </summary>
        /// <param name="left">An <see cref="Area"/>.</param>
        /// <param name="right">Another <see cref="Area"/></param>
        /// <returns>A value indicating whether the two <see cref="Area"/>s are unequal.</returns>
        public static bool operator !=(Area left, Area right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The identifier for an area, represented as a byte.
        /// </summary>
        public readonly byte Id;

        /// <summary>
        /// Gets a value indicating whether the area is considered walkable (not <see cref="Area.Null"/>).
        /// </summary>
        public bool IsWalkable
        {
            get
            {
                return Id != 0;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Area"/> struct.
        /// </summary>
        /// <param name="id">An identifier for the area.</param>
        public Area(byte id)
        {
            this.Id = id;
        }

        /// <summary>
        /// Compares this instance with another instance of <see cref="Area"/> for equality.
        /// </summary>
        /// <param name="other">An <see cref="Area"/>.</param>
        /// <returns>A value indicating whether the two <see cref="Area"/>s are equal.</returns>
        public bool Equals(Area other)
        {
            return this.Id == other.Id;
        }
        /// <summary>
        /// Compares this instance with a byte representing another <see cref="Area"/> for equality.
        /// </summary>
        /// <param name="other">A byte.</param>
        /// <returns>A value indicating whether this instance and the specified byte are equal.</returns>
        public bool Equals(byte other)
        {
            return this.Id == other;
        }
        /// <summary>
        /// Compares this instance with another object for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether this instance and the specified object are equal.</returns>
        public override bool Equals(object obj)
        {
            var areaObj = obj as Area?;
            var byteObj = obj as byte?;

            if (areaObj.HasValue)
                return this.Equals(areaObj.Value);
            else if (byteObj.HasValue)
                return this.Equals(byteObj.Value);
            else
                return false;
        }
        /// <summary>
        /// Generates a hashcode unique to the <see cref="Id"/> of this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        /// <summary>
        /// Converts this instance to a human-readable string.
        /// </summary>
        /// <returns>A string representing this instance.</returns>
        public override string ToString()
        {
            if (Id == 0)
                return "Null/Unwalkable";
            else
                return Id.ToString();
        }
    }

    /// <summary>
    /// A set of cardinal directions.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// The west direction.
        /// </summary>
        West = 0,
        /// <summary>
        /// The north direction.
        /// </summary>
        North = 1,
        /// <summary>
        /// The east direction.
        /// </summary>
        East = 2,
        /// <summary>
        /// The south direction.
        /// </summary>
        South = 3
    }

    /// <summary>
    /// A set of extension methods to make using the Direction enum a lot simpler.
    /// </summary>
    public static class DirectionExtensions
    {
        private static readonly int[] OffsetsX = { -1, 0, 1, 0 };
        private static readonly int[] OffsetsY = { 0, 1, 0, -1 };

        /// <summary>
        /// Gets an X offset.
        /// </summary>
        /// <remarks>
        /// The directions cycle between the following, starting from 0: west, north, east, south.
        /// </remarks>
        /// <param name="dir">The direction.</param>
        /// <returns>The offset for the X coordinate.</returns>
        public static int GetHorizontalOffset(this Direction dir)
        {
            return OffsetsX[(int)dir];
        }
        /// <summary>
        /// Get a Y offset.
        /// </summary>
        /// <remarks>
        /// The directions cycle between the following, starting from 0: west, north, east, south.
        /// </remarks>
        /// <param name="dir">The direction.</param>
        /// <returns>The offset for the Y coordinate.</returns>
        public static int GetVerticalOffset(this Direction dir)
        {
            return OffsetsY[(int)dir];
        }
        /// <summary>
        /// Gets the next cardinal direction in clockwise order.
        /// </summary>
        /// <param name="dir">The current direction.</param>
        /// <returns>The next direction.</returns>
        public static Direction NextClockwise(this Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return Direction.North;
                case Direction.North:
                    return Direction.East;
                case Direction.East:
                    return Direction.South;
                case Direction.South:
                    return Direction.West;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
        /// <summary>
        /// Gets the next cardinal direction in counter-clockwise order.
        /// </summary>
        /// <param name="dir">The current direction.</param>
        /// <returns>The next direction.</returns>
        public static Direction NextCounterClockwise(this Direction dir)
        {
            switch (dir)
            {
                case Direction.West:
                    return Direction.South;
                case Direction.South:
                    return Direction.East;
                case Direction.East:
                    return Direction.North;
                case Direction.North:
                    return Direction.West;
                default:
                    throw new ArgumentException("dir isn't a valid Direction.");
            }
        }
    }
}
