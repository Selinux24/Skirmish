using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation
{
    using Engine.PathFinding.RecastNavigation.Detour;

    /// <summary>
    /// Graph node
    /// </summary>
    public class GraphNode : IGraphNode
    {
        /// <summary>
        /// Gets a graph node list from a navigation mesh
        /// </summary>
        /// <param name="mesh">Navigation mesh</param>
        /// <returns>Returns a list of graph nodes</returns>
        public static List<GraphNode> FindAll(NavMesh mesh)
        {
            List<GraphNode> nodes = [];

            if (mesh.TileCache != null)
            {
                var tileHeaders = mesh.TileCache
                    .GetTiles()
                    .Select(tile => tile.Header)
                    .Where(header => header.IsValid())
                    .ToArray();

                foreach (var header in tileHeaders)
                {
                    var tile = mesh.GetTileAt(header.TX, header.TY, header.TLayer);
                    if (tile == null)
                    {
                        continue;
                    }

                    nodes.AddRange(FindAllNodes(mesh, tile));
                }
            }
            else
            {
                var tiles = mesh.Tiles
                    .Take(mesh.MaxTiles)
                    .Where(tile => tile.Header.IsValid())
                    .ToArray();

                foreach (var tile in tiles)
                {
                    nodes.AddRange(FindAllNodes(mesh, tile));
                }
            }

            return nodes;
        }
        /// <summary>
        /// Gets a graph nodes from tile
        /// </summary>
        /// <param name="mesh">Navigation mesh</param>
        /// <param name="tile">Tile</param>
        /// <returns>Returns a list of graph nodes</returns>
        private static List<GraphNode> FindAllNodes(NavMesh mesh, MeshTile tile)
        {
            List<GraphNode> nodes = [];

            var bse = mesh.GetTileRef(tile);
            int tileNum = mesh.DecodePolyIdTile(bse);

            var polys = tile
                .GetPolys()
                .Where(p => p.Type != PolyTypes.OffmeshConnection)
                .ToArray();

            var tris = polys.SelectMany(tile.GetDetailTris);

            nodes.Add(new()
            {
                Id = tileNum,
                Triangles = [.. tris],
                TotalCost = 1,
            });

            return nodes;
        }
        /// <summary>
        /// Finds the graph node which contains the specified point, from a navigation mesh
        /// </summary>
        /// <param name="mesh">Navigation mesh</param>
        /// <returns>Returns a graph node</returns>
        public static GraphNode FindNode(NavMesh mesh, Vector3 point)
        {
            if (mesh.TileCache != null)
            {
                var tileHeaders = mesh.TileCache
                    .GetTiles()
                    .Select(tile => tile.Header)
                    .Where(header => header.IsValid());

                foreach (var header in tileHeaders)
                {
                    var tile = mesh.GetTileAt(header.TX, header.TY, header.TLayer);
                    if (tile == null)
                    {
                        continue;
                    }

                    var node = FindNode(mesh, tile, point);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }
            else
            {
                var tiles = mesh.Tiles
                    .Take(mesh.MaxTiles)
                    .Where(tile => tile.Header.IsValid());

                foreach (var tile in tiles)
                {
                    var node = FindNode(mesh, tile, point);
                    if (node != null)
                    {
                        return node;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Finds the graph node which contains the specified point, from tile
        /// </summary>
        /// <param name="mesh">Navigation mesh</param>
        /// <param name="tile">Tile</param>
        /// <returns>Returns a graph node</returns>
        private static GraphNode FindNode(NavMesh mesh, MeshTile tile, Vector3 point)
        {
            var bse = mesh.GetTileRef(tile);
            int tileNum = mesh.DecodePolyIdTile(bse);

            var polys = tile
                .GetPolys()
                .Where(p => p.Type != PolyTypes.OffmeshConnection)
                .ToArray();

            List<Triangle> tris = [];

            foreach (var p in polys)
            {
                var dtris = tile.GetDetailTris(p);

                if (!Intersection.PointInMesh(point, dtris))
                {
                    continue;
                }

                tris.AddRange(dtris);
            }

            return new()
            {
                Id = tileNum,
                Triangles = tris,
                TotalCost = 1,
            };
        }

        /// <inheritdoc/>
        public int Id { get; set; }
        /// <inheritdoc/>
        public float TotalCost { get; set; }
        /// <inheritdoc/>
        public Vector3 Center
        {
            get
            {
                Vector3 center = Vector3.Zero;

                foreach (var pos in Triangles.Select(tri => tri.GetCenter()))
                {
                    center += pos;
                }

                return center / Math.Max(1, Triangles.Count());
            }
        }
        /// <summary>
        /// Node triangle list
        /// </summary>
        public IEnumerable<Triangle> Triangles { get; private set; }

        /// <inheritdoc/>
        public bool Contains(Vector3 point)
        {
            return Intersection.PointInMesh(point, Triangles);
        }
        /// <inheritdoc/>
        public IEnumerable<Vector3> GetPoints()
        {
            return Triangles.SelectMany(t => t.GetVertices());
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Center: {Center}; Cost: {TotalCost:0.00}";
        }
    }
}
