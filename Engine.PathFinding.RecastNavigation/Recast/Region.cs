using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Region
    /// </summary>
    public class Region
    {
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
        /// Initializes a region list
        /// </summary>
        /// <param name="nreg">Number of regions in the list</param>
        /// <returns></returns>
        public static List<Region> InitializeRegionList(int nreg)
        {
            var regions = new List<Region>(nreg);

            // Construct regions
            for (int i = 0; i < nreg; ++i)
            {
                regions.Add(new(i));
            }

            return regions;
        }
        /// <summary>
        /// Removes all the regions smaller than the specified area
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <param name="minRegionArea">Minimum region area</param>
        public static void RemoveSmallRegions(List<Region> regions, int minRegionArea)
        {
            for (int i = 0; i < regions.Count; ++i)
            {
                if (regions[i].SpanCount <= 0 || regions[i].SpanCount >= minRegionArea || regions[i].ConnectsToBorder)
                {
                    continue;
                }

                int reg = regions[i].Id;
                for (int j = 0; j < regions.Count; ++j)
                {
                    if (regions[j].Id == reg)
                    {
                        regions[j].Id = 0;
                    }
                }
            }
        }
        /// <summary>
        /// Removes smallest regions
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <param name="minRegionArea">Minimum region area</param>
        public static void RemoveSmallestRegions(List<Region> regions, int minRegionArea)
        {
            var stack = new List<int>();
            var trace = new List<int>();

            for (int i = 0; i < regions.Count; ++i)
            {
                var reg = regions[i];
                if (reg.Id == 0 || CompactHeightfield.IsBorder(reg.Id))
                {
                    continue;
                }
                if (reg.SpanCount == 0)
                {
                    continue;
                }
                if (reg.Visited)
                {
                    continue;
                }

                stack.Clear();
                trace.Clear();

                reg.Visited = true;
                stack.Add(i);

                // Count the total size of all the connected regions.
                // Also keep track of the regions connects to a tile border.
                var (spanCount, connectsToBorder) = ProcessRegions(regions, stack, trace);

                // If the accumulated regions size is too small, remove it.
                // Do not remove areas which connect to tile borders
                // as their size cannot be estimated correctly and removing them
                // can potentially remove necessary areas.
                if (spanCount >= minRegionArea || connectsToBorder)
                {
                    continue;
                }

                // Kill all visited regions.
                for (int j = 0; j < trace.Count; ++j)
                {
                    regions[trace[j]].SpanCount = 0;
                    regions[trace[j]].Id = 0;
                }
            }
        }
        private static (int spanCount, bool connectsToBorder) ProcessRegions(List<Region> regions, List<int> stack, List<int> trace)
        {
            bool connectsToBorder = false;
            int spanCount = 0;

            while (stack.Count > 0)
            {
                // Pop
                int ri = stack.PopLast();
                var creg = regions[ri];

                spanCount += creg.SpanCount;
                trace.Add(ri);

                foreach (var connection in creg.GetConnections())
                {
                    if (CompactHeightfield.IsBorder(connection))
                    {
                        connectsToBorder = true;
                        continue;
                    }

                    var neireg = regions[connection];
                    if (neireg.Visited)
                    {
                        continue;
                    }

                    if (neireg.Id == 0 || CompactHeightfield.IsBorder(neireg.Id))
                    {
                        continue;
                    }

                    // Visit
                    stack.Add(neireg.Id);
                    neireg.Visited = true;
                }
            }

            return (spanCount, connectsToBorder);
        }
        /// <summary>
        /// Compress region ids
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <returns>Returns the last region id</returns>
        public static int CompressRegionIds(List<Region> regions)
        {
            for (int i = 0; i < regions.Count; ++i)
            {
                regions[i].Remap = false;
                if (regions[i].Id == 0)
                {
                    // Skip nil regions.
                    continue;
                }
                if (CompactHeightfield.IsBorder(regions[i].Id))
                {
                    // Skip external regions.
                    continue;
                }
                regions[i].Remap = true;
            }

            int regIdGen = 0;
            for (int i = 0; i < regions.Count; ++i)
            {
                if (!regions[i].Remap)
                {
                    continue;
                }
                int oldId = regions[i].Id;
                int newId = ++regIdGen;
                for (int j = i; j < regions.Count; ++j)
                {
                    if (regions[j].Id == oldId)
                    {
                        regions[j].Id = newId;
                        regions[j].Remap = false;
                    }
                }
            }

            return regIdGen;
        }

        /// <summary>
        /// Gets whether a region can merge with another
        /// </summary>
        /// <param name="reg">Region</param>
        /// <returns>Returns true if the regions can merge</returns>
        public bool CanMergeWithRegion(Region reg)
        {
            if (AreaType != reg.AreaType)
            {
                return false;
            }

            int n = 0;
            for (int i = 0; i < connections.Count; ++i)
            {
                if (connections[i] == reg.Id)
                {
                    n++;
                }
            }

            if (n > 1)
            {
                return false;
            }

            for (int i = 0; i < floors.Count; ++i)
            {
                if (floors[i] == reg.Id)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Merges the specified region into the current region, and clears the specified region
        /// </summary>
        /// <param name="reg">Second region</param>
        /// <returns>Returns true if the regions merge</returns>
        public bool Merge(Region reg)
        {
            int aid = Id;
            int bid = reg.Id;

            // Duplicate current neighbourhood.
            var acon = new List<int>(connections);
            var bcon = reg.connections;

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
            connections.Clear();
            for (int i = 0, ni = acon.Count; i < ni - 1; ++i)
            {
                connections.Add(acon[(insa + 1 + i) % ni]);
            }

            for (int i = 0, ni = bcon.Count; i < ni - 1; ++i)
            {
                connections.Add(bcon[(insb + 1 + i) % ni]);
            }

            RemoveAdjacentNeighbours();

            for (int j = 0; j < reg.floors.Count; ++j)
            {
                AddUniqueFloorRegion(reg.floors[j]);
            }
            SpanCount += reg.SpanCount;
            reg.SpanCount = 0;
            reg.connections.Clear();

            return true;
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
        public void AddConnections(int[] neighbours)
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
        public int[] GetConnections()
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
        public int[] GetFloors()
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
        /// <summary>
        /// Gets whether the region overlaps with the specified neighbour
        /// </summary>
        /// <param name="nei">Neighbour index</param>
        public bool OverlapWithNeighbour(int nei)
        {
            return floors.Contains(nei);
        }
        /// <summary>
        /// Merges the specified region floors with current
        /// </summary>
        /// <param name="regn">Region</param>
        public void MergeFloors(Region regn)
        {
            if (regn == null)
            {
                return;
            }

            foreach (var floor in regn.floors)
            {
                AddUniqueFloorRegion(floor);
            }

            // Update bounds
            YMin = Math.Min(YMin, regn.YMin);
            YMax = Math.Max(YMax, regn.YMax);

            // Move the span count to current instance, and clears the other instace
            SpanCount += regn.SpanCount;
            regn.SpanCount = 0;

            // Updates border connection
            ConnectsToBorder = ConnectsToBorder || regn.ConnectsToBorder;
        }

        /// <summary>
        /// Merges small regions to nearest neighbours
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <param name="mergeRegionSize">Merge region size</param>
        public static void MergeSmallRegionsToNeighbours(List<Region> regions, int mergeRegionSize)
        {
            int mergeCount;
            do
            {
                mergeCount = 0;

                for (int i = 0; i < regions.Count; ++i)
                {
                    var reg = regions[i];

                    if (!reg.MergeIntoRegions(regions, mergeRegionSize))
                    {
                        continue;
                    }

                    mergeCount++;
                }
            }
            while (mergeCount > 0);
        }
        /// <summary>
        /// Merges current region into the specified valid region in the list
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <param name="mergeRegionSize">Merge region size</param>
        private bool MergeIntoRegions(List<Region> regions, int mergeRegionSize)
        {
            if (Id == 0)
            {
                return false;
            }
            if (CompactHeightfield.IsBorder(Id))
            {
                return false;
            }
            if (Overlap)
            {
                return false;
            }
            if (SpanCount == 0)
            {
                return false;
            }

            // Check to see if the region should be merged.
            if (SpanCount > mergeRegionSize && IsRegionConnectedToBorder())
            {
                return false;
            }

            // Small region with more than 1 connection.
            // Or region which is not connected to a border at all.
            // Find smallest neighbour region that connects to this one.
            int mergeId = FindSmallestNeighbourRegion(regions);
            if (mergeId == Id)
            {
                return false;
            }

            // Found new id.
            int oldId = Id;
            var target = regions[mergeId];

            // Merge neighbours.
            if (!target.Merge(this))
            {
                return false;
            }

            // Fixup regions pointing to current region.
            FixupRegions(regions, mergeId, oldId);

            return true;
        }
        /// <summary>
        /// Finds the smalles neighbour region in the specified region list
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <returns>Returns the region identifier</returns>
        private int FindSmallestNeighbourRegion(List<Region> regions)
        {
            int smallest = int.MaxValue;
            int mergeId = Id;
            foreach (var connection in GetConnections())
            {
                if (CompactHeightfield.IsBorder(connection))
                {
                    continue;
                }

                var mreg = regions[connection];
                if (mreg.Id == 0 || CompactHeightfield.IsBorder(mreg.Id) || mreg.Overlap)
                {
                    continue;
                }

                if (mreg.SpanCount < smallest && CanMergeWithRegion(mreg) && mreg.CanMergeWithRegion(this))
                {
                    smallest = mreg.SpanCount;
                    mergeId = mreg.Id;
                }
            }

            return mergeId;
        }
        /// <summary>
        /// Fixup region ids in the region list
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <param name="newId">New id</param>
        /// <param name="oldId">Old id</param>
        private static void FixupRegions(List<Region> regions, int newId, int oldId)
        {
            for (int j = 0; j < regions.Count; ++j)
            {
                if (regions[j].Id == 0 || CompactHeightfield.IsBorder(regions[j].Id))
                {
                    continue;
                }

                // If another region was already merged into current region
                // change the nid of the previous region too.
                if (regions[j].Id == oldId)
                {
                    regions[j].Id = newId;
                }

                // Replace the current region with the new one if the
                // current regions is neighbour.
                regions[j].ReplaceNeighbour(oldId, newId);
            }
        }

        /// <summary>
        /// Gets overlaping regions
        /// </summary>
        /// <param name="regions">Region list</param>
        /// <returns>Returns the overlaping region ids</returns>
        public static int[] GetOverlapingRegions(List<Region> regions)
        {
            return regions
                .Where(r => r.Overlap)
                .Select(r => r.Id)
                .ToArray();
        }

        /// <summary>
        /// Remap region ids
        /// </summary>
        /// <param name="regions">Region data list</param>
        /// <param name="srcReg">Region id list</param>
        /// <param name="nregs">Number of region ids in the list</param>
        public static void RemapRegions(List<Region> regions, int[] srcReg, int nregs)
        {
            for (int i = 0; i < nregs; ++i)
            {
                if (!CompactHeightfield.IsBorder(srcReg[i]))
                {
                    srcReg[i] = regions[srcReg[i]].Id;
                }
            }
        }
    }
}
