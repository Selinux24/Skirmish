
namespace Engine.PathFinding.RecastNavigation.Recast
{
    /// <summary>
    /// Build poligon detail parameters
    /// </summary>
    struct BuildPolyDetailParams
    {
        /// <summary>
        /// Sample distance
        /// </summary>
        public float SampleDist { get; set; }
        /// <summary>
        /// Sample maximum error
        /// </summary>
        public float SampleMaxError { get; set; }
        /// <summary>
        /// Height search radius
        /// </summary>
        public int HeightSearchRadius { get; set; }
    }
}
