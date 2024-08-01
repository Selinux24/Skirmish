using AISamples.SceneCWRVirtualWorld.Content;
using Engine;
using SharpDX;

namespace AISamples.SceneCWRVirtualWorld.Items
{
    class Tree(Vector3 position, float radius, float height)
    {
        public Vector3 Position { get; set; } = position;
        public float Radius { get; set; } = radius;
        public float Height { get; set; } = height;

        public static TreeFile FromTree(Tree tree)
        {
            return new()
            {
                Position = Vector3File.FromVector3(tree.Position),
                Radius = tree.Radius,
                Height = tree.Height,
            };
        }
        public static Tree FromTreeFile(TreeFile tree)
        {
            var position = Vector3File.FromVector3File(tree.Position);
            var radius = tree.Radius;
            var height = tree.Height;

            return new(position, radius, height);
        }

        public float DistanceToPoint(Vector2 point)
        {
            return Vector2.Distance(Position.XZ(), point);
        }
    }
}
