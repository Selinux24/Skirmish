using Engine.Content;
using Engine.Content.FmtCollada;
using Engine.Content.FmtColladaTests;
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
        private static readonly Vector3 tankHullPos = new(0, 0, 0);
        private static readonly Vector3 tankTurretPos = new(0.4481661f, 6.813155f, -0.2201393f);
        private static readonly Vector3 tankBarrelPos = new(0, 0, -3.95312f);
        private const float tolerance = MathUtil.ZeroTolerance;

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
                Optimize = false,
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

            //Sets the transform of the current instance
            Matrix instanceTrn = Matrix.Translation(Vector3.One);
            Matrix hullTrn = Matrix.Identity;
            Matrix turretTrn = Matrix.RotationQuaternion(Quaternion.RotationAxis(Vector3.Up, MathUtil.PiOverTwo));
            Matrix barrelTrn = Matrix.RotationQuaternion(Quaternion.RotationAxis(Vector3.Left, MathUtil.PiOverFour));

            model[0].Manipulator.SetTransform(instanceTrn);
            model[0].GetModelPartByName(tankHullPart).Manipulator.SetTransform(hullTrn);
            model[0].GetModelPartByName(tankTurretPart).Manipulator.SetTransform(turretTrn);
            model[0].GetModelPartByName(tankBarrelPart).Manipulator.SetTransform(barrelTrn);

            //Local - must return the local * parent local transform
            var trnLocalHull = model[0].GetLocalTransformByName(tankHullPart);
            var trnLocalTurret = model[0].GetLocalTransformByName(tankTurretPart);
            var trnLocalBarrel = model[0].GetLocalTransformByName(tankBarrelPart);
            var expectedHull = hullTrn * instanceTrn;
            var expectedTurret = turretTrn * expectedHull;
            var expectedBarrel = barrelTrn * expectedTurret;
            AssertMatrix.AreEqual(expectedHull, trnLocalHull, tolerance, "Local transform test hull failed");
            AssertMatrix.AreEqual(expectedTurret, trnLocalTurret, tolerance, "Local transform test turret failed");
            AssertMatrix.AreEqual(expectedBarrel, trnLocalBarrel, tolerance, "Local transform test barrel failed");

            //World - returns each part position, transformed with the specified instance transform
            var trnWorldHull = model[0].GetWorldTransformByName(tankHullPart);
            var trnWorldTurret = model[0].GetWorldTransformByName(tankTurretPart);
            var trnWorldBarrel = model[0].GetWorldTransformByName(tankBarrelPart);
            expectedHull = hullTrn * instanceTrn;
            expectedTurret = turretTrn * expectedHull;
            expectedBarrel = barrelTrn * expectedTurret;
            expectedHull *= Matrix.Translation(tankHullPos);
            expectedTurret *= Matrix.Translation(tankTurretPos);
            expectedBarrel *= Matrix.Translation(tankBarrelPos);
            expectedHull = Matrix.Translation(Vector3.Zero) * expectedHull;
            expectedTurret = Matrix.Translation(tankHullPos) * expectedTurret;
            expectedBarrel = Matrix.Translation(tankTurretPos) * expectedBarrel;
            AssertMatrix.AreEqual(expectedHull, trnWorldHull, tolerance, "World transform test hull failed");
            AssertMatrix.AreEqual(expectedTurret, trnWorldTurret, tolerance, "World transform test turret failed");
            AssertMatrix.AreEqual(expectedBarrel, trnWorldBarrel, tolerance, "World transform test barrel failed");

            //Part - returns each part transformed with the instance transform
            var trnPartHull = model[0].GetPartTransformByName(tankHullPart);
            var trnPartTurret = model[0].GetPartTransformByName(tankTurretPart);
            var trnPartBarrel = model[0].GetPartTransformByName(tankBarrelPart);
            expectedHull = Matrix.Translation(-tankHullPos) * hullTrn * Matrix.Translation(tankHullPos);
            expectedTurret = Matrix.Translation(-tankTurretPos - tankHullPos) * turretTrn * Matrix.Translation(tankTurretPos + tankHullPos);
            expectedBarrel = Matrix.Translation(-tankBarrelPos - tankTurretPos - tankHullPos) * barrelTrn * Matrix.Translation(tankBarrelPos + tankTurretPos + tankHullPos);
            expectedHull *= instanceTrn;
            expectedTurret *= expectedHull;
            expectedBarrel *= expectedTurret;
            AssertMatrix.AreEqual(expectedHull, trnPartHull, tolerance, "Part transform test hull failed");
            AssertMatrix.AreEqual(expectedTurret, trnPartTurret, tolerance, "Part transform test turret failed");
            AssertMatrix.AreEqual(expectedBarrel, trnPartBarrel, tolerance, "Part transform test barrel failed");

            //Pose - returns each part position
            var trnPoseHull = model[0].GetPoseTransformByName(tankHullPart);
            var trnPoseTurret = model[0].GetPoseTransformByName(tankTurretPart);
            var trnPoseBarrel = model[0].GetPoseTransformByName(tankBarrelPart);
            AssertMatrix.AreEqual(Matrix.Translation(tankHullPos), trnPoseHull, tolerance, "Pose transform test hull failed");
            AssertMatrix.AreEqual(Matrix.Translation(tankHullPos + tankTurretPos), trnPoseTurret, tolerance, "Pose transform test turret failed");
            AssertMatrix.AreEqual(Matrix.Translation(tankHullPos + tankTurretPos + tankBarrelPos), trnPoseBarrel, tolerance, "Pose transform test barrel failed");
        }
    }
}
