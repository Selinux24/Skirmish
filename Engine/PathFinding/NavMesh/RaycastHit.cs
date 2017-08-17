using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace Engine.PathFinding.NavMesh
{
    /// <summary>
    /// Ray casting hit result
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RaycastHit
    {
        /// <summary>
        /// T
        /// </summary>
        public float T;
        /// <summary>
        /// Normal
        /// </summary>
        public Vector3 Normal;
        /// <summary>
        /// Edge index
        /// </summary>
        public int EdgeIndex;

        /// <summary>
        /// Returns true if the hit has contact
        /// </summary>
        public bool HasContact
        {
            get
            {
                return this.T >= 1f;
            }
        }
    }
}
