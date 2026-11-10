using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.Mathematics
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class OrientedBoundingBoxTests
    {
        public TestContext TestContext { get; set; }

        OrientedBoundingBox obb1;
        Vector3[] corners;
        Plane[] faces;

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");

            obb1 = new(-Vector3.One, Vector3.One);

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
            var aabbCorners = obb1.GetVertices();
            Assert.AreSequenceEqual(aabbCorners, corners);
        }
        [TestMethod()]
        public void EdgesTest()
        {
            var aabbFaces = obb1.GetFaces();
            Assert.AreSequenceEqual(aabbFaces, faces);
        }
    }
}
