using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    /// <summary>
    /// Foliage patch
    /// </summary>
    class FoliagePatch
    {
        /// <summary>
        /// Maximum number of elements in patch
        /// </summary>
        public const int MAX = 1024 * 8;

        /// <summary>
        /// Foliage patch id static counter
        /// </summary>
        private static int ID = 0;
        /// <summary>
        /// Gets the next instance Id
        /// </summary>
        /// <returns>Returns the next Instance Id</returns>
        private static int GetID()
        {
            return ++ID;
        }

        /// <summary>
        /// Foliage generated data
        /// </summary>
        private readonly VertexBillboard[] foliageData = new VertexBillboard[MAX];
        /// <summary>
        /// Foliage data count
        /// </summary>
        private int foliageCount = 0;

        /// <summary>
        /// Patch id
        /// </summary>
        public readonly int Id = 0;
        /// <summary>
        /// Foliage map channel
        /// </summary>
        public int Channel { get; protected set; }
        /// <summary>
        /// Foliage populating flag
        /// </summary>
        public bool Planting { get; protected set; }
        /// <summary>
        /// Foliage populated flag
        /// </summary>
        public bool Planted { get; protected set; }
        /// <summary>
        /// Returns true if the path has foliage data
        /// </summary>
        public bool HasData
        {
            get
            {
                return foliageCount > 0;
            }
        }

        /// <summary>
        /// Planting task
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Vegetation task</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        /// <returns>Returns generated vertex data</returns>
        private static VertexBillboard[] PlantNode(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox)
        {
            List<VertexBillboard> vertexData = new(MAX);

            Random rnd = new(description.Seed);
            int count = (int)MathF.Min(MAX, MAX * description.Density);

            Parallel.For(0, count, (index) =>
            {
                var v = CalculatePoint(scene, map, description, gbbox, nbbox, rnd);
                if (v.HasValue)
                {
                    vertexData.Add(v.Value);
                }
            });

            return [.. vertexData];
        }
        /// <summary>
        /// Calculates a planting point
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Vegetation task</param>
        /// <param name="gbbox">Relative bounding box to plant</param>
        /// <param name="nbbox">Node box</param>
        /// <param name="rnd">Randomizer</param>
        /// <returns>Returns the planting point</returns>
        private static VertexBillboard? CalculatePoint(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox, Random rnd)
        {
            VertexBillboard? result = null;

            Vector2 min = new(gbbox.Minimum.X, gbbox.Minimum.Z);
            Vector2 max = new(gbbox.Maximum.X, gbbox.Maximum.Z);

            //Attempts
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = new(
                    rnd.NextFloat(nbbox.Minimum.X, nbbox.Maximum.X),
                    nbbox.Maximum.Y + 1f,
                    rnd.NextFloat(nbbox.Minimum.Z, nbbox.Maximum.Z));

                bool plant = false;
                if (map != null)
                {
                    var c = map.GetRelative(pos, min, max);

                    if (c[description.Index] > 0)
                    {
                        plant = rnd.NextFloat(0, 1) < c[description.Index];
                    }
                }
                else
                {
                    plant = true;
                }

                if (plant)
                {
                    Vector2 size = new(
                        rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                        rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y));

                    var planted = Plant(scene, pos, size, out var res);
                    if (planted)
                    {
                        result = res;

                        break;
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// Plants one item
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="pos">Position</param>
        /// <param name="size">Size</param>
        /// <param name="res">Resulting item</param>
        /// <returns>Returns true if an item has been planted</returns>
        private static bool Plant(Scene scene, Vector3 pos, Vector2 size, out VertexBillboard res)
        {
            var ray = scene.GetTopDownRay(pos, PickingHullTypes.FacingOnly | PickingHullTypes.Geometry);

            bool found = scene.PickFirst<Triangle>(ray, SceneObjectUsages.Ground, out var r);
            if (found && r.PickingResult.Primitive.Normal.Y > 0.5f)
            {
                res = new VertexBillboard()
                {
                    Position = r.PickingResult.Position,
                    Size = size,
                };

                return true;
            }

            res = new VertexBillboard();

            return false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FoliagePatch()
        {
            Id = GetID();
            Planted = false;
            Planting = false;

            Channel = -1;
        }

        /// <summary>
        /// Launches foliage population asynchronous task
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Terrain vegetation description</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        public async Task PlantAsync(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox)
        {
            if (Planting)
            {
                return;
            }

            //Start planting task
            Planting = true;

            try
            {
                Channel = description.Index;

                var data = await Task.Run(() => PlantNode(scene, map, description, gbbox, nbbox));

                foliageCount = data.Length;
                Array.Copy(data, foliageData, foliageCount);

                Planted = true;
            }
            finally
            {
                Planting = false;
            }
        }
        /// <summary>
        /// Get foliage data
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        /// <param name="transparent">Use transparency</param>
        /// <returns>Returns the foliage data ordered by distance to eye position. Far first if transparency specified, near first otherwise</returns>
        public VertexBillboard[] GetData(Vector3 eyePosition, bool transparent)
        {
            if (foliageCount <= 0)
            {
                return [];
            }

            //Sort data
            Array.Sort(foliageData, (obj1, obj2) =>
            {
                int f = transparent ? -1 : 1;

                var d1 = f * Vector3.DistanceSquared(obj1.Position, eyePosition);
                var d2 = f * Vector3.DistanceSquared(obj2.Position, eyePosition);

                return d1.CompareTo(d2);
            });

            return foliageData.Take(foliageCount).ToArray();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Id}.Channle_{Channel} => Planting: {Planting} / Planted: {Planted}; HasData: {HasData}; Instances: {foliageCount}";
        }
    }
}
