
namespace Engine.PathFinding.RecastNavigation
{
    public struct SweepSpan
    {
        /// <summary>
        /// row id
        /// </summary>
        public int rid;
        /// <summary>
        /// region id
        /// </summary>
        public int id;
        /// <summary>
        /// number samples
        /// </summary>
        public int ns;
        /// <summary>
        /// neighbour id
        /// </summary>
        public int nei;

        /// <summary>
        /// Gets the text representation of the instance
        /// </summary>
        /// <returns>Returns the text representation of the instance</returns>
        public override string ToString()
        {
            if (rid != 0 || id != 0 || ns != 0 || nei != 0)
            {
                return string.Format(
                    "Row Id: {0}; Region Id: {1}; Samples: {2}; Neighbor Id: {3};",
                    this.rid, this.id, this.ns, this.nei);
            }
            else
            {
                return "Empty";
            }
        }
    }
}
