using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class BoundingBoxTests
    {
        public TestContext TestContext { get; set; }

        BoundingBox aabb1;
        Vector3[] corners;
        Plane[] faces;

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");

            aabb1 = new(-Vector3.One, Vector3.One);

            corners =
            [
                new(){ X= 1, Y= 1, Z= 1 },
                new(){ X= 1, Y= 1, Z=-1 },
                new(){ X=-1, Y= 1, Z=-1 },
                new(){ X=-1, Y= 1, Z= 1 },
                new(){ X= 1, Y=-1, Z= 1 },
                new(){ X= 1, Y=-1, Z=-1 },
                new(){ X=-1, Y=-1, Z=-1 },
                new(){ X=-1, Y=-1, Z= 1 },
            ];

            faces =
            [
                new(){ Normal=new(){ X= 0, Y= 1, Z= 0 }, D=-1 },
                new(){ Normal=new(){ X= 0, Y=-1, Z= 0 }, D=-1 },
                new(){ Normal=new(){ X= 0, Y= 0, Z= 1 }, D=-1 },
                new(){ Normal=new(){ X= 0, Y= 0, Z=-1 }, D=-1 },
                new(){ Normal=new(){ X=-1, Y= 0, Z= 0 }, D=-1 },
                new(){ Normal=new(){ X= 1, Y= 0, Z= 0 }, D=-1 },
            ];
        }

        [TestMethod()]
        public void VerticesTest()
        {
            var aabbCorners = aabb1.GetVertices();
            Assert.AreSequenceEqual(aabbCorners, corners);
        }
        [TestMethod()]
        public void EdgesTest()
        {
            var aabbFaces = aabb1.GetFaces();
            Assert.AreSequenceEqual(aabbFaces, faces);
        }
        [TestMethod()]
        public void SubdivideOctreeSpanTest()
        {
            BoundingBox box = new(new Vector3(-1), new Vector3(1));
            Span<BoundingBox> subdivisions = stackalloc BoundingBox[8];

            box.SubdivideOctree(subdivisions, 0.1f);

            Assert.AreSequenceEqual(box.SubdivideOctree(0.1f), subdivisions.ToArray());
        }
    }
}
