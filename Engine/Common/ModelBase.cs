﻿using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine.Common
{
    using Engine.Content;

    /// <summary>
    /// Model basic implementation
    /// </summary>
    public abstract class ModelBase : Drawable
    {
        /// <summary>
        /// Meshes by level of detail dictionary
        /// </summary>
        private LODDictionary meshesByLOD = new LODDictionary();
        /// <summary>
        /// Default level of detail
        /// </summary>
        private readonly LevelOfDetailEnum defaultLevelOfDetail = LevelOfDetailEnum.None;
        /// <summary>
        /// Level of detail
        /// </summary>
        public virtual LevelOfDetailEnum LevelOfDetail { get; set; }
        /// <summary>
        /// Gets the texture count for texture index
        /// </summary>
        public int TextureCount { get; private set; }

        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="content">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelBase(Game game, ModelContent content, bool instanced = false, int instances = 0, bool loadAnimation = true, bool loadNormalMaps = true, bool dynamic = false)
            : base(game)
        {
            var desc = new DrawingDataDescription()
            {
                Instanced = instanced,
                Instances = instances,
                LoadAnimation = loadAnimation,
                LoadNormalMaps = loadNormalMaps,
                TextureCount = this.TextureCount,
                DynamicBuffers = dynamic,
            };

            var drawable = DrawingData.Build(game, content, desc);

            this.meshesByLOD.Add(LevelOfDetailEnum.High, drawable);

            this.defaultLevelOfDetail = LevelOfDetailEnum.High;
            this.LevelOfDetail = LevelOfDetailEnum.High;
        }
        /// <summary>
        /// Base model
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="content">Model content</param>
        /// <param name="instanced">Is instanced</param>
        /// <param name="instances">Instance count</param>
        /// <param name="loadAnimation">Sets whether the load phase attemps to read skinning data</param>
        /// <param name="loadNormalMaps">Sets whether the load phase attemps to read normal mappings</param>
        /// <param name="dynamic">Sets whether the buffers must be created inmutables or not</param>
        public ModelBase(Game game, LODModelContent content, bool instanced = false, int instances = 0, bool loadAnimation = true, bool loadNormalMaps = true, bool dynamic = false)
            : base(game)
        {
            var desc = new DrawingDataDescription()
            {
                Instanced = instanced,
                Instances = instances,
                LoadAnimation = loadAnimation,
                LoadNormalMaps = loadNormalMaps,
                TextureCount = this.TextureCount,
                DynamicBuffers = dynamic,
            };

            foreach (var lod in content.Keys)
            {
                if (this.defaultLevelOfDetail == LevelOfDetailEnum.None)
                {
                    this.defaultLevelOfDetail = lod;
                }

                var drawable = DrawingData.Build(game, content[lod], desc);

                this.meshesByLOD.Add(lod, drawable);
            }

            this.LevelOfDetail = this.defaultLevelOfDetail;
        }

        /// <summary>
        /// Dispose model buffers
        /// </summary>
        public override void Dispose()
        {
            if (this.meshesByLOD != null)
            {
                foreach (var lod in this.meshesByLOD.Keys)
                {
                    this.meshesByLOD[lod].Dispose();
                }

                this.meshesByLOD.Clear();
                this.meshesByLOD = null;
            }
        }

        /// <summary>
        /// Adds the specified vertex buffer binding to all elements of the drawing data dictionary
        /// </summary>
        /// <param name="vertexBufferBinding">Binding to add</param>
        internal void AddVertexBufferBinding(VertexBufferBinding vertexBufferBinding)
        {
            foreach (var lod in this.meshesByLOD.Keys)
            {
                var drawingData = this.meshesByLOD[lod];

                foreach (var meshList in drawingData.Meshes.Values)
                {
                    foreach (var mesh in meshList)
                    {
                        if (mesh.Value.Instanced)
                        {
                            mesh.Value.AddVertexBufferBinding(vertexBufferBinding);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets the nearest level of detail for the specified level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the nearest level of detail for the specified level of detail</returns>
        internal LevelOfDetailEnum GetLODNearest(LevelOfDetailEnum lod)
        {
            if (this.meshesByLOD.ContainsKey(lod))
            {
                return lod;
            }
            else if (this.meshesByLOD.Keys.Count > 1)
            {
                int i = Helper.NextPowerOfTwo((int)lod + 1);

                for (int l = i; l < (int)LevelOfDetailEnum.Minimum + 1; l *= 2)
                {
                    if (this.meshesByLOD.ContainsKey((LevelOfDetailEnum)l))
                    {
                        return (LevelOfDetailEnum)l;
                    }
                }

                return this.defaultLevelOfDetail;
            }
            else
            {
                return this.defaultLevelOfDetail;
            }
        }
        /// <summary>
        /// Gets the minimum level of detail
        /// </summary>
        /// <returns>Returns the minimum level of detail</returns>
        internal LevelOfDetailEnum GetLODMinimum()
        {
            int l = int.MaxValue;

            foreach (var lod in this.meshesByLOD.Keys)
            {
                if ((int)lod < l)
                {
                    l = (int)lod;
                }
            }

            return (LevelOfDetailEnum)l;
        }
        /// <summary>
        /// Gets the maximum level of detail
        /// </summary>
        /// <returns>Returns the maximum level of detail</returns>
        internal LevelOfDetailEnum GetLODMaximum()
        {
            int l = int.MinValue;

            foreach (var lod in this.meshesByLOD.Keys)
            {
                if ((int)lod > l)
                {
                    l = (int)lod;
                }
            }

            return (LevelOfDetailEnum)l;
        }
        /// <summary>
        /// Gets the drawing data by level of detail
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <returns>Returns the drawing data object</returns>
        internal DrawingData GetDrawingData(LevelOfDetailEnum lod)
        {
            if (this.meshesByLOD.ContainsKey(lod))
            {
                return this.meshesByLOD[lod];
            }

            return null;
        }
        /// <summary>
        /// Changes the drawing data and updates internal state
        /// </summary>
        /// <param name="prevData">Previous drawing data</param>
        /// <param name="newLod">New level of detail</param>
        /// <returns>Returns the updated drawing data</returns>
        internal DrawingData ChangeDrawingData(DrawingData prevData, LevelOfDetailEnum newLod)
        {
            var newData = this.meshesByLOD[newLod];

            

            return newData;
        }
    }
}
