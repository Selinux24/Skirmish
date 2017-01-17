using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using System.Collections.Generic;
using Buffer = SharpDX.Direct3D11.Buffer;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;
    using Engine.Helpers;

    /// <summary>
    /// Line list drawer
    /// </summary>
    public class TriangleListDrawer : Drawable
    {
        /// <summary>
        /// Vertex buffer
        /// </summary>
        private Buffer vertexBuffer = null;
        /// <summary>
        /// Vertex count
        /// </summary>
        private int vertexCount = 0;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        private VertexBufferBinding[] vertexBufferBinding = null;
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
        /// <param name="description">Description</param>
        /// <param name="count">Maximum triangle count</param>
        public TriangleListDrawer(Game game, TriangleListDrawerDescription description, int count)
            : base(game, description)
        {
            this.dictionaryChanged = false;

            this.InitializeBuffers(count * 3);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="description">Description</param>
        /// <param name="triangles">Triangle list</param>
        /// <param name="color">Color</param>
        public TriangleListDrawer(Game game, TriangleListDrawerDescription description, Triangle[] triangles, Color4 color)
            : base(game, description)
        {
            this.dictionary.Add(color, new List<Triangle>(triangles));
            this.dictionaryChanged = true;

            this.InitializeBuffers(triangles.Length * 3);
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.vertexBuffer);
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

            var effect = DrawerPool.EffectDefaultBasic;
            var technique = effect.GetTechnique(VertexTypes.PositionColor, false, DrawingStages.Drawing, context.DrawerMode);

            #region Per frame update

            effect.UpdatePerFrame(context.World, context.ViewProjection);

            #endregion

            #region Per object update

            effect.UpdatePerObject(null, null, null, 0, 0, 0);

            #endregion

            //Sets vertex and index buffer
            this.Game.Graphics.DeviceContext.InputAssembler.InputLayout = effect.GetInputLayout(technique);
            Counters.IAInputLayoutSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
            Counters.IAVertexBuffersSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
            Counters.IAIndexBufferSets++;
            this.Game.Graphics.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            Counters.IAPrimitiveTopologySets++;

            if (this.EnableAlphaBlending) this.Game.Graphics.SetBlendDefaultAlpha();
            else this.Game.Graphics.SetBlendDefault();

            for (int p = 0; p < technique.Description.PassCount; p++)
            {
                technique.GetPassByIndex(p).Apply(this.Game.Graphics.DeviceContext, 0);

                this.Game.Graphics.DeviceContext.Draw(this.vertexCount, 0);

                Counters.DrawCallsPerFrame++;
                Counters.InstancesPerFrame++;
                Counters.PrimitivesPerFrame += this.vertexCount / 3;
            }
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
        /// Initialize buffers
        /// </summary>
        /// <param name="vertexCount">Vertex count</param>
        private void InitializeBuffers(int vertexCount)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[vertexCount];

            this.vertexBuffer = this.Game.Graphics.Device.CreateVertexBufferWrite(vertices);
            this.vertexBufferBinding = new[]
            {
                new VertexBufferBinding(this.vertexBuffer, vertices[0].GetStride(), 0),
            };
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

                this.vertexCount = data.Count;
                if (data.Count > 0)
                {
                    this.Game.Graphics.DeviceContext.WriteBuffer(this.vertexBuffer, data.ToArray());
                }

                this.dictionaryChanged = false;
            }
        }
    }
}
