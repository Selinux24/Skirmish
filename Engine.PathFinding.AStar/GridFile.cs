using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid file
    /// </summary>
    [Serializable]
    public struct GridFile
    {
        /// <summary>
        /// Calculates a hash string from a list of triangles
        /// </summary>
        /// <param name="settings">Settings</param>
        /// <param name="triangles">Triangle list</param>
        /// <returns>Returns the hash string</returns>
        public static string GetHash(PathFinderSettings settings, IEnumerable<Triangle> triangles)
        {
            List<byte> buffer = new();

            var tris = triangles.ToList();
            tris.Sort((t1, t2) =>
            {
                return t1.GetHashCode().CompareTo(t2.GetHashCode());
            });

            var serTris = tris
                .Select(t => new[]
                {
                    t.Point1.X, t.Point1.Y, t.Point1.Z,
                    t.Point2.X, t.Point2.Y, t.Point2.Z,
                    t.Point3.X, t.Point3.Y, t.Point3.Z,
                })
                .ToArray();

            buffer.AddRange(serTris.SerializeJson());
            buffer.AddRange(settings.SerializeJson());

            return buffer.ToArray().GetMd5Sum();
        }

        /// <summary>
        /// Creates a graph file from a graph
        /// </summary>
        /// <param name="grid">Graph</param>
        /// <returns>Returns the graph file</returns>
        public static async Task<GridFile> FromGridAsync(Grid grid)
        {
            var tris = await grid.Input.GetTrianglesAsync();

            //Calculate hash
            string hash = GetHash(grid.Settings, tris);

            var nodes = grid.Nodes.Select(GridNodeFile.FromNode).ToList();

            return new GridFile()
            {
                Settings = grid.Settings,
                Nodes = nodes,
                Hash = hash,
            };
        }
        /// <summary>
        /// Creates a graph file from a graph
        /// </summary>
        /// <param name="grid">Graph</param>
        /// <returns>Returns the graph file</returns>
        public static GridFile FromGrid(Grid grid)
        {
            var tris = grid.Input.GetTriangles();

            //Calculate hash
            string hash = GetHash(grid.Settings, tris);

            var nodes = grid.Nodes.Select(GridNodeFile.FromNode).ToList();

            return new GridFile()
            {
                Settings = grid.Settings,
                Nodes = nodes,
                Hash = hash,
            };
        }
        /// <summary>
        /// Creates a graph from a graph file
        /// </summary>
        /// <param name="file">Graph file</param>
        /// <param name="input">Input geometry</param>
        /// <returns>Returns the graph</returns>
        public static async Task<Grid> FromGraphFileAsync(GridFile file, GridInput input)
        {
            Grid grid = null;

            await Task.Run(() =>
            {
                var nodes = file.Nodes.Select(GridNodeFile.FromFile);

                var grid = new Grid(file.Settings, input);
                grid.SetNodes(nodes);
                grid.Initialized = true;
            });

            return grid;
        }
        /// <summary>
        /// Creates a graph from a graph file
        /// </summary>
        /// <param name="file">Graph file</param>
        /// <param name="input">Input geometry</param>
        /// <returns>Returns the graph</returns>
        public static Grid FromGraphFile(GridFile file, GridInput input)
        {
            var nodes = file.Nodes.Select(GridNodeFile.FromFile);

            var grid = new Grid(file.Settings, input);
            grid.SetNodes(nodes);
            grid.Initialized = true;

            return grid;
        }
        /// <summary>
        /// Loads the graph file from a file name
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the graph file</returns>
        public static async Task<GridFile> LoadAsync(string fileName)
        {
            try
            {
                var buffer = await File.ReadAllBytesAsync(fileName);

                return buffer.Decompress<GridFile>();
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(GridFile), "Error loading the graph from a file.", ex);

                throw;
            }
        }
        /// <summary>
        /// Loads the graph file from a file name
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Returns the graph file</returns>
        public static GridFile Load(string fileName)
        {
            try
            {
                var buffer = File.ReadAllBytes(fileName);

                return buffer.Decompress<GridFile>();
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(GridFile), "Error loading the graph from a file.", ex);

                throw;
            }
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph</param>
        public static async Task SaveAsync(string fileName, Grid graph)
        {
            try
            {
                var graphFile = await FromGridAsync(graph);

                var buffer = graphFile.Compress();

                await File.WriteAllBytesAsync(fileName, buffer);
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(GridFile), "Error saving the graph to a file.", ex);

                throw;
            }
        }
        /// <summary>
        /// Saves the graph to a file
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="graph">Graph</param>
        public static void Save(string fileName, Grid graph)
        {
            try
            {
                var graphFile = FromGrid(graph);

                var buffer = graphFile.Compress();

                File.WriteAllBytes(fileName, buffer);
            }
            catch (Exception ex)
            {
                Logger.WriteError(nameof(GridFile), "Error saving the graph to a file.", ex);

                throw;
            }
        }

        /// <summary>
        /// Graph settings
        /// </summary>
        public GridGenerationSettings Settings { get; set; }
        /// <summary>
        /// Grid node files
        /// </summary>
        public List<GridNodeFile> Nodes { get; set; }
        /// <summary>
        /// File source hash
        /// </summary>
        public string Hash { get; set; }
    }
}
