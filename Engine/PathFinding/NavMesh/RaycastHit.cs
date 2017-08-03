using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RaycastHit
    {
        public float T;
        public Vector3 Normal;
        public int EdgeIndex;

        public bool IsHit
        {
            get
            {
                return T != float.MaxValue;
            }
        }
    }
}
