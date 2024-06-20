using Engine;
using Engine.Common;
using Engine.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests.Content
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContentDataTest
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
        public void DataTest()
        {
            var desc = GeometryUtil.CreateBox(Topology.TriangleList, cubeMedium);
            var data = VertexData.FromDescriptor(desc);
            SubMeshContent subMeshcontent = new(Topology.TriangleList, null, false, false, Matrix.Identity);
            subMeshcontent.SetVertices(data);
            Dictionary<string, SubMeshContent> geometryContent = new()
            {
                { "cube", subMeshcontent }
            };

            ContentData content = new();
            content.AddGeometryContent("default", geometryContent);

            var res = content.CreateGeometry(true, true, null).GetAwaiter().GetResult();
            var points = res["default"].First().Value.GetPoints();
            var bounds = BoundingBox.FromPoints(points.ToArray());

            Assert.AreEqual(cubeMedium, bounds);
        }

        [TestMethod()]
        public void DataWithTransformTest()
        {
            Dictionary<string, IMaterialContent> materialContent = new()
            {
                { "defaultMat1", MaterialBlinnPhongContent.Default },
                { "defaultMat2", MaterialBlinnPhongContent.Default },
            };

            var desc = GeometryUtil.CreateBox(Topology.TriangleList, cubeMedium);
            var data = VertexData.FromDescriptor(desc);
            SubMeshContent subMeshcontent1 = new(Topology.TriangleList, "defaultMat1", false, false, Matrix.Scaling(2));
            subMeshcontent1.SetVertices(data);
            SubMeshContent subMeshcontent2 = new(Topology.TriangleList, "defaultMat2", false, false, Matrix.Scaling(0.5f));
            subMeshcontent2.SetVertices(data);
            Dictionary<string, SubMeshContent> geometryContent = new()
            {
                { "cubeBig", subMeshcontent1 },
                { "cubeSmall", subMeshcontent2 },
            };

            ContentData content = new();
            content.AddMaterialContent(materialContent);
            content.AddGeometryContent("default", geometryContent);

            var res = content.CreateGeometry(true, true, null).GetAwaiter().GetResult();
            var points1 = res["default"]["defaultMat1"].GetPoints();
            var points2 = res["default"]["defaultMat2"].GetPoints();
            var bounds1 = BoundingBox.FromPoints(points1.ToArray());
            var bounds2 = BoundingBox.FromPoints(points2.ToArray());

            Assert.AreEqual(cubeBig, bounds1);
            Assert.AreEqual(cubeSmall, bounds2);

            var dd = DrawingData.Read(null, content, new()).GetAwaiter().GetResult();
            Assert.IsNotNull(dd);

            var points = dd.GetPoints();
            var bounds = BoundingBox.FromPoints(points.ToArray());
            Assert.AreEqual(cubeBig, bounds);
        }
    }
}