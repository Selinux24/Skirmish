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
    [OSCondition(OperatingSystems.Windows)]
    public class SharpDXExtensionsTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void VerticesAndEdgesTest()
        {
            var aabb = new BoundingBox(-Vector3.One, Vector3.One);
            var obb = new OrientedBoundingBox(aabb);

            var aabbCorners = aabb.GetVertices().ToArray();
            var obbCorners = obb.GetVertices().ToArray();

            Assert.AreSequenceEqual(aabbCorners, obbCorners);

            var aabbFaces = aabb.GetFaces().ToArray();
            var obbFaces = obb.GetFaces().ToArray();

            Assert.AreSequenceEqual(aabbFaces, obbFaces);
        }
    }
}
