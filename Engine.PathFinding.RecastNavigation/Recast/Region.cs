using System.Collections.Generic;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Region
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets whether a region can merge with another
        /// </summary>
        /// <param name="rega">First region</param>
        /// <param name="regb">Second region</param>
        /// <returns>Returns true if the regions can merge</returns>
        public static bool CanMergeWithRegion(Region rega, Region regb)
        {
            if (rega.AreaType != regb.AreaType)
            {
                return false;
            }

            int n = 0;
            for (int i = 0; i < rega.connections.Count; ++i)
            {
                if (rega.connections[i] == regb.Id)
                {
                    n++;
                }
            }

            if (n > 1)
            {
                return false;
            }

            for (int i = 0; i < rega.floors.Count; ++i)
            {
                if (rega.floors[i] == regb.Id)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Merges the second region with the first region, and clears the second region
        /// </summary>
        /// <param name="rega">First region</param>
        /// <param name="regb">Second region</param>
        /// <returns>Returns true if the regions merge</returns>
        public static bool MergeRegions(Region rega, Region regb)
        {
            int aid = rega.Id;
            int bid = regb.Id;

            // Duplicate current neighbourhood.
            var acon = new List<int>(rega.connections);
            var bcon = regb.connections;

            // Find insertion point on A.
            int insa = -1;
            for (int i = 0; i < acon.Count; ++i)
            {
                if (acon[i] == bid)
                {
                    insa = i;
                    break;
                }
            }
            if (insa == -1)
            {
                return false;
            }

            // Find insertion point on B.
            int insb = -1;
            for (int i = 0; i < bcon.Count; ++i)
            {
                if (bcon[i] == aid)
                {
                    insb = i;
                    break;
                }
            }
            if (insb == -1)
            {
                return false;
            }

            // Merge neighbours.
            rega.connections.Clear();
            for (int i = 0, ni = acon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(acon[(insa + 1 + i) % ni]);
            }

            for (int i = 0, ni = bcon.Count; i < ni - 1; ++i)
            {
                rega.connections.Add(bcon[(insb + 1 + i) % ni]);
            }

            rega.RemoveAdjacentNeighbours();

            for (int j = 0; j < regb.floors.Count; ++j)
            {
                rega.AddUniqueFloorRegion(regb.floors[j]);
            }
            rega.SpanCount += regb.SpanCount;
            regb.SpanCount = 0;
            regb.connections.Clear();

            return true;
        }

        /// <summary>
        /// Connection list
        /// </summary>
        private readonly List<int> connections = new();
        /// <summary>
        /// Floor list
        /// </summary>
        private readonly List<int> floors = new();

        /// <summary>
        /// ID of the region
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Number of spans belonging to this region
        /// </summary>
        public int SpanCount { get; set; }
        /// <summary>
        /// Area type.
        /// </summary>
        public AreaTypes AreaType { get; set; }
        /// <summary>
        /// Remap flag
        /// </summary>
        public bool Remap { get; set; }
        /// <summary>
        /// Visited flag
        /// </summary>
        public bool Visited { get; set; }
        /// <summary>
        /// Overlap flag
        /// </summary>
        public bool Overlap { get; set; }
        /// <summary>
        /// Connect to border flag
        /// </summary>
        public bool ConnectsToBorder { get; set; }
        /// <summary>
        /// Minimum height
        /// </summary>
        public int YMin { get; set; }
        /// <summary>
        /// Maximum height
        /// </summary>
        public int YMax { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Region id</param>
        public Region(int id)
        {
            Id = id;
            SpanCount = 0;
            AreaType = AreaTypes.RC_NULL_AREA;
            Remap = false;
            Visited = false;
            Overlap = false;
            ConnectsToBorder = false;
            YMin = int.MaxValue;
            YMax = 0;
        }

        /// <summary>
        /// Adds an unique neighbour connection
        /// </summary>
        /// <param name="neighbour">Neighbour index</param>
        public void AddUniqueConnection(int neighbour)
        {
            if (connections.Contains(neighbour))
            {
                return;
            }

            connections.Add(neighbour);
        }
        /// <summary>
        /// Adds a neighbour list into the region
        /// </summary>
        /// <param name="neighbours">Neighbour list</param>
        /// <remarks>Removes adjacent neigbours after addition</remarks>
        public void AddConnections(IEnumerable<int> neighbours)
        {
            connections.AddRange(neighbours);

            RemoveAdjacentNeighbours();
        }
        /// <summary>
        /// Gets whether the region is connected to a border or not
        /// </summary>
        /// <returns>Returns true if the region is connected to a border</returns>
        public bool IsRegionConnectedToBorder()
        {
            // Region is connected to border if one of the neighbours is null id.
            return connections.Contains(0);
        }
        /// <summary>
        /// Removes adjacent neighbours
        /// </summary>
        public void RemoveAdjacentNeighbours()
        {
            // Remove adjacent duplicates.
            for (int i = 0; i < connections.Count && connections.Count > 1;)
            {
                // Next index
                int ni = (i + 1) % connections.Count;

                if (connections[i] == connections[ni])
                {
                    // Remove duplicate
                    for (int j = i; j < connections.Count - 1; j++)
                    {
                        connections[j] = connections[j + 1];
                    }
                    connections.RemoveAt(connections.Count - 1);
                }
                else
                {
                    i++;
                }
            }
        }
        /// <summary>
        /// Replaces a neighbour
        /// </summary>
        /// <param name="oldId">Old id</param>
        /// <param name="newId">New id</param>
        /// <remarks>Removes adjacent neigbours after replace</remarks>
        public void ReplaceNeighbour(int oldId, int newId)
        {
            bool neiChanged = false;
            for (int i = 0; i < connections.Count; ++i)
            {
                if (connections[i] == oldId)
                {
                    connections[i] = newId;
                    neiChanged = true;
                }
            }

            for (int i = 0; i < floors.Count; ++i)
            {
                if (floors[i] == oldId)
                {
                    floors[i] = newId;
                }
            }

            if (neiChanged)
            {
                RemoveAdjacentNeighbours();
            }
        }
        /// <summary>
        /// Adds an unique floor region
        /// </summary>
        /// <param name="floorId">Floor id</param>
        public void AddUniqueFloorRegion(int floorId)
        {
            if (floors.Contains(floorId))
            {
                return;
            }

            floors.Add(floorId);
        }
        /// <summary>
        /// Gets the connection list
        /// </summary>
        /// <returns>Returns the list of connections</returns>
        public IEnumerable<int> GetConnections()
        {
            return connections.ToArray();
        }
        /// <summary>
        /// Gets the connection count
        /// </summary>
        /// <returns>Returns the current connection count</returns>
        public int GetConnectionCount()
        {
            return connections.Count;
        }
        /// <summary>
        /// Gets the floor list
        /// </summary>
        /// <returns>Returns the list of floors</returns>
        public IEnumerable<int> GetFloors()
        {
            return floors.ToArray();
        }
        /// <summary>
        /// Gets the floor count
        /// </summary>
        /// <returns>Returns the current floor count</returns>
        public int GetFloorCount()
        {
            return floors.Count;
        }
    };
}
