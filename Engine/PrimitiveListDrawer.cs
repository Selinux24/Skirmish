using SharpDX;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly ConcurrentDictionary<Color4, List<T>> dictionary = new ConcurrentDictionary<Color4, List<T>>();
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
        /// Returns true if the buffers were ready
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                if (vertexBuffer?.Ready != true)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Description</param>
        public PrimitiveListDrawer(string name, Scene scene, PrimitiveListDrawerDescription<T> description)
            : base(name, scene, description)
        {
            T tmp = default;
            stride = tmp.GetStride();
            topology = tmp.GetTopology();

            int count;
            if (description.Primitives?.Length > 0)
            {
                count = description.Primitives.Length * stride;

                dictionary.TryAdd(description.Color, new List<T>(description.Primitives));
                dictionaryChanged = true;
            }
            else
            {
                count = description.Count * stride;

                dictionaryChanged = false;
            }

            InitializeBuffers(name, count);
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PrimitiveListDrawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                BufferManager?.RemoveVertexData(vertexBuffer);
            }
        }
        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return;
            }

            WriteDataInBuffer();

            if (drawCount <= 0)
            {
                return;
            }

            Counters.InstancesPerFrame += dictionary.Count;
            Counters.PrimitivesPerFrame += drawCount / stride;

            var effect = DrawerPool.EffectDefaultBasic;
            var technique = effect.GetTechnique(VertexTypes.PositionColor, false);

            BufferManager.SetInputAssembler(technique, vertexBuffer, topology);

            effect.UpdatePerFrameBasic(Matrix.Identity, context);
            effect.UpdatePerObject(0, null, 0, false);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.Draw(drawCount, vertexBuffer.BufferOffset);
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="vertexCount">Vertex count</param>
        private void InitializeBuffers(string name, int vertexCount)
        {
            vertexBuffer = BufferManager.AddVertexData(name, true, new VertexPositionColor[vertexCount]);
        }
        /// <summary>
        /// Set primitive
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="primitive">Primitive</param>
        public void SetPrimitives(Color4 color, T primitive)
        {
            SetPrimitives(color, new[] { primitive });
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
                if (!dictionary.ContainsKey(color))
                {
                    dictionary.TryAdd(color, new List<T>());
                }
                else
                {
                    dictionary[color].Clear();
                }

                dictionary[color].AddRange(primitives);

                dictionaryChanged = true;
            }
            else
            {
                if (dictionary.ContainsKey(color))
                {
                    dictionary.TryRemove(color, out _);

                    dictionaryChanged = true;
                }
            }
        }
        /// <summary>
        /// Set primitives list
        /// </summary>
        /// <param name="primitivesDict">Primitives by color dictionary</param>
        public void SetPrimitives(Dictionary<Color4, IEnumerable<T>> primitivesDict)
        {
            foreach (var primitive in primitivesDict)
            {
                SetPrimitives(primitive.Key, primitive.Value);
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
            if (!dictionary.ContainsKey(color))
            {
                dictionary.TryAdd(color, new List<T>());
            }

            dictionary[color].AddRange(primitives);

            dictionaryChanged = true;
        }
        /// <summary>
        /// Add primitives to list
        /// </summary>
        /// <param name="primitivesDict">Primitives by color dictionary</param>
        public void AddPrimitives(Dictionary<Color4, IEnumerable<T>> primitivesDict)
        {
            foreach (var primitive in primitivesDict)
            {
                AddPrimitives(primitive.Key, primitive.Value);
            }
        }
        /// <summary>
        /// Remove by color
        /// </summary>
        /// <param name="color">Color</param>
        public void Clear(Color4 color)
        {
            if (dictionary.ContainsKey(color))
            {
                dictionary.TryRemove(color, out _);
            }

            dictionaryChanged = true;
        }
        /// <summary>
        /// Remove all
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();

            dictionaryChanged = true;
        }
        /// <summary>
        /// Writes dictionary data in buffer
        /// </summary>
        public void WriteDataInBuffer()
        {
            if (!dictionaryChanged)
            {
                return;
            }

            List<VertexPositionColor> data = new List<VertexPositionColor>();

            var copy = dictionary.ToArray();

            foreach (var item in copy)
            {
                var color = item.Key;
                var primitives = item.Value;

                for (int i = 0; i < primitives.Count; i++)
                {
                    var vList = primitives[i].GetVertices();
                    for (int v = 0; v < vList.Count(); v++)
                    {
                        data.Add(new VertexPositionColor()
                        {
                            Position = vList.ElementAt(v),
                            Color = color
                        });
                    }
                }
            }

            if (!BufferManager.WriteVertexBuffer(vertexBuffer, data))
            {
                return;
            }

            drawCount = data.Count;

            dictionaryChanged = false;
        }
    }

    /// <summary>
    /// Primitive drawer extensions
    /// </summary>
    public static class PrimitiveListDrawerExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <typeparam name="T">Primitive type</typeparam>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<PrimitiveListDrawer<T>> AddComponentPrimitiveListDrawer<T>(this Scene scene, string name, PrimitiveListDrawerDescription<T> description, SceneObjectUsages usage = SceneObjectUsages.None, int layer = Scene.LayerDefault) where T : IVertexList
        {
            PrimitiveListDrawer<T> component = null;

            await Task.Run(() =>
            {
                component = new PrimitiveListDrawer<T>(name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
