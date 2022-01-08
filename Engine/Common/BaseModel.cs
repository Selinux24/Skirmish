﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Common
{
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
        private readonly Dictionary<LevelOfDetail, DrawingData> meshesByLOD = new Dictionary<LevelOfDetail, DrawingData>();
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
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropicFiltering { get; private set; }
        /// <inheritdoc/>
        public abstract ISkinningData SkinningData { get; }
        /// <summary>
        /// Use spheric volume for culling test
        /// </summary>
        public bool SphericVolume { get; private set; }

        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Object description</param>
        protected BaseModel(string id, string name, Scene scene, BaseModelDescription description)
            : base(id, name, scene, description)
        {
            if (description.Content == null)
            {
                throw new ArgumentException($"{nameof(description)} must have a {nameof(description.Content)} instance specified.", nameof(description));
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

            if (desc.Instanced)
            {
                InstancingBuffer = BufferManager.AddInstancingData($"{Name}.Instances", true, desc.Instances);
            }

            var geo = description.Content.ReadModelContent();
            if (!geo.Any())
            {
                throw new ArgumentException("Bad content description file. The resource file does not generate any geometry.", nameof(description));
            }

            if (geo.Count() == 1)
            {
                var iGeo = geo.First();

                if (description.Optimize) iGeo.Optimize();

                var drawable = DrawingData.Build(Game, Name, iGeo, desc, InstancingBuffer).GetAwaiter().GetResult();

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

                    var drawable = DrawingData.Build(Game, Name, content[lod], desc, InstancingBuffer).GetAwaiter().GetResult();

                    meshesByLOD.Add(lod, drawable);
                }
            }

            UseAnisotropicFiltering = description.UseAnisotropicFiltering;
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

                if (InstancingBuffer != null)
                {
                    BufferManager.RemoveInstancingData(InstancingBuffer);
                    InstancingBuffer = null;
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
        /// <remarks>If the sepecified level of detail not exists, returns the first available drawing data.</remarks>
        public DrawingData GetDrawingData(LevelOfDetail lod)
        {
            if (meshesByLOD == null)
            {
                return null;
            }

            if (meshesByLOD.ContainsKey(lod))
            {
                return meshesByLOD[lod];
            }

            return GetFirstDrawingData(LevelOfDetail.Minimum);
        }
        /// <summary>
        /// Gets the first drawing data avaliable for the specified level of detail, from the specified one
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
                if (meshesByLOD.ContainsKey(lod))
                {
                    return meshesByLOD[lod];
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

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            var drawingData = GetDrawingData(LevelOfDetail.High);
            if (drawingData == null)
            {
                return Enumerable.Empty<IMeshMaterial>();
            }

            return drawingData.Materials.Values.ToArray();
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            foreach (var meshMaterials in meshesByLOD.Values.Select(d => d.Materials))
            {
                var meshMaterial = meshMaterials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
                if (meshMaterial != null)
                {
                    return meshMaterials[meshMaterial];
                }
            }

            return null;
        }
        /// <inheritdoc/>
        public void ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            bool updated = false;

            foreach (var meshMaterials in meshesByLOD.Values.Select(d => d.Materials))
            {
                var meshMaterial = meshMaterials.Keys.FirstOrDefault(m => string.Equals(m, meshMaterialName, StringComparison.OrdinalIgnoreCase));
                if (meshMaterial != null)
                {
                    meshMaterials[meshMaterial] = material;
                    updated = true;
                }
            }

            if (updated)
            {
                Scene.UpdateMaterialPalette();
            }
        }
    }
}
