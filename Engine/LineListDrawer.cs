using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Line list drawer
    /// </summary>
    public class LineListDrawer : Model
    {
        /// <summary>
        /// Line list mesh
        /// </summary>
        private Mesh mesh
        {
            get
            {
                return this.DrawingData.Meshes[ModelContent.StaticMesh][ModelContent.NoMaterial];
            }
        }
        /// <summary>
        /// Lines dictionary by color
        /// </summary>
        private Dictionary<Color4, List<Line3>> dictionary = new Dictionary<Color4, List<Line3>>();
        /// <summary>
        /// Dictionary changes flag
        /// </summary>
        private bool dictionaryChanged = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="count">Maximum line count</param>
        public LineListDrawer(Game game, int count)
            : base(game, ModelContent.GenerateLineList(new Line3[count], Color.Transparent), true)
        {
            this.EnableAlphaBlending = true;

            this.dictionaryChanged = false;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        public LineListDrawer(Game game, Line3[] lines, Color4 color)
            : base(game, ModelContent.GenerateLineList(lines, color), true)
        {
            this.EnableAlphaBlending = true;

            this.dictionary.Add(color, new List<Line3>(lines));
            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        public LineListDrawer(Game game, Triangle[] triangles, Color4 color)
            : base(game, ModelContent.GenerateLineList(Line3.CreateWiredTriangle(triangles), color), true)
        {
            this.EnableAlphaBlending = true;

            var lines = Line3.CreateWiredTriangle(triangles);

            this.dictionary.Add(color, new List<Line3>(lines));
            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Draw content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            if (this.dictionary.Count > 0)
            {
                this.WriteDataInBuffer();
            }

            base.Draw(context);
        }
        /// <summary>
        /// No frustum culling
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void FrustumCulling(BoundingFrustum frustum)
        {
            this.Cull = false;
        }

        /// <summary>
        /// Set line
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="line">Line</param>
        public void SetLines(Color4 color, Line3 line)
        {
            SetLines(color, new[] { line });
        }
        /// <summary>
        /// Set line list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Line list</param>
        public void SetLines(Color4 color, Line3[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                if (!this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Add(color, new List<Line3>());
                }
                else
                {
                    this.dictionary[color].Clear();
                }

                this.dictionary[color].AddRange(lines);

                this.dictionaryChanged = true;
            }
            else
            {
                if (this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Remove(color);

                    this.dictionaryChanged = true;
                }
            }
        }
        /// <summary>
        /// Add line to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="line">Line</param>
        public void AddLines(Color4 color, Line3 line)
        {
            AddLines(color, new[] { line });
        }
        /// <summary>
        /// Add lines to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Line list</param>
        public void AddLines(Color4 color, Line3[] lines)
        {
            if (!this.dictionary.ContainsKey(color))
            {
                this.dictionary.Add(color, new List<Line3>());
            }

            this.dictionary[color].AddRange(lines);

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Set triangle list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void SetTriangles(Color4 color, Triangle[] triangles)
        {
            if (triangles != null && triangles.Length > 0)
            {
                if (!this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Add(color, new List<Line3>());
                }
                else
                {
                    this.dictionary[color].Clear();
                }

                this.dictionary[color].AddRange(Line3.CreateWiredTriangle(triangles));

                this.dictionaryChanged = true;
            }
            else
            {
                if (this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Remove(color);

                    this.dictionaryChanged = true;
                }
            }
        }
        /// <summary>
        /// Add triangles to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void AddTriangles(Color4 color, Triangle[] triangles)
        {
            if (!this.dictionary.ContainsKey(color))
            {
                this.dictionary.Add(color, new List<Line3>());
            }

            this.dictionary[color].AddRange(Line3.CreateWiredTriangle(triangles));

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Remove by color
        /// </summary>
        /// <param name="color">Color</param>
        public void Clear(Color4 color)
        {
            if (this.dictionary.ContainsKey(color))
            {
                this.dictionary.Remove(color);
            }

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Remove all
        /// </summary>
        public void Clear()
        {
            this.dictionary.Clear();

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Writes dictionary data in buffer
        /// </summary>
        public void WriteDataInBuffer()
        {
            if (this.dictionaryChanged)
            {
                List<IVertexData> data = new List<IVertexData>();

                foreach (Color4 color in this.dictionary.Keys)
                {
                    List<Line3> lines = this.dictionary[color];
                    if (lines.Count > 0)
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            data.Add(new VertexPositionColor() { Position = lines[i].Point1, Color = color });
                            data.Add(new VertexPositionColor() { Position = lines[i].Point2, Color = color });
                        }
                    }
                }

                this.mesh.WriteVertexData(this.DeviceContext, data.ToArray());

                this.dictionaryChanged = false;
            }
        }
    }
}
