﻿using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Drawers.Forward;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using SharpDX;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Primitives
{
    /// <summary>
    /// Geometry color list drawer
    /// </summary>
    /// <typeparam name="T">Geometry color list type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class GeometryColorDrawer<T>(Scene scene, string id, string name) : Drawable<GeometryColorDrawerDescription<T>>(scene, id, name), ITransformable3D where T : IVertexList
    {
        const string className = nameof(GeometryColorDrawer<T>);

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
        private readonly ConcurrentDictionary<Color4, List<T>> dictionary = new();
        /// <summary>
        /// Dictionary changes flag
        /// </summary>
        private bool dictionaryChanged = false;
        /// <summary>
        /// Item stride
        /// </summary>
        private int stride = 0;
        /// <summary>
        /// Item topology
        /// </summary>
        private Topology topology;
        /// <summary>
        /// Buffer exchange data list
        /// </summary>
        private List<VertexPositionColor> bufferData = [];

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
        /// <inheritdoc/>
        public IManipulator3D Manipulator { get; private set; } = new Manipulator3D();

        /// <summary>
        /// Destructor
        /// </summary>
        ~GeometryColorDrawer()
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
        public override async Task ReadAssets(GeometryColorDrawerDescription<T> description)
        {
            await base.ReadAssets(description);

            T tmp = default;
            stride = tmp.GetStride();
            topology = tmp.GetTopology();

            int count;
            if (Description.Primitives?.Length > 0)
            {
                bufferData = new List<VertexPositionColor>(Description.Primitives.Length);

                count = Description.Primitives.Length * stride;

                dictionary.TryAdd(Description.Color, new List<T>(Description.Primitives));
                dictionaryChanged = true;
            }
            else
            {
                bufferData = new List<VertexPositionColor>(Description.Count);

                count = Description.Count * stride;

                dictionaryChanged = false;
            }

            InitializeBuffers(Name, count);
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
            SetPrimitives(color, [primitive]);
        }
        /// <summary>
        /// Set primitives list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="primitives">Primitives list</param>
        public void SetPrimitives(Color4 color, IEnumerable<T> primitives)
        {
            if (primitives?.Any() ?? false)
            {
                if (!dictionary.TryGetValue(color, out var values))
                {
                    values = [];
                    dictionary.TryAdd(color, values);
                }
                else
                {
                    values.Clear();
                }

                values.AddRange(primitives);

                dictionaryChanged = true;

                return;
            }

            if (dictionary.ContainsKey(color))
            {
                dictionary.TryRemove(color, out _);

                dictionaryChanged = true;
            }
        }
        /// <summary>
        /// Set primitives list
        /// </summary>
        /// <param name="primitivesDict">Primitives by color dictionary</param>
        public void SetPrimitives(IDictionary<Color4, IEnumerable<T>> primitivesDict)
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
            AddPrimitives(color, [primitive]);
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
                dictionary.TryAdd(color, []);
            }

            dictionary[color].AddRange(primitives);

            dictionaryChanged = true;
        }
        /// <summary>
        /// Add primitives to list
        /// </summary>
        /// <param name="primitivesDict">Primitives by color dictionary</param>
        public void AddPrimitives(IDictionary<Color4, IEnumerable<T>> primitivesDict)
        {
            foreach (var primitive in primitivesDict)
            {
                AddPrimitives(primitive.Key, primitive.Value);
            }
        }
        /// <summary>
        /// Add primitives to list
        /// </summary>
        /// <param name="primitivesEnum">Primitives by color enumerable</param>
        public void AddPrimitives(IEnumerable<(Color4, IEnumerable<T>)> primitivesEnum)
        {
            foreach (var primitive in primitivesEnum)
            {
                AddPrimitives(primitive.Item1, primitive.Item2);
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

        /// <inheritdoc/>
        public void SetManipulator(IManipulator3D manipulator)
        {
            if (manipulator == null)
            {
                Logger.WriteWarning(this, $"{className} Name: {Name} - Sets a null manipulator. Discarded.");

                return;
            }

            Manipulator = manipulator;
        }

        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (!BuffersReady)
            {
                return false;
            }

            bool draw = context.ValidateDraw(BlendMode);
            if (!draw)
            {
                return false;
            }

            var dc = context.DeviceContext;

            WriteDataInBuffer(dc, context.Camera.Position);

            if (drawCount <= 0)
            {
                return false;
            }

            var drawer = BuiltInShaders.GetDrawer<BuiltInPositionColor>();
            if (drawer == null)
            {
                return false;
            }

            drawer.UpdateMesh(dc, BuiltInDrawerMeshState.SetLocal(Manipulator.GlobalTransform));
            drawer.UpdateMaterial(dc, BuiltInDrawerMaterialState.Default());

            bool drawn = drawer.Draw(dc, new DrawOptions
            {
                VertexBuffer = vertexBuffer,
                VertexDrawCount = drawCount,
                Topology = topology,
            });

            return drawn;
        }
        /// <summary>
        /// Writes dictionary data in buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        public void WriteDataInBuffer(IEngineDeviceContext dc, Vector3 eyePosition)
        {
            UpdateBufferData(eyePosition);

            if (!Game.WriteVertexBuffer(dc, vertexBuffer, [.. bufferData]))
            {
                return;
            }

            drawCount = bufferData.Count;

            dictionaryChanged = false;
        }
        /// <summary>
        /// Updates buffer data
        /// </summary>
        private void UpdateBufferData(Vector3 eyePosition)
        {
            if (!dictionaryChanged)
            {
                return;
            }

            bufferData.Clear();

            var copy = dictionary.ToArray();

            //Sort each primitive list by distance to eye
            var l = copy
                .SelectMany(p => p.Value.Select(v => new { Color = p.Key, Vertices = v.GetVertices().ToArray() }))
                .OrderByDescending(p => p.Vertices.Max(v => Vector3.Distance(v, eyePosition)));

            foreach (var item in l)
            {
                var color = item.Color;
                var primitives = item.Vertices;

                for (int i = 0; i < primitives.Length; i++)
                {
                    bufferData.Add(new()
                    {
                        Position = primitives[i],
                        Color = color
                    });
                }
            }
        }
    }
}