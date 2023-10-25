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

        private static readonly string tankBarrelPart = "Barrel-mesh";
        private static readonly string tankTurretPart = "Turret-mesh";
        private static readonly string tankHullPart = "Hull-mesh";

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
        public async Task LoadTest()
        {
            var contentDesc = ContentDescription.FromFile("Resources/Tree", "tree1.json");

            var tDesc = new ModelInstancedDescription()
            {
                Content = contentDesc,
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
        [TestMethod()]
        public async Task LoadBakedTest()
        {
            var contentBakedDesc = ContentDescription.FromFile("Resources/Tree", "tree1_baked.json");

            var tDesc = new ModelInstancedDescription()
            {
                Content = contentBakedDesc,
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

        [TestMethod()]
        public async Task LoadWithPartsTest()
        {
            var tDesc = new ModelInstancedDescription()
            {
                Content = ContentDescription.FromFile("Resources/Tank", "Leopard.json"),
                Instances = 1,
                Optimize = true,
                PickingHull = PickingHullTypes.Hull,
                CastShadow = ShadowCastingAlgorihtms.Directional | ShadowCastingAlgorihtms.Spot,
                StartsVisible = false,
                TransformNames = new[] { tankBarrelPart, tankTurretPart, tankHullPart },
                TransformDependences = new[] { 1, 2, -1 },
            };

            Mock<Scene> mockScene = new(null);
            mockScene.SetupAllProperties();

            ModelInstanced model = new(mockScene.Object, "Tets", "Test");
            await model.ReadAssets(tDesc);

            var trnLocalHull = model[0].GetLocalTransformByName(tankHullPart);
            var trnLocalTurret = model[0].GetLocalTransformByName(tankTurretPart);
            var trnLocalBarrel = model[0].GetLocalTransformByName(tankBarrelPart);

            Assert.AreEqual(Vector3.Zero, trnLocalHull.TranslationVector);
            Assert.AreEqual(Vector3.Zero, trnLocalTurret.TranslationVector);
            Assert.AreEqual(Vector3.Zero, trnLocalBarrel.TranslationVector);

            var trnGlobalHull = model[0].GetGlobalTransformByName(tankHullPart);
            var trnGlobalTurret = model[0].GetGlobalTransformByName(tankTurretPart);
            var trnGlobalBarrel = model[0].GetGlobalTransformByName(tankBarrelPart);

            Assert.AreEqual(Vector3.Zero, trnGlobalHull.TranslationVector);
            Assert.AreEqual(Vector3.Zero, trnGlobalTurret.TranslationVector);
            Assert.AreEqual(Vector3.Zero, trnGlobalBarrel.TranslationVector);
        }
    }
}
