using System.Collections.Generic;

namespace Engine.Geometry
{
    /// <summary>
    /// A Region contains a group of adjacent spans.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the number of spans
        /// </summary>
        public int SpanCount { get; set; }
        /// <summary>
        /// Gets or sets the region id 
        /// </summary>
        public RegionId Id { get; set; }
        /// <summary>
        /// Gets or sets the AreaType of this region
        /// </summary>
        public Area AreaType { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this region has been remapped or not
        /// </summary>
        public bool Remap { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this region has been visited or not
        /// </summary>
        public bool Visited { get; set; }
        /// <summary>
        /// Gets the list of floor regions
        /// </summary>
        public List<RegionId> FloorRegions { get; private set; }
        /// <summary>
        /// Gets the list of connected regions
        /// </summary>
        public List<RegionId> Connections { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the region is a border region.
        /// </summary>
        public bool IsBorder
        {
            get
            {
                return RegionId.HasFlags(Id, RegionFlags.Border);
            }
        }
        /// <summary>
        /// Gets a value indicating whether the region is either a border region or the null region.
        /// </summary>
        public bool IsBorderOrNull
        {
            get
            {
                return Id.IsNull || IsBorder;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Region" /> class.
        /// </summary>
        /// <param name="idNum">The id</param>
        public Region(int idNum)
        {
            SpanCount = 0;
            Id = new RegionId(idNum);
            AreaType = 0;
            Remap = false;
            Visited = false;

            Connections = new List<RegionId>();
            FloorRegions = new List<RegionId>();
        }

        /// <summary>
        /// Remove adjacent connections if there is a duplicate
        /// </summary>
        public void RemoveAdjacentNeighbors()
        {
            if (Connections.Count <= 1)
                return;

            // Remove adjacent duplicates.
            for (int i = 0; i < Connections.Count; i++)
            {
                //get the next i
                int ni = (i + 1) % Connections.Count;

                //remove duplicate if found
                if (Connections[i] == Connections[ni])
                {
                    Connections.RemoveAt(i);
                    i--;
                }
            }
        }
        /// <summary>
        /// Replace all connection and floor values 
        /// </summary>
        /// <param name="oldId">The value you want to replace</param>
        /// <param name="newId">The new value that will be used</param>
        public void ReplaceNeighbor(RegionId oldId, RegionId newId)
        {
            //replace the connections
            bool neiChanged = false;
            for (int i = 0; i < Connections.Count; ++i)
            {
                if (Connections[i] == oldId)
                {
                    Connections[i] = newId;
                    neiChanged = true;
                }
            }

            //replace the floors
            for (int i = 0; i < FloorRegions.Count; ++i)
            {
                if (FloorRegions[i] == oldId)
                    FloorRegions[i] = newId;
            }

            //make sure to remove adjacent neighbors
            if (neiChanged)
                RemoveAdjacentNeighbors();
        }
        /// <summary>
        /// Determine whether this region can merge with another region.
        /// </summary>
        /// <param name="otherRegion">The other region to merge with</param>
        /// <returns>True if the two regions can be merged, false if otherwise</returns>
        public bool CanMergeWith(Region otherRegion)
        {
            //make sure areas are the same
            if (AreaType != otherRegion.AreaType)
                return false;

            //count the number of connections to the other region
            int n = 0;
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i] == otherRegion.Id)
                    n++;
            }

            //make sure there's only one connection
            if (n > 1)
                return false;

            //make sure floors are separate
            if (FloorRegions.Contains(otherRegion.Id))
                return false;

            return true;
        }
        /// <summary>
        /// Only add a floor if it hasn't been added already
        /// </summary>
        /// <param name="n">The value of the floor</param>
        public void AddUniqueFloorRegion(RegionId n)
        {
            if (!FloorRegions.Contains(n))
                FloorRegions.Add(n);
        }
        /// <summary>
        /// Merge two regions into one. Needs good testing
        /// </summary>
        /// <param name="otherRegion">The region to merge with</param>
        /// <returns>True if merged successfully, false if otherwise</returns>
        public bool MergeWithRegion(Region otherRegion)
        {
            RegionId thisId = Id;
            RegionId otherId = otherRegion.Id;

            // Duplicate current neighborhood.
            List<RegionId> thisConnected = new List<RegionId>();
            for (int i = 0; i < Connections.Count; ++i)
                thisConnected.Add(Connections[i]);
            List<RegionId> otherConnected = otherRegion.Connections;

            // Find insertion point on this region
            int insertInThis = -1;
            for (int i = 0; i < thisConnected.Count; ++i)
            {
                if (thisConnected[i] == otherId)
                {
                    insertInThis = i;
                    break;
                }
            }

            if (insertInThis == -1)
                return false;

            // Find insertion point on the other region
            int insertInOther = -1;
            for (int i = 0; i < otherConnected.Count; ++i)
            {
                if (otherConnected[i] == thisId)
                {
                    insertInOther = i;
                    break;
                }
            }

            if (insertInOther == -1)
                return false;

            // Merge neighbors.
            Connections = new List<RegionId>();
            for (int i = 0, ni = thisConnected.Count; i < ni - 1; ++i)
                Connections.Add(thisConnected[(insertInThis + 1 + i) % ni]);

            for (int i = 0, ni = otherConnected.Count; i < ni - 1; ++i)
                Connections.Add(otherConnected[(insertInOther + 1 + i) % ni]);

            RemoveAdjacentNeighbors();

            for (int j = 0; j < otherRegion.FloorRegions.Count; ++j)
                AddUniqueFloorRegion(otherRegion.FloorRegions[j]);
            SpanCount += otherRegion.SpanCount;
            otherRegion.SpanCount = 0;
            otherRegion.Connections.Clear();

            return true;
        }
        /// <summary>
        /// Test if region is connected to a border
        /// </summary>
        /// <returns>True if connected, false if not</returns>
        public bool IsConnectedToBorder()
        {
            // Region is connected to border if
            // one of the neighbors is null id.
            for (int i = 0; i < Connections.Count; ++i)
            {
                if (Connections[i] == 0)
                    return true;
            }

            return false;
        }
    }
}
