using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.PathFinding.AStar
{
    /// <summary>
    /// Grid input geometry
    /// </summary>
    public class GridInput : PathFinderInput
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fnc">Get triangles function</param>
        public GridInput(Func<IEnumerable<Triangle>> fnc) : base(fnc)
        {

        }

        /// <inheritdoc/>
        public override async Task<IGraph> CreateGraph(PathFinderSettings settings, Action<float> progressCallback = null)
        {
            var triangles = await GetTriangles();

            return await Task.Run(() => Grid.CreateGrid(settings, this, triangles, progressCallback));
        }

        /// <inheritdoc/>
        public override async Task Refresh()
        {
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<string> GetHash(PathFinderSettings settings)
        {
            var tris = await GetTriangles();
            return GridFile.GetHash(settings, tris);
        }
        /// <inheritdoc/>
        public override async Task<IGraph> Load(string fileName, string hash = null)
        {
            // Load file
            var file = await GridFile.Load(fileName);

            // Test hash
            if (!string.IsNullOrEmpty(hash) && file.Hash != hash)
            {
                return null;
            }

            return await GridFile.FromGraphFile(file, this);
        }
        /// <inheritdoc/>
        public override async Task Save(string fileName, IGraph graph)
        {
            if (graph is not Grid grid)
            {
                throw new ArgumentException($"Bad grid graph type: {graph}", nameof(graph));
            }

            await GridFile.Save(fileName, grid);
        }
    }
}
