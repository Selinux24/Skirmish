using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests.Common
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class SharpDXExtensionsTests
    {
        static TestContext _testContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void VerticesAndEdgesTest()
        {
            var aabb = new BoundingBox(-Vector3.One, Vector3.One);
            var obb = new OrientedBoundingBox(aabb);

            var aabbCorners = aabb.GetVertices().ToArray();
            var obbCorners = obb.GetVertices().ToArray();

            CollectionAssert.AreEqual(aabbCorners, obbCorners);

            var aabbFaces = aabb.GetFaces().ToArray();
            var obbFaces = obb.GetFaces().ToArray();

            CollectionAssert.AreEqual(aabbFaces, obbFaces);
        }
    }
}
