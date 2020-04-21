
namespace Engine.PathFinding.RecastNavigation.Recast
{
    public struct SweepSpan
    {
        /// <summary>
        /// row id
        /// </summary>
        public int RId { get; set; }
        /// <summary>
        /// region id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// number samples
        /// </summary>
        public int NS { get; set; }
        /// <summary>
        /// neighbour id
        /// </summary>
        public int Nei { get; set; }

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (RId != 0 || Id != 0 || NS != 0 || Nei != 0)
            {
                return string.Format(
                    "Row Id: {0}; Region Id: {1}; Samples: {2}; Neighbor Id: {3};",
                    this.RId, this.Id, this.NS, this.Nei);
            }
            else
            {
                return "Empty";
            }
        }
    }
}
