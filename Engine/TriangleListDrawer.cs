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
        private Mesh triangleListMesh
        {
            get
            {
                return this.Meshes[ModelContent.StaticMesh][ModelContent.NoMaterial];
            }
        }
        /// <summary>
        /// Triangle dictionary by color
        /// </summary>
        private Dictionary<Color4, List<Triangle>> triangleDictionary = new Dictionary<Color4, List<Triangle>>();
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
            : base(game, ModelContent.GenerateTriangleList(triangles, color))
        {
            this.EnableAlphaBlending = true;

            this.triangleDictionary.Add(color, new List<Triangle>(triangles));

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="count">Maximum triangle count</param>
        public TriangleListDrawer(Game game, int count)
            : base(game, ModelContent.GenerateTriangleList(new Triangle[count], Color.Transparent))
        {
            this.EnableAlphaBlending = true;

            this.dictionaryChanged = false;
        }
        /// <summary>
        /// Update content
        /// </summary>
        /// <param name="context">Context</param>
        public override void Update(UpdateContext context)
        {
            if (this.triangleDictionary.Count > 0)
            {
                this.WriteDataInBuffer();
            }

            base.Update(context);
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
        /// Set triangle list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void SetTriangles(Color4 color, Triangle[] triangle)
        {
            if (triangle != null && triangle.Length > 0)
            {
                if (!this.triangleDictionary.ContainsKey(color))
                {
                    this.triangleDictionary.Add(color, new List<Triangle>());
                }
                else
                {
                    this.triangleDictionary[color].Clear();
                }

                this.triangleDictionary[color].AddRange(triangle);

                this.dictionaryChanged = true;
            }
            else
            {
                if (this.triangleDictionary.ContainsKey(color))
                {
                    this.triangleDictionary.Remove(color);

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
            if (!this.triangleDictionary.ContainsKey(color))
            {
                this.triangleDictionary.Add(color, new List<Triangle>());
            }

            this.triangleDictionary[color].AddRange(triangle);

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Remove by color
        /// </summary>
        /// <param name="color">Color</param>
        public void ClearTriangles(Color4 color)
        {
            if (this.triangleDictionary.ContainsKey(color))
            {
                this.triangleDictionary.Remove(color);
            }

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Remove all
        /// </summary>
        public void ClearTriangles()
        {
            this.triangleDictionary.Clear();

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

                foreach (Color4 color in this.triangleDictionary.Keys)
                {
                    List<Triangle> triangles = this.triangleDictionary[color];
                    if (triangles.Count > 0)
                    {
                        for (int i = 0; i < triangles.Count; i++)
                        {
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point1, Color = color });
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point2, Color = color });

                            data.Add(new VertexPositionColor() { Position = triangles[i].Point2, Color = color });
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point3, Color = color });

                            data.Add(new VertexPositionColor() { Position = triangles[i].Point3, Color = color });
                            data.Add(new VertexPositionColor() { Position = triangles[i].Point1, Color = color });
                        }
                    }
                }

                this.triangleListMesh.WriteVertexData(this.DeviceContext, data.ToArray());

                this.dictionaryChanged = false;
            }
        }
    }
}
