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
        private Mesh lineListMesh
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
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        public LineListDrawer(Game game, Scene3D scene, Line[] lines, Color color)
            : base(game, scene, ModelContent.GenerateLineList(lines, color))
        {

        }
        /// <summary>
        /// Set line list
        /// </summary>
        /// <param name="lines">Line list</param>
        /// <param name="color">Color</param>
        public void SetLines(Line[] lines, Color color)
        {
            List<IVertexData> data = new List<IVertexData>();

            for (int i = 0; i < lines.Length; i++)
            {
                data.Add(new VertexPositionColor() { Position = lines[i].Point1, Color = color });
                data.Add(new VertexPositionColor() { Position = lines[i].Point2, Color = color });
            }

            this.lineListMesh.WriteVertexData(this.DeviceContext, data.ToArray());
        }
    }
}
