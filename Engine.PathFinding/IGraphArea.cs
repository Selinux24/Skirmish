﻿
namespace Engine.PathFinding
{
    /// <summary>
    /// Graph area interface
    /// </summary>
    public interface IGraphArea
    {
        /// <summary>
        /// Area id
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Area type
        /// </summary>
        GraphAreaTypes AreaType { get; set; }
    }
}