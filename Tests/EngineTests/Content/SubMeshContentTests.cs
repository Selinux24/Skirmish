using Engine;
using Engine.Common;
using Engine.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests.Content
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class SubMeshContentTests
    {
        static TestContext _testContext;

        static BoundingBox cubeBig;
        static BoundingBox cubeMedium;
        static BoundingBox cubeSmall;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            Vector3 vBig = Vector3.One * 2f;
            Vector3 vMedium = Vector3.One;
            Vector3 vSmall = Vector3.One * 0.5f;

            cubeBig = new BoundingBox(-vBig, vBig);
            cubeMedium = new BoundingBox(-vMedium, vMedium);
            cubeSmall = new BoundingBox(-vSmall, vSmall);
        }
        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            var desc = GeometryUtil.CreateBox(Topology.TriangleList, cubeMedium);
            var data = VertexData.FromDescriptor(desc);

            SubMeshContent content1 = new(Topology.TriangleList, null, false, false, Matrix.Identity);
            content1.SetVertices(data);
            var tris1 = content1.GetTriangles();
            var points1 = tris1.SelectMany(t => t.GetVertices()).ToArray();
            var bounds1 = BoundingBox.FromPoints(points1);
            Assert.AreEqual(cubeMedium, bounds1);

            SubMeshContent content2 = new(Topology.TriangleList, null, false, false, Matrix.Scaling(2));
            content2.SetVertices(data);
            var tris2 = content2.GetTriangles();
            var points2 = tris2.SelectMany(t => t.GetVertices()).ToArray();
            var bounds2 = BoundingBox.FromPoints(points2);
            Assert.AreEqual(cubeBig, bounds2);

            SubMeshContent content3 = new(Topology.TriangleList, null, false, false, Matrix.Scaling(0.5f));
            content3.SetVertices(data);
            var tris3 = content3.GetTriangles();
            var points3 = tris3.SelectMany(t => t.GetVertices()).ToArray();
            var bounds3 = BoundingBox.FromPoints(points3);
            Assert.AreEqual(cubeSmall, bounds3);
        }
    }
}