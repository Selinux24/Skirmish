using SharpDX;
using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Store constants, structs, methods in this single class so that other classes can access this information.
    /// </summary>
    public class PathfindingCommon
    {
        public const int VERTS_PER_POLYGON = 6; //max number of vertices

        public const int STRAIGHTPATH_START = 0x01; //vertex is in start position of path
        public const int STRAIGHTPATH_END = 0x02; //vertex is in end position of path
        public const int STRAIGHTPATH_OFFMESH_CONNECTION = 0x04; //vertex is at start of an off-mesh connection
    }
}
