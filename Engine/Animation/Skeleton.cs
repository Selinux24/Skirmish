using System.Collections.Generic;

namespace Engine.Animation
{
    /// <summary>
    /// Skeleton class
    /// </summary>
    public class Skeleton
    {
        /// <summary>
        /// Root joint
        /// </summary>
        public Joint Root { get; private set; }
        /// <summary>
        /// Joint indices
        /// </summary>
        public int[] JointIndices { get; private set; }
        /// <summary>
        /// Joint names
        /// </summary>
        public string[] JointNames { get; private set; }

        /// <summary>
        /// Flatten skeleton
        /// </summary>
        /// <param name="joint">Joint</param>
        /// <param name="parentIndex">Parent joint index</param>
        /// <param name="indices">Joint indices</param>
        /// <param name="names">Joint names</param>
        private static void FlattenSkeleton(Joint joint, int parentIndex, List<int> indices, List<string> names)
        {
            indices.Add(parentIndex);

            int index = names.Count;

            names.Add(joint.Name);

            if (joint.Childs != null && joint.Childs.Length > 0)
            {
                for (int i = 0; i < joint.Childs.Length; i++)
                {
                    FlattenSkeleton(
                        joint.Childs[i],
                        index,
                        indices,
                        names);
                }
            }
        }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="root">Root joint</param>
        public Skeleton(Joint root)
        {
            List<int> indices = new List<int>();
            List<string> names = new List<string>();
            FlattenSkeleton(root, -1, indices, names);

            this.Root = root;
            this.JointIndices = indices.ToArray();
            this.JointNames = names.ToArray();
        }
    }
}
