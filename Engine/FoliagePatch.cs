using Engine.Common;
using SharpDX;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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
        public const int MAX = 1024 * 4;

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
        /// Temporal planting data
        /// </summary>
        private readonly ConcurrentBag<VertexBillboard> tmpData = [];

        /// <summary>
        /// Patch id
        /// </summary>
        public readonly int Id = GetID();
        /// <summary>
        /// Foliage map channel
        /// </summary>
        public int Channel { get; protected set; } = -1;
        /// <summary>
        /// Foliage populated flag
        /// </summary>
        public bool Planted { get; protected set; } = false;
        /// <summary>
        /// Foliage populating flag
        /// </summary>
        public bool Planting { get; protected set; } = false;
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
        /// Calculates a list of points in the specified bounds
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Foliage descripton</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        /// <returns>Returns generated vertex data</returns>
        private void CalculatePoints(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox, Action callback)
        {
            Planting = true;

            tmpData.Clear();

            Random rnd = new(description.Seed);

            var rayList = CalculatePickingRays(scene, map, description, gbbox, nbbox, rnd);

            scene.PickFirstAsync<Triangle>(rayList, SceneObjectUsages.Ground, (res) =>
            {
                foreach (var (found, r) in res)
                {
                    Vector2 size = new(
                        rnd.NextFloat(description.MinSize.X, description.MaxSize.X),
                        rnd.NextFloat(description.MinSize.Y, description.MaxSize.Y));

                    if (found)
                    {
                        tmpData.Add(new()
                        {
                            Position = r.PickingResult.Position,
                            Size = size,
                        });
                    }
                }

                var array = tmpData.ToArray();
                Array.Copy(array, foliageData, array.Length);
                foliageCount = array.Length;

                Planting = false;
                Planted = true;

                callback?.Invoke();
            });
        }
        /// <summary>
        /// Calculates a picking ray list to test over terrain
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Foliage descripton</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        /// <param name="rnd">Randomizer</param>
        private static IEnumerable<PickingRay> CalculatePickingRays(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox, Random rnd)
        {
            int count = (int)MathF.Min(MAX, MAX * description.Density);

            for (int i = 0; i < count; i++)
            {
                if (!CalculatePoint(map, description, gbbox, nbbox, rnd, out var pos))
                {
                    continue;
                }

                var ray = scene.GetTopDownRay(pos, PickingHullTypes.FacingOnly | PickingHullTypes.Geometry);

                yield return ray;
            }
        }
        /// <summary>
        /// Calculates a point
        /// </summary>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Foliage descripton</param>
        /// <param name="gbbox">Relative bounding box to plant</param>
        /// <param name="nbbox">Node box</param>
        /// <param name="rnd">Randomizer</param>
        /// <param name="pos">Resulting point</param>
        private static bool CalculatePoint(FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox, Random rnd, out Vector3 pos)
        {
            Vector2 min = new(gbbox.Minimum.X, gbbox.Minimum.Z);
            Vector2 max = new(gbbox.Maximum.X, gbbox.Maximum.Z);

            pos = new(
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

            return plant;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FoliagePatch()
        {

        }

        /// <summary>
        /// Launches foliage population
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="map">Foliage map</param>
        /// <param name="description">Terrain vegetation description</param>
        /// <param name="gbbox">Global bounding box</param>
        /// <param name="nbbox">Node bounding box</param>
        public void Plant(Scene scene, FoliageMap map, FoliageMapChannel description, BoundingBox gbbox, BoundingBox nbbox, Action callback)
        {
            Channel = description.Index;

            CalculatePoints(scene, map, description, gbbox, nbbox, callback);
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
            return $"{Id}.Channel_{Channel} => Planted: {Planted}; HasData: {HasData}; Instances: {foliageCount}";
        }
    }
}
