using System;

namespace Engine.PathFinding.NavMesh
{
    [Flags]
    public enum FindPathOptions
    {
        None = 0x00,

        AnyAngle = 0x01
    }
}
