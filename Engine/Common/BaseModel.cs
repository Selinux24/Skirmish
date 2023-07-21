using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Deferred;
    using Engine.BuiltIn.Forward;
    using Engine.Content;

    /// <summary>
    /// Model basic implementation
    /// </summary>
    public abstract class BaseModel<T> : Drawable<T>, IUseMaterials, IUseSkinningData where T : BaseModelDescription
    {
        /// <summary>
        /// Meshes by level of detail dictionary
        /// </summary>
        private readonly Dictionary<LevelOfDetail, DrawingData> meshesByLOD = new();
        /// <summary>
        /// Default level of detail
        /// </summary>
        private LevelOfDetail defaultLevelOfDetail = LevelOfDetail.Minimum;

        /// <summary>
        /// Gets the texture count for texture index
        /// </summary>
        public int TextureCount { get; private set; }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropicFiltering { get; private set; }
        /// <inheritdoc/>
        public abstract ISkinningData SkinningData { get; }
        /// <summary>
        /// Culling volume for culling test
        /// </summary>
        public CullingVolumeTypes CullingVolumeType { get; private set; }
        /// <summary>
        /// Collider type for collision tests
        /// </summary>
        public ColliderTypes ColliderType { get; private set; }
        /// <inheritdoc/>
        public PickingHullTypes PickingHull { get; set; }
        /// <inheritdoc/>
        public PickingHullTypes PathFindingHull { get; set; }

        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        protected BaseModel(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseModel()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                meshesByLOD.Values.ToList().ForEach(m => m?.Dispose());
                meshesByLOD.Clear();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async Task InitializeAssets(T description)
        {
            await base.InitializeAssets(description);

            UseAnisotropicFiltering = description?.UseAnisotropicFiltering ?? false;
            CullingVolumeType = description?.CullingVolumeType ?? CullingVolumeTypes.SphericVolume;
            ColliderType = description?.ColliderType ?? ColliderTypes.None;
            PickingHull = description?.PickingHull ?? PickingHullTypes.Default;
            PathFindingHull = description?.PathFindingHull ?? PickingHullTypes.None;
        }
        /// <summary>
        /// Initializes model geometry
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        protected Task InitializeGeometry(T description, BufferDescriptor instancingBuffer = null)
        {
            if (description?.Content == null)
            {
                throw new ArgumentException($"{nameof(description)} must have a {nameof(description.Content)} instance specified.", nameof(description));
            }

            return InitializeGeometryInternal(description, instancingBuffer);
        }
        /// <summary>
        /// Initializes model geometry
        /// </summary>
        /// <param name="description">Description</param>
        /// <param name="instancingBuffer">Instancing buffer descriptor</param>
        private async Task InitializeGeometryInternal(T description, BufferDescriptor instancingBuffer = null)
        {
            var geo = await description.Content.ReadContentData();
            if (!geo.Any())
            {
                throw new EngineException("Bad content description file. The resource file does not generate any geometry.");
            }

            var desc = new DrawingDataDescription()
            {
                Instanced = description.Instanced,
                Instances = description.Instances,
                LoadAnimation = description.LoadAnimation,
                LoadNormalMaps = description.LoadNormalMaps,
                DynamicBuffers = description.Dynamic,

                TextureCount = TextureCount,
            };

            if (geo.Count() == 1)
            {
                var iGeo = geo.First();

                if (description.Optimize) iGeo.Optimize();

                var drawable = await DrawingData.Build(Game, Name, iGeo, desc, instancingBuffer);

                meshesByLOD.Add(LevelOfDetail.High, drawable);
            }
            else
            {
                var content = ContentData.BuildLOD(geo, description.Optimize);

                foreach (var lod in content.Keys)
                {
                    if (defaultLevelOfDetail == LevelOfDetail.None)
                    {
                        defaultLevelOfDetail = lod;
                    }

                    var drawable = await DrawingData.Build(Game, Name, content[lod], desc, instancingBuffer);

                    meshesByLOD.Add(lod, drawable);
                }
            }
        }

        /// <summary>
        /// Gets the nearest level of detail for the specified level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the nearest level of detail for the specified level of detail</returns>
        public LevelOfDetail GetLODNearest(LevelOfDetail lod)
        {
            if (meshesByLOD == null)
            {
                return LevelOfDetail.None;
            }

            if (meshesByLOD.Keys.Count == 0)
            {
                return defaultLevelOfDetail;
            }
            else
            {
                if (meshesByLOD.Keys.Count == 1)
                {
                    return meshesByLOD.Keys.First();
                }
                else
                {
                    int i = (int)lod;

                    for (int l = i; l > 0; l /= 2)
                    {
                        if (meshesByLOD.ContainsKey((LevelOfDetail)l))
                        {
                            return (LevelOfDetail)l;
                        }
                    }

                    return defaultLevelOfDetail;
                }
            }
        }
        /// <summary>
        /// Gets the minimum level of detail
        /// </summary>
        /// <returns>Returns the minimum level of detail</returns>
        public LevelOfDetail GetLODMinimum()
        {
            if (meshesByLOD == null)
            {
                return LevelOfDetail.None;
            }

            int l = int.MaxValue;

            foreach (var lod in meshesByLOD.Keys)
            {
                if ((int)lod < l)
                {
                    l = (int)lod;
                }
            }

            return (LevelOfDetail)l;
        }
        /// <summary>
        /// Gets the maximum level of detail
        /// </summary>
        /// <returns>Returns the maximum level of detail</returns>
        public LevelOfDetail GetLODMaximum()
        {
            if (meshesByLOD == null)
            {
                return LevelOfDetail.None;
            }

            int l = int.MinValue;

            foreach (var lod in meshesByLOD.Keys)
            {
                if ((int)lod > l)
                {
                    l = (int)lod;
                }
            }

            return (LevelOfDetail)l;
        }
        /// <summary>
        /// Gets the drawing data by level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the drawing data object</returns>
        /// <remarks>If the specified level of detail not exists, returns the first available drawing data.</remarks>
        public DrawingData GetDrawingData(LevelOfDetail lod)
        {
            if (meshesByLOD == null)
            {
                return null;
            }

            if (meshesByLOD.TryGetValue(lod, out var value))
            {
                return value;
            }

            return GetFirstDrawingData(LevelOfDetail.Minimum);
        }
        /// <summary>
        /// Gets the first drawing data available for the specified level of detail, from the specified one
        /// </summary>
        /// <param name="lod">First level of detail</param>
        /// <returns>Returns the first available level of detail drawing data</returns>
        public DrawingData GetFirstDrawingData(LevelOfDetail lod)
        {
            if (meshesByLOD == null)
            {
                return null;
            }

            while (lod > LevelOfDetail.None)
            {
                if (meshesByLOD.TryGetValue(lod, out var value))
                {
                    return value;
                }

                lod = (LevelOfDetail)((int)lod / 2);
            }

            return null;
        }

        /// <summary>
        /// Gets the drawing effect for the current instance
        /// </summary>
        /// <param name="mode">Drawing mode</param>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns the drawing effect</returns>
        protected IBuiltInDrawer GetDrawer(DrawerModes mode, VertexTypes vertexType, bool instanced)
        {
            if (mode.HasFlag(DrawerModes.Forward))
            {
                return ForwardDrawerManager.GetDrawer(vertexType, instanced);
            }

            if (mode.HasFlag(DrawerModes.Deferred))
            {
                return DeferredDrawerManager.GetDrawer(vertexType, instanced);
            }

            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            var drawingData = GetDrawingData(LevelOfDetail.High);

            return drawingData?.GetMaterials() ?? Enumerable.Empty<IMeshMaterial>();
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            foreach (var drawingData in meshesByLOD.Values)
            {
                var material = drawingData.GetFirstMaterial(meshMaterialName);
                if (material != null)
                {
                    return material;
                }
            }

            return null;
        }
        /// <inheritdoc/>
        public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            bool updated = false;

            foreach (var drawingData in meshesByLOD.Values)
            {
                if (drawingData.ReplaceMaterials(meshMaterialName, material))
                {
                    updated = true;
                }
            }

            return updated;
        }
    }
}
