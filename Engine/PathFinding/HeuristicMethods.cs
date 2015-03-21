
namespace Engine.PathFinding
{
    /// <summary>
    /// Calculation methods for PathFinder
    /// </summary>
    public enum HeuristicMethods
    {
        /// <summary>
        /// Euclidean distance between two points
        /// </summary>
        /// <remarks>Travel in any direction is allowed, rather than just along ranks, files and diagonals</remarks>
        Euclidean,
        /// <summary>
        /// Manhattan distance between two points
        /// </summary>
        /// <remarks>Distance on a square grid where one can only travel in horizontal and vertical directions</remarks>
        Manhattan,
        /// <summary>
        /// Chebyshev distance between two points
        /// </summary>
        /// <remarks>Distance in all directions is considered to have the same cost, as on a chessboard</remarks>
        DiagonalDistance1,
        /// <summary>
        /// Chebyshev distance between two points
        /// </summary>
        /// <remarks>Travel along the diagonals is considered to be slightly more expensive than along ranks and files</remarks>
        DiagonalDistance2,
        /// <summary>
        /// Hexagonal distance between two points
        /// </summary>
        /// <remarks>Assumes a hexagonal coordinate system in which one axis lies along the diagonals of the hexes</remarks>
        HexDistance,
    }
}
