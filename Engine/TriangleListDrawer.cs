using System.Collections.Generic;
using SharpDX;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Line list drawer
    /// </summary>
    public class TriangleListDrawer : Model
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
        /// Triangle dictionary by color
        /// </summary>
        private Dictionary<Color4, List<Triangle>> dictionary = new Dictionary<Color4, List<Triangle>>();
        /// <summary>
        /// Dictionary changes flag
        /// </summary>
        private bool dictionaryChanged = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        public TriangleListDrawer(Game game, Triangle[] triangles, Color4 color)
            : base(game, ModelContent.GenerateTriangleList(triangles, color), true)
        {
            this.EnableAlphaBlending = true;

            this.dictionary.Add(color, new List<Triangle>(triangles));
            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="count">Maximum triangle count</param>
        public TriangleListDrawer(Game game, int count)
            : base(game, ModelContent.GenerateTriangleList(new Triangle[count], Color.Transparent), true)
        {
            this.EnableAlphaBlending = true;

            this.dictionaryChanged = false;
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
        /// No culling
        /// </summary>
        /// <param name="frustum">Frustum</param>
        public override void Culling(BoundingFrustum frustum)
        {
            this.Cull = false;
        }
        /// <summary>
        /// No culling
        /// </summary>
        /// <param name="sphere">Sphere</param>
        public override void Culling(BoundingSphere sphere)
        {
            this.Cull = false;
        }

        /// <summary>
        /// Set triangle list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void SetTriangles(Color4 color, Triangle[] triangle)
        {
            if (triangle != null && triangle.Length > 0)
            {
                if (!this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Add(color, new List<Triangle>());
                }
                else
                {
                    this.dictionary[color].Clear();
                }

                this.dictionary[color].AddRange(triangle);

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
        public void AddTriangles(Color4 color, Triangle[] triangle)
        {
            if (!this.dictionary.ContainsKey(color))
            {
                this.dictionary.Add(color, new List<Triangle>());
            }

            this.dictionary[color].AddRange(triangle);

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
                    List<Triangle> triangles = this.dictionary[color];
                    if (triangles.Count > 0)
                    {
                        for (int i = 0; i < triangles.Count; i++)
                        {
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point1, Color = color });
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point2, Color = color });
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point3, Color = color });
                        }
                    }
                }

                this.mesh.WriteVertexData(this.DeviceContext, data.ToArray());

                this.dictionaryChanged = false;
            }
        }
    }
}
