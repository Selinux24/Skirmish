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
        private static readonly Vector3 tankHullPos = new(0, 0, 0);
        private static readonly Vector3 tankTurretPos = new(0.4481661f, 6.813155f, -0.2201393f);
        private static readonly Vector3 tankBarrelPos = new(0, 0, -3.95312f);

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
            Matrix turretTrn = Matrix.RotationQuaternion(Quaternion.RotationAxis(Vector3.Up, MathUtil.Pi));
            Matrix barrelTrn = Matrix.Identity;

            model[0].Manipulator.SetTransform(instanceTrn);
            model[0].GetModelPartByName(tankTurretPart).Manipulator.SetTransform(turretTrn);

            //Local - must return the local * parent local transform
            var trnLocalHull = model[0].GetLocalTransformByName(tankHullPart);
            var trnLocalTurret = model[0].GetLocalTransformByName(tankTurretPart);
            var trnLocalBarrel = model[0].GetLocalTransformByName(tankBarrelPart);
            var expectedHull = hullTrn;
            var expectedTurret = turretTrn * expectedHull;
            var expectedBarrel = barrelTrn * expectedTurret;
            MatrixAreEqual(expectedHull, trnLocalHull);
            MatrixAreEqual(expectedTurret, trnLocalTurret);
            MatrixAreEqual(expectedBarrel, trnLocalBarrel);

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
            MatrixAreEqual(expectedHull, trnWorldHull);
            MatrixAreEqual(expectedTurret, trnWorldTurret);
            MatrixAreEqual(expectedBarrel, trnWorldBarrel);

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
            MatrixAreEqual(expectedHull, trnPartHull);
            MatrixAreEqual(expectedTurret, trnPartTurret);
            MatrixAreEqual(expectedBarrel, trnPartBarrel);

            //Pose - returns each part position
            var trnPoseHull = model[0].GetPoseTransformByName(tankHullPart);
            var trnPoseTurret = model[0].GetPoseTransformByName(tankTurretPart);
            var trnPoseBarrel = model[0].GetPoseTransformByName(tankBarrelPart);
            MatrixAreEqual(Matrix.Translation(tankHullPos), trnPoseHull);
            MatrixAreEqual(Matrix.Translation(tankHullPos + tankTurretPos), trnPoseTurret);
            MatrixAreEqual(Matrix.Translation(tankHullPos + tankTurretPos + tankBarrelPos), trnPoseBarrel);
        }

        private static void MatrixAreEqual(Matrix m1, Matrix m2)
        {
            Assert.AreEqual(m1.M11, m2.M11, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M12, m2.M12, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M13, m2.M13, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M14, m2.M14, MathUtil.ZeroTolerance);

            Assert.AreEqual(m1.M21, m2.M21, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M22, m2.M22, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M23, m2.M23, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M24, m2.M24, MathUtil.ZeroTolerance);

            Assert.AreEqual(m1.M31, m2.M31, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M32, m2.M32, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M33, m2.M33, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M34, m2.M34, MathUtil.ZeroTolerance);

            Assert.AreEqual(m1.M41, m2.M41, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M42, m2.M42, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M43, m2.M43, MathUtil.ZeroTolerance);
            Assert.AreEqual(m1.M44, m2.M44, MathUtil.ZeroTolerance);
        }
    }
}
