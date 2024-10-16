﻿using Engine.BuiltIn.Components.Models;
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

        private const float tolerance = MathUtil.ZeroTolerance;

        private const string resourceTree = "Resources/Tree";
        private const string tree = "tree1.json";
        private const string treeBacked = "tree1_baked.json";
        private const string treeMeshName = "Tree_3-mesh";
        private const string treeBarkName = "Bark-material";
        private const string treeTreeName = "Tree-material";

        private const string resourceTank = "Resources/Tank";
        private const string tank = "Leopard.json";
        private const string tankBacked = "Leopard_backed.json";
        private const string tankBarrelPart = "Barrel-mesh";
        private const string tankTurretPart = "Turret-mesh";
        private const string tankHullPart = "Hull-mesh";

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
        [DataRow(resourceTree, tree)]
        [DataRow(resourceTree, treeBacked)]
        public async Task LoadTest(string resource, string fileName)
        {
            var contentDesc = ContentDescription.FromFile(resource, fileName);

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
            model[0].Manipulator.SetScaling(2);
            model[0].Manipulator.SetRotation(1, 0, 0);
            model[0].Manipulator.SetPosition(10, 10, 10);
            var sph = model[0].GetBoundingSphere(true);

            var content = await tDesc.Content.ReadContentData();
            var res = await content.First().CreateGeometry(true, true, null);
            var points1 = res[treeMeshName][treeBarkName].GetPoints();
            var points2 = res[treeMeshName][treeTreeName].GetPoints();
            List<Vector3> points = [.. points1, .. points2];
            var bounds = BoundingSphere.FromPoints(points.Distinct().ToArray());
            bounds = bounds.SetTransform(Matrix.Scaling(2) * Matrix.RotationYawPitchRoll(1, 0, 0) * Matrix.Translation(10, 10, 10));
            Assert.AreEqual(sph, bounds);
        }
        [TestMethod()]
        [DataRow("Default", resourceTank, tank, new[] { 0f, 0f, 0f }, new[] { 0.4481661f, 6.813155f, -0.2201393f }, new[] { 0, 0, -3.95312f })]
        [DataRow("Backed", resourceTank, tankBacked, new[] { 0f, 0f, 0f }, new[] { 0f, 0f, 0f }, new[] { 0f, 0f, 0f })]
        public async Task LoadWithPartsTest(string testCase, string resource, string fileName, float[] p1, float[] p2, float[] p3)
        {
            Vector3 tankHullPos = new(p1);
            Vector3 tankTurretPos = new(p2);
            Vector3 tankBarrelPos = new(p3);

            var tDesc = new ModelInstancedDescription()
            {
                Content = ContentDescription.FromFile(resource, fileName),
                Instances = 1,
                Optimize = false,
                PickingHull = PickingHullTypes.Hull,
                CastShadow = ShadowCastingAlgorihtms.Directional | ShadowCastingAlgorihtms.Spot,
                StartsVisible = false,
                TransformNames = [tankBarrelPart, tankTurretPart, tankHullPart],
                TransformDependences = [1, 2, -1],
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
            AssertMatrix.AreEqual(expectedHull, trnLocalHull, tolerance, $"{testCase} => Local transform test hull failed");
            AssertMatrix.AreEqual(expectedTurret, trnLocalTurret, tolerance, $"{testCase} => Local transform test turret failed");
            AssertMatrix.AreEqual(expectedBarrel, trnLocalBarrel, tolerance, $"{testCase} => Local transform test barrel failed");

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
            AssertMatrix.AreEqual(expectedHull, trnWorldHull, tolerance, $"{testCase} => World transform test hull failed");
            AssertMatrix.AreEqual(expectedTurret, trnWorldTurret, tolerance, $"{testCase} => World transform test turret failed");
            AssertMatrix.AreEqual(expectedBarrel, trnWorldBarrel, tolerance, $"{testCase} => World transform test barrel failed");

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
            AssertMatrix.AreEqual(expectedHull, trnPartHull, tolerance, $"{testCase} => Part transform test hull failed");
            AssertMatrix.AreEqual(expectedTurret, trnPartTurret, tolerance, $"{testCase} => Part transform test turret failed");
            AssertMatrix.AreEqual(expectedBarrel, trnPartBarrel, tolerance, $"{testCase} => Part transform test barrel failed");

            //Pose - returns each part position
            var trnPoseHull = model[0].GetPoseTransformByName(tankHullPart);
            var trnPoseTurret = model[0].GetPoseTransformByName(tankTurretPart);
            var trnPoseBarrel = model[0].GetPoseTransformByName(tankBarrelPart);
            AssertMatrix.AreEqual(Matrix.Translation(tankHullPos), trnPoseHull, tolerance, $"{testCase} => Pose transform test hull failed");
            AssertMatrix.AreEqual(Matrix.Translation(tankHullPos + tankTurretPos), trnPoseTurret, tolerance, $"{testCase} => Pose transform test turret failed");
            AssertMatrix.AreEqual(Matrix.Translation(tankHullPos + tankTurretPos + tankBarrelPos), trnPoseBarrel, tolerance, $"{testCase} => Pose transform test barrel failed");
        }
    }
}
