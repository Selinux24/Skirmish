using Engine.Content;
using Engine.Content.FmtCollada;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.ModularSceneryTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class LoaderTests
    {
        static TestContext _testContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            GameResourceManager.RegisterLoader<LoaderCollada>();

            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public async Task LoadFromFolderTest()
        {
            var tDesc = new ModelInstancedDescription()
            {
                Content = ContentDescription.FromFile("Resources", "tree1.json"),
                Instances = 10,
                Optimize = true,
                PickingHull = PickingHullTypes.Hull,
                CastShadow = ShadowCastingAlgorihtms.Directional | ShadowCastingAlgorihtms.Spot,
                StartsVisible = false,
            };

            Mock<Scene> mockScene = new(null);
            mockScene.SetupAllProperties();

            ModelInstanced model = new(mockScene.Object, "Tets", "Test");
            await model.ReadAssets(tDesc);
            model[0].Manipulator.SetScale(2);
            model[0].Manipulator.SetRotation(1, 0, 0);
            model[0].Manipulator.SetPosition(10, 10, 10);
            var sph = model[0].GetBoundingSphere(true);

            var content = await tDesc.Content.ReadContentData();
            var res = await content.First().CreateGeometry(true, true, null);
            var points1 = res["Tree_3-mesh"]["Bark-material"].GetPoints();
            var points2 = res["Tree_3-mesh"]["Tree-material"].GetPoints();
            List<Vector3> points = new();
            points.AddRange(points1);
            points.AddRange(points2);
            var bounds = BoundingSphere.FromPoints(points.Distinct().ToArray());
            bounds = bounds.SetTransform(Matrix.Scaling(2) * Matrix.RotationYawPitchRoll(1, 0, 0) * Matrix.Translation(10, 10, 10));
            Assert.AreEqual(sph, bounds);
        }
    }
}
