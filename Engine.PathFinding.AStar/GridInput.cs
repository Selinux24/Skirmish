using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid input geometry
    /// </summary>
    /// <remarks>
    /// Constructor
    /// </remarks>
    /// <param name="fnc">Get triangles function</param>
    public class GridInput(Func<IEnumerable<Triangle>> fnc) : PathFinderInput(fnc)
    {
        /// <inheritdoc/>
        public override async Task<IGraph> CreateGraphAsync(PathFinderSettings settings, Action<float> progressCallback = null)
        {
            var triangles = await GetTrianglesAsync();

            return await Task.Run(() => Grid.CreateGrid(settings, this, triangles, progressCallback));
        }
        /// <inheritdoc/>
        public override IGraph CreateGraph(PathFinderSettings settings, Action<float> progressCallback = null)
        {
            var triangles = GetTriangles();

            return Grid.CreateGrid(settings, this, triangles, progressCallback);
        }

        /// <inheritdoc/>
        public override async Task RefreshAsync()
        {
            await Task.CompletedTask;
        }
        /// <inheritdoc/>
        public override void Refresh()
        {
            //Not applicable
        }

        /// <inheritdoc/>
        public override async Task<string> GetHashAsync(PathFinderSettings settings)
        {
            var tris = await GetTrianglesAsync();
            return GridFile.GetHash(settings, tris);
        }
        /// <inheritdoc/>
        public override string GetHash(PathFinderSettings settings)
        {
            var tris = GetTriangles();
            return GridFile.GetHash(settings, tris);
        }
        /// <inheritdoc/>
        public override async Task<IGraph> LoadAsync(string fileName, string hash = null)
        {
            // Load file
            var file = await GridFile.LoadAsync(fileName);

            // Test hash
            if (!string.IsNullOrEmpty(hash) && file.Hash != hash)
            {
                return null;
            }

            return await GridFile.FromGraphFileAsync(file, this);
        }
        /// <inheritdoc/>
        public override IGraph Load(string fileName, string hash = null)
        {
            // Load file
            var file = GridFile.Load(fileName);

            // Test hash
            if (!string.IsNullOrEmpty(hash) && file.Hash != hash)
            {
                return null;
            }

            return GridFile.FromGraphFile(file, this);
        }
        /// <inheritdoc/>
        public override async Task SaveAsync(string fileName, IGraph graph)
        {
            if (graph is not Grid grid)
            {
                throw new ArgumentException($"Bad grid graph type: {graph}", nameof(graph));
            }

            await GridFile.SaveAsync(fileName, grid);
        }
        /// <inheritdoc/>
        public override void Save(string fileName, IGraph graph)
        {
            if (graph is not Grid grid)
            {
                throw new ArgumentException($"Bad grid graph type: {graph}", nameof(graph));
            }

            GridFile.Save(fileName, grid);
        }
    }
}
