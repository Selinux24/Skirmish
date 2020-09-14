using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Engine.Common
{
    using Engine.Animation;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Model basic implementation
    /// </summary>
    public abstract class BaseModel : Drawable, IUseMaterials, IUseSkinningData
    {
        /// <summary>
        /// Meshes by level of detail dictionary
        /// </summary>
        private readonly LevelOfDetailDictionary meshesByLOD = new LevelOfDetailDictionary();
        /// <summary>
        /// Default level of detail
        /// </summary>
        private readonly LevelOfDetail defaultLevelOfDetail = LevelOfDetail.Minimum;

        /// <summary>
        /// Instancing buffer
        /// </summary>
        protected BufferDescriptor InstancingBuffer { get; private set; } = null;

        /// <summary>
        /// Gets the texture count for texture index
        /// </summary>
        public int TextureCount { get; private set; }
        /// <summary>
        /// Gets the material list used by the current drawing data
        /// </summary>
        public IEnumerable<MeshMaterial> Materials
        {
            get
            {
                List<MeshMaterial> matList = new List<MeshMaterial>();

                var drawingData = this.GetDrawingData(LevelOfDetail.High);
                if (drawingData != null)
                {
                    foreach (var meshMaterial in drawingData.Materials.Keys)
                    {
                        matList.Add(drawingData.Materials[meshMaterial]);
                    }
                }

                return matList.ToArray();
            }
        }
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropicFiltering { get; set; }
        /// <summary>
        /// Gets the skinning list used by the current drawing data
        /// </summary>
        public SkinningData SkinningData
        {
            get
            {
                return this.GetDrawingData(LevelOfDetail.High)?.SkinningData;
            }
        }
        /// <summary>
        /// Use spheric volume for culling test
        /// </summary>
        public bool SphericVolume { get; set; }

        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Object description</param>
        protected BaseModel(Scene scene, BaseModelDescription description)
            : base(scene, description)
        {
            var desc = new DrawingDataDescription()
            {
                Instanced = description.Instanced,
                Instances = description.Instances,
                LoadAnimation = description.LoadAnimation,
                LoadNormalMaps = description.LoadNormalMaps,
                TextureCount = this.TextureCount,
                DynamicBuffers = description.Dynamic,
            };

            IEnumerable<ModelContent> geo;
            if (!string.IsNullOrEmpty(description.Content.ModelContentFilename))
            {
                string contentFile = Path.Combine(description.Content.ContentFolder, description.Content.ModelContentFilename);
                var contentDesc = SerializationHelper.DeserializeXmlFromFile<ModelContentDescription>(contentFile);
                var loader = GameResourceManager.GetLoaderForFile(contentDesc.ModelFileName);
                geo = loader.Load(description.Content.ContentFolder, contentDesc);
            }
            else if (description.Content.ModelContentDescription != null)
            {
                var contentDesc = description.Content.ModelContentDescription;
                var loader = GameResourceManager.GetLoaderForFile(contentDesc.ModelFileName);
                geo = loader.Load(description.Content.ContentFolder, contentDesc);
            }
            else if (description.Content.ModelContent != null)
            {
                geo = new[] { description.Content.ModelContent };
            }
            else
            {
                throw new EngineException("No geometry found in description.");
            }

            if (desc.Instanced)
            {
                InstancingBuffer = this.BufferManager.AddInstancingData($"{description.Name}.Instances", true, desc.Instances);
            }

            if (geo.Count() == 1)
            {
                var iGeo = geo.First();

                if (description.Optimize) iGeo.Optimize();

                var drawable = DrawingData.Build(this.Game, description.Name, iGeo, desc, InstancingBuffer);

                this.meshesByLOD.Add(LevelOfDetail.High, drawable);
            }
            else
            {
                var content = LevelOfDetailModelContent.Build(geo, description.Optimize);

                foreach (var lod in content.Keys)
                {
                    if (this.defaultLevelOfDetail == LevelOfDetail.None)
                    {
                        this.defaultLevelOfDetail = lod;
                    }

                    var drawable = DrawingData.Build(this.Game, description.Name, content[lod], desc, InstancingBuffer);

                    this.meshesByLOD.Add(lod, drawable);
                }
            }

            this.UseAnisotropicFiltering = description.UseAnisotropicFiltering;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~BaseModel()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose model buffers
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.meshesByLOD.Values.ToList().ForEach(m => m?.Dispose());
                this.meshesByLOD.Clear();

                if (this.InstancingBuffer != null)
                {
                    this.BufferManager.RemoveInstancingData(this.InstancingBuffer);
                    this.InstancingBuffer = null;
                }
            }
        }

        /// <summary>
        /// Gets the nearest level of detail for the specified level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the nearest level of detail for the specified level of detail</returns>
        internal LevelOfDetail GetLODNearest(LevelOfDetail lod)
        {
            if (this.meshesByLOD == null)
            {
                return LevelOfDetail.None;
            }

            if (this.meshesByLOD.Keys.Count == 0)
            {
                return this.defaultLevelOfDetail;
            }
            else
            {
                if (this.meshesByLOD.Keys.Count == 1)
                {
                    return this.meshesByLOD.Keys.First();
                }
                else
                {
                    int i = (int)lod;

                    for (int l = i; l > 0; l /= 2)
                    {
                        if (this.meshesByLOD.ContainsKey((LevelOfDetail)l))
                        {
                            return (LevelOfDetail)l;
                        }
                    }

                    return this.defaultLevelOfDetail;
                }
            }
        }
        /// <summary>
        /// Gets the minimum level of detail
        /// </summary>
        /// <returns>Returns the minimum level of detail</returns>
        internal LevelOfDetail GetLODMinimum()
        {
            if (this.meshesByLOD == null)
            {
                return LevelOfDetail.None;
            }

            int l = int.MaxValue;

            foreach (var lod in this.meshesByLOD.Keys)
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
        internal LevelOfDetail GetLODMaximum()
        {
            if (this.meshesByLOD == null)
            {
                return LevelOfDetail.None;
            }

            int l = int.MinValue;

            foreach (var lod in this.meshesByLOD.Keys)
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
        internal DrawingData GetDrawingData(LevelOfDetail lod)
        {
            if (this.meshesByLOD == null)
            {
                return null;
            }

            if (this.meshesByLOD.ContainsKey(lod))
            {
                return this.meshesByLOD[lod];
            }

            return null;
        }
        /// <summary>
        /// Gets the first drawing data avaliable for the specified level of detail, from the specified one
        /// </summary>
        /// <param name="lod">First level of detail</param>
        /// <returns>Returns the first available level of detail drawing data</returns>
        internal DrawingData GetFirstDrawingData(LevelOfDetail lod)
        {
            if (this.meshesByLOD == null)
            {
                return null;
            }

            while (lod > LevelOfDetail.None)
            {
                if (this.meshesByLOD.ContainsKey(lod))
                {
                    return this.meshesByLOD[lod];
                }

                lod = (LevelOfDetail)((int)lod / 2);
            }

            return null;
        }

        /// <summary>
        /// Gets the drawing effect for the current instance
        /// </summary>
        /// <param name="mode">Drawing mode</param>
        /// <returns>Returns the drawing effect</returns>
        protected IGeometryDrawer GetEffect(DrawerModes mode)
        {
            if (mode.HasFlag(DrawerModes.Forward))
            {
                return DrawerPool.EffectDefaultBasic;
            }
            else if (mode.HasFlag(DrawerModes.Deferred))
            {
                return DrawerPool.EffectDeferredBasic;
            }

            return null;
        }
    }
}
