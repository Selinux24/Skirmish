using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Primitive list drawer
    /// </summary>
    /// <typeparam name="T">Primitive list type</typeparam>
    public class PrimitiveListDrawer<T> : Drawable where T : IVertexList
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
        private readonly Dictionary<Color4, List<T>> dictionary = new Dictionary<Color4, List<T>>();
        /// <summary>
        /// Dictionary changes flag
        /// </summary>
        private bool dictionaryChanged = false;
        /// <summary>
        /// Item stride
        /// </summary>
        private readonly int stride = 0;
        /// <summary>
        /// Item topology
        /// </summary>
        private readonly Topology topology;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public PrimitiveListDrawer(Scene scene, PrimitiveListDrawerDescription<T> description)
            : base(scene, description)
        {
            var vertsPerItem = default(T).GetVertices();
            stride = vertsPerItem.Length;
            switch (stride)
            {
                case 1:
                    topology = Topology.PointList;
                    break;
                case 2:
                    topology = Topology.LineList;
                    break;
                case 3:
                    topology = Topology.TriangleList;
                    break;
                default:
                    topology = Topology.PointList;
                    break;
            }

            int count = 0;
            if (description.Primitives?.Length > 0)
            {
                count = description.Primitives.Length * stride;

                this.dictionary.Add(description.Color, new List<T>(description.Primitives));
                this.dictionaryChanged = true;
            }
            else
            {
                count = description.Count * stride;

                this.dictionaryChanged = false;
            }

            this.InitializeBuffers(description.Name, count);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PrimitiveListDrawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                this.BufferManager?.RemoveVertexData(this.vertexBuffer);
            }
        }
        /// <summary>
        /// Draw content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            if ((mode.HasFlag(DrawerModes.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModes.TransparentOnly) && this.Description.AlphaEnabled))
            {
                this.WriteDataInBuffer();

                if (this.drawCount > 0)
                {
                    var effect = DrawerPool.EffectDefaultBasic;

                    Counters.InstancesPerFrame += this.dictionary.Count;
                    Counters.PrimitivesPerFrame += this.drawCount / this.stride;

                    effect.UpdatePerFrameBasic(Matrix.Identity, context);
                    effect.UpdatePerObject(0, null, 0, false);

                    var technique = effect.GetTechnique(VertexTypes.PositionColor, false);
                    this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, this.topology);

                    var graphics = this.Game.Graphics;

                    if (this.Description.AlphaEnabled)
                    {
                        graphics.SetBlendDefaultAlpha();
                    }

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(this.drawCount, this.vertexBuffer.Offset);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexCount">Vertex count</param>
        private void InitializeBuffers(string name, int vertexCount)
        {
            this.vertexBuffer = this.BufferManager.Add(name, new VertexPositionColor[vertexCount], true, 0);
        }
        /// <summary>
        /// Set primitive
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="primitive">Primitive</param>
        public void SetPrimitives(Color4 color, T primitive)
        {
            this.SetPrimitives(color, new[] { primitive });
        }
        /// <summary>
        /// Set primitives list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="primitives">Primitives list</param>
        public void SetPrimitives(Color4 color, IEnumerable<T> primitives)
        {
            if (primitives?.Count() > 0)
            {
                if (!this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Add(color, new List<T>());
                }
                else
                {
                    this.dictionary[color].Clear();
                }

                this.dictionary[color].AddRange(primitives);

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
        /// Add primitive to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="primitive">primitive</param>
        public void AddPrimitives(Color4 color, T primitive)
        {
            AddPrimitives(color, new[] { primitive });
        }
        /// <summary>
        /// Add primitives to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="primitives">Primitives list</param>
        public void AddPrimitives(Color4 color, IEnumerable<T> primitives)
        {
            if (!this.dictionary.ContainsKey(color))
            {
                this.dictionary.Add(color, new List<T>());
            }

            this.dictionary[color].AddRange(primitives);

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

                foreach (var color in this.dictionary.Keys)
                {
                    var primitives = this.dictionary[color];
                    for (int i = 0; i < primitives.Count; i++)
                    {
                        var vList = primitives[i].GetVertices();
                        for (int v = 0; v < vList.Length; v++)
                        {
                            data.Add(new VertexPositionColor() { Position = vList[v], Color = color });
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
