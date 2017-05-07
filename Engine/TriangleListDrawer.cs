using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Line list drawer
    /// </summary>
    public class TriangleListDrawer : Drawable
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Primitives to draw
        /// </summary>
        private int drawCount = 0;
        /// <summary>
        /// Triangle dictionary by color
        /// </summary>
        private Dictionary<Color4, List<Triangle>> dictionary = new Dictionary<Color4, List<Triangle>>();
        /// <summary>
        /// Dictionary changes flag
        /// </summary>
        private bool dictionaryChanged = false;

        /// <summary>
        /// Maximum number of instances
        /// </summary>
        public override int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Description</param>
        /// <param name="count">Maximum triangle count</param>
        public TriangleListDrawer(Game game, BufferManager bufferManager, TriangleListDrawerDescription description, int count)
            : base(game, bufferManager, description)
        {
            this.dictionaryChanged = false;

            this.InitializeBuffers(count * 3);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="bufferManager">Buffer manager</param>
        /// <param name="description">Description</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        public TriangleListDrawer(Game game, BufferManager bufferManager, TriangleListDrawerDescription description, IEnumerable<Triangle> triangles, Color4 color)
            : base(game, bufferManager, description)
        {
            this.dictionary.Add(color, new List<Triangle>(triangles));
            this.dictionaryChanged = true;

            this.InitializeBuffers(triangles.Count() * 3);
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        public override void Dispose()
        {

        }
        /// <summary>
        /// Update content
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {

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

            if (this.drawCount > 0)
            {
                if (context.DrawerMode != DrawerModesEnum.ShadowMap)
                {
                    Counters.InstancesPerFrame += this.dictionary.Count;
                    Counters.PrimitivesPerFrame += this.drawCount / 3;
                }

                var effect = DrawerPool.EffectDefaultBasic;
                var technique = effect.GetTechnique(VertexTypes.PositionColor, false, DrawingStages.Drawing, context.DrawerMode);

                #region Per frame update

                effect.UpdatePerFrame(context.World, context.ViewProjection);

                #endregion

                #region Per object update

                effect.UpdatePerObject(null, null, null, 0, 0, 0);

                #endregion

                this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.TriangleList);

                if (this.AlphaEnabled) this.Game.Graphics.SetBlendDefaultAlpha();
                else this.Game.Graphics.SetBlendDefault();

                for (int p = 0; p < technique.Description.PassCount; p++)
                {
                    technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                    this.Game.Graphics.DeviceContext.Draw(this.drawCount, this.vertexBuffer.Offset);

                    Counters.DrawCallsPerFrame++;
                }
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="vertexCount">Vertex count</param>
        private void InitializeBuffers(int vertexCount)
        {
            this.vertexBuffer = this.BufferManager.Add(this.Name, new VertexPositionColor[vertexCount], true, 0);
        }
        /// <summary>
        /// Set triangle list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void SetTriangles(Color4 color, IEnumerable<Triangle> triangle)
        {
            if (triangle != null && triangle.Count() > 0)
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
        public void AddTriangles(Color4 color, IEnumerable<Triangle> triangle)
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
                List<VertexPositionColor> data = new List<VertexPositionColor>();

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

                this.drawCount = data.Count;

                if (data.Count > 0)
                {
                    this.BufferManager.WriteBuffer(this.vertexBuffer.Slot, this.vertexBuffer.Offset, data.ToArray());
                }

                this.dictionaryChanged = false;
            }
        }
    }
}
