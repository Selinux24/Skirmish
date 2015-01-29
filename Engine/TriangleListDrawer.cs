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
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="scene">Scene</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        public TriangleListDrawer(Game game, Scene3D scene, Triangle[] triangles, Color4 color)
            : base(game, scene, ModelContent.GenerateTriangleList(triangles, color))
        {

        }
        /// <summary>
        /// Set triangle list
        /// </summary>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        public void SetTriangles(Triangle[] triangles, Color4 color)
        {
            List<IVertexData> data = new List<IVertexData>();

            for (int i = 0; i < triangles.Length; i++)
            {
                data.Add(new VertexPositionColor() { Position = triangles[i].Point1, Color = color });
                data.Add(new VertexPositionColor() { Position = triangles[i].Point2, Color = color });
                data.Add(new VertexPositionColor() { Position = triangles[i].Point3, Color = color });
            }

            this.triangleListMesh.WriteVertexData(this.DeviceContext, data.ToArray());
        }
    }
}
