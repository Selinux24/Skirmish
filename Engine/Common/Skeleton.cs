using System.Collections.Generic;

namespace Engine.Common
{
    public class Skeleton
    {
        public Joint Root { get; private set; }

        public int[] JointIndices { get; private set; }

        public string[] JointNames { get; private set; }

        public Skeleton(Joint root)
        {
            List<int> indices = new List<int>();
            List<string> names = new List<string>();
            FlattenSkeleton(root, -1, indices, names);

            this.Root = root;
            this.JointIndices = indices.ToArray();
            this.JointNames = names.ToArray();
        }

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
    }
}
