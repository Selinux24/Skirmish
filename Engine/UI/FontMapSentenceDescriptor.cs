using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine.UI
{
    using Engine.BuiltIn.Primitives;

    /// <summary>
    /// Sentence descriptor
    /// </summary>
    public struct FontMapSentenceDescriptor
    {
        /// <summary>
        /// One character sentence descriptor
        /// </summary>
        public static readonly FontMapSentenceDescriptor OneCharDescriptor = Create(1);
        /// <summary>
        /// Creates a new sentence descriptor
        /// </summary>
        /// <param name="size">Number of characters</param>
        public static FontMapSentenceDescriptor Create(int size)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);

            return new()
            {
                Vertices = new VertexFont[size * 4],
                VertexCount = 0,
                Indices = new uint[size * 6],
                IndexCount = 0,

                size = Vector2.Zero,
                isDirty = false,
            };
        }

        /// <summary>
        /// Size
        /// </summary>
        private Vector2 size;
        /// <summary>
        /// Dirty flag
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Vertices
        /// </summary>
        public VertexFont[] Vertices { get; set; }
        /// <summary>
        /// Vertex count
        /// </summary>
        public uint VertexCount { get; set; }
        /// <summary>
        /// Indices
        /// </summary>
        public uint[] Indices { get; set; }
        /// <summary>
        /// Index count
        /// </summary>
        public uint IndexCount { get; set; }

        /// <summary>
        /// Clears the descriptor
        /// </summary>
        public void Clear()
        {
            VertexCount = 0;
            IndexCount = 0;

            isDirty = true;
        }

        /// <summary>
        /// Adds items to the descriptor
        /// </summary>
        /// <param name="indices">Index list</param>
        /// <param name="vertices">Vertex list</param>
        /// <param name="uvs">Uvs</param>
        /// <param name="color">Color</param>
        public void Add(IEnumerable<uint> indices, IEnumerable<Vector3> vertices, IEnumerable<Vector2> uvs, Color4 color)
        {
            Vector3[] vArray = [.. vertices];
            Vector2[] uvArray = [.. uvs];

            if (vArray.Length != uvArray.Length)
            {
                throw new ArgumentException("Vertices and uvs must have the same length");
            }

            foreach (uint idx in indices)
            {
                if (IndexCount >= Indices.Length)
                {
                    continue;
                }

                Indices[IndexCount++] = idx + VertexCount;
            }

            for (int i = 0; i < vArray.Length; i++)
            {
                if (VertexCount >= Vertices.Length)
                {
                    continue;
                }

                Vertices[VertexCount].Position = vArray[i];
                Vertices[VertexCount].Texture = uvArray[i];
                Vertices[VertexCount].Color = color;

                VertexCount++;
            }

            isDirty = true;
        }

        /// <summary>
        /// Aligns the vertices into the rectangle
        /// </summary>
        /// <param name="rect">Rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a new vertex list</returns>
        public void Adjust(RectangleF rect, TextHorizontalAlign horizontalAlign, TextVerticalAlign verticalAlign)
        {
            //Find total height (the column height)
            var textSize = GetSize(0, (int)VertexCount);

            int lIndex = 0;

            for (int i = 0; i < VertexCount; i += 4)
            {
                if (i == 0 || !MathUtil.IsZero(Vertices[i].Position.X))
                {
                    //Skip until the first character
                    continue;
                }

                AdjustLine(lIndex, i, textSize, rect, horizontalAlign, verticalAlign);

                //New line
                lIndex = i;
            }

            AdjustLine(lIndex, (int)VertexCount, textSize, rect, horizontalAlign, verticalAlign);

            isDirty = true;
        }
        /// <summary>
        /// Aligns the line vertices into the rectangle
        /// </summary>
        /// <param name="from">From position</param>
        /// <param name="to">To position</param>
        /// <param name="textSize">Text size</param>
        /// <param name="rect">Rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        private readonly void AdjustLine(int from, int to, Vector2 textSize, RectangleF rect, TextHorizontalAlign horizontalAlign, TextVerticalAlign verticalAlign)
        {
            //Find this line width
            var lineSize = GetSize(from, to);

            //Calculate displacement deltas
            float diffX = GetDeltaX(horizontalAlign, rect.Width, lineSize.X);
            float diffY = GetDeltaY(verticalAlign, rect.Height, textSize.Y);

            if (MathUtil.IsZero(diffX) && MathUtil.IsZero(diffY))
            {
                //No changes, add the line and skip the update
                return;
            }

            //Update all the coordinates
            for (int x = from; x < to; x++)
            {
                Vertices[x].Position.X += diffX;
                Vertices[x].Position.Y -= diffY;
            }
        }
        /// <summary>
        /// Gets the delta x
        /// </summary>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="lineWidth">Line width</param>
        private static float GetDeltaX(TextHorizontalAlign horizontalAlign, float maxWidth, float lineWidth)
        {
            float diffX;
            if (horizontalAlign == TextHorizontalAlign.Center)
            {
                diffX = -lineWidth * 0.5f;
            }
            else if (horizontalAlign == TextHorizontalAlign.Right)
            {
                diffX = (maxWidth * 0.5f) - lineWidth;
            }
            else
            {
                diffX = -maxWidth * 0.5f;
            }

            return diffX;
        }
        /// <summary>
        /// Gets the delta x
        /// </summary>
        /// <param name="verticalAlign">Vertical align</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <param name="columnHeight">Column height</param>
        private static float GetDeltaY(TextVerticalAlign verticalAlign, float maxHeight, float columnHeight)
        {
            float diffY;
            if (verticalAlign == TextVerticalAlign.Middle)
            {
                diffY = -columnHeight * 0.5f;
            }
            else if (verticalAlign == TextVerticalAlign.Bottom)
            {
                diffY = (maxHeight * 0.5f) - columnHeight;
            }
            else
            {
                diffY = -maxHeight * 0.5f;
            }

            return diffY;
        }
        /// <summary>
        /// Measures the vertex list
        /// </summary>
        /// <returns>Returns a vector with the width in the x component, and the height in the y component</returns>
        public Vector2 GetSize()
        {
            if (isDirty)
            {
                size = GetSize(0, (int)VertexCount);

                isDirty = false;
            }

            return size;
        }
        /// <summary>
        /// Measures the vertex list
        /// </summary>
        /// <param name="from">Index from</param>
        /// <param name="to">Index to</param>
        /// <returns>Returns a vector with the width in the x component, and the height in the y component</returns>
        private readonly Vector2 GetSize(int from, int to)
        {
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            float minX = float.MaxValue;
            float minY = float.MaxValue;

            for (int i = from; i < to; i++)
            {
                var p = Vertices[i].Position;

                maxX = MathF.Max(maxX, p.X);
                maxY = MathF.Max(maxY, -p.Y);

                minX = MathF.Min(minX, p.X);
                minY = MathF.Min(minY, -p.Y);
            }

            return new(maxX - minX, maxY - minY);
        }
    }
}
