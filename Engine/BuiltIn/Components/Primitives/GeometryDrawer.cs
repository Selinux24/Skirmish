using Engine.BuiltIn.Drawers;
using Engine.BuiltIn.Primitives;
using Engine.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Primitives
{
    /// <summary>
    /// Geometry drawer
    /// </summary>
    /// <typeparam name="T">Geometry type</typeparam>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="scene">Scene</param>
    /// <param name="id">Id</param>
    /// <param name="name">Name</param>
    public sealed class GeometryDrawer<T>(Scene scene, string id, string name) : Drawable<GeometryDrawerDescription<T>>(scene, id, name), ITransformable3D where T : struct, IVertexData
    {
        const string className = nameof(GeometryDrawer<T>);

        /// <summary>
        /// Triangle dictionary by color
        /// </summary>
        private readonly ConcurrentBag<T> bag = [];
        /// <summary>
        /// Vertex type
        /// </summary>
        private readonly VertexTypes vertexType = default(T).VertexType;

        /// <summary>
        /// Topology
        /// </summary>
        private Topology topology;
        /// <summary>
        /// Material
        /// </summary>
        private IMeshMaterial material;
        /// <summary>
        /// Bag changed flag
        /// </summary>
        private bool bagChanged = false;
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Vertices to draw
        /// </summary>
        private int vertexCount = 0;

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
        ~GeometryDrawer()
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
        public override async Task ReadAssets(GeometryDrawerDescription<T> description)
        {
            await base.ReadAssets(description);

            if (description.Topology == Topology.Undefined)
            {
                throw new EngineException("Topology is not defined.");
            }

            topology = description.Topology;
            material = description.ReadMaterial();

            var primitives = description.Vertices ?? [];

            int count;
            if (primitives.Length > 0)
            {
                count = Description.Vertices.Length;

                foreach (var item in primitives)
                {
                    bag.Add(item);
                }
                bagChanged = true;
            }
            else
            {
                count = Description.Count;

                bagChanged = false;
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
            vertexBuffer = BufferManager.AddVertexData(name, true, new T[vertexCount]);
        }
        /// <summary>
        /// Set primitive
        /// </summary>
        /// <param name="primitive">Primitive</param>
        public void SetPrimitives(T primitive)
        {
            SetPrimitives([primitive]);
        }
        /// <summary>
        /// Set primitives list
        /// </summary>
        /// <param name="primitives">Primitives list</param>
        public void SetPrimitives(IEnumerable<T> primitives)
        {
            bag.Clear();

            if (!(primitives?.Any() ?? false))
            {
                return;
            }

            foreach (var item in primitives)
            {
                bag.Add(item);
            }
            bagChanged = true;
        }
        /// <summary>
        /// Add primitive to list
        /// </summary>
        /// <param name="primitive">primitive</param>
        public void AddPrimitives(T primitive)
        {
            AddPrimitives([primitive]);
        }
        /// <summary>
        /// Add primitives to list
        /// </summary>
        /// <param name="primitives">Primitives list</param>
        public void AddPrimitives(IEnumerable<T> primitives)
        {
            if (!(primitives?.Any() ?? false))
            {
                return;
            }

            foreach (var item in primitives)
            {
                bag.Add(item);
            }
            bagChanged = true;
        }
        /// <summary>
        /// Remove all
        /// </summary>
        public void Clear()
        {
            bag.Clear();
            bagChanged = true;
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

            WriteDataInBuffer(dc);

            if (vertexCount <= 0)
            {
                return false;
            }

            var drawer = BuiltInDrawer.GetDrawer(context.DrawerMode, vertexType, false);
            if (drawer == null)
            {
                return false;
            }

            drawer.UpdateMesh(dc, BuiltInDrawerMeshState.SetLocal(Manipulator.GlobalTransform));
            drawer.UpdateMaterial(dc, new BuiltInDrawerMaterialState()
            {
                Material = material,
            });

            bool drawn = drawer.Draw(dc, new DrawOptions
            {
                VertexBuffer = vertexBuffer,
                VertexDrawCount = vertexCount,
                Topology = topology,
            });

            return drawn;
        }
        /// <summary>
        /// Writes dictionary data in buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        public void WriteDataInBuffer(IEngineDeviceContext dc)
        {
            if (!bagChanged)
            {
                return;
            }

            var copy = bag.ToArray();

            if (!Game.WriteVertexBuffer(dc, vertexBuffer, copy))
            {
                return;
            }

            vertexCount = copy.Length;

            bagChanged = false;
        }
    }
}
