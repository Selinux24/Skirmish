using Engine;
using Engine.Common;
using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class RayPickingHelperTests
    {
        public TestContext TestContext { get; set; }

        static Mock<IRayPickable<Triangle>> mockQuad1;
        static Mock<IRayPickable<Triangle>> mockQuad2;
        static Mock<IRayPickable<Triangle>> mockQuad3;

        static Triangle t1_1;
        static Triangle t1_2;
        static Triangle t2_1;
        static Triangle t2_2;
        static Triangle t3_1;
        static Triangle t3_2;

        static BoundingSphere bsph1;
        static BoundingSphere bsph2;
        static BoundingSphere bsph3;

        static BoundingBox bbox1;
        static BoundingBox bbox2;
        static BoundingBox bbox3;

        static PickingRay rayDefault;
        static PickingRay rayDefaultReverse;
        static PickingRay rayDefaultNoContact;
        static PickingRay rayAllDirections;
        static PickingRay rayAllDirectionsReverse;
        static PickingRay rayAllDirectionsNoContact;
        static PickingRay rayCoarse;
        static PickingRay rayCoarseReverse;
        static PickingRay rayCoarseNoContact;

        static Vector3 toQuad1Position;
        static Vector3 toQuad2Position;
        static Vector3 toQuad3Position;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var ray = new Ray(Vector3.Zero, Vector3.ForwardLH);
            var rayReverse = new Ray(Vector3.ForwardLH * 4, Vector3.BackwardLH);
            var rayNoContact = new Ray(Vector3.Zero, Vector3.BackwardLH);

            rayDefault = new PickingRay(ray, PickingHullTypes.Default);
            rayDefaultReverse = new PickingRay(rayReverse, PickingHullTypes.Default);
            rayDefaultNoContact = new PickingRay(rayNoContact, PickingHullTypes.Default);

            rayAllDirections = new PickingRay(ray, PickingHullTypes.Geometry);
            rayAllDirectionsReverse = new PickingRay(rayReverse, PickingHullTypes.Geometry);
            rayAllDirectionsNoContact = new PickingRay(rayNoContact, PickingHullTypes.Geometry);

            rayCoarse = new PickingRay(ray, PickingHullTypes.Coarse);
            rayCoarseReverse = new PickingRay(rayReverse, PickingHullTypes.Coarse);
            rayCoarseNoContact = new PickingRay(rayNoContact, PickingHullTypes.Coarse);

            t1_1 = new Triangle(new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(-1, -1, 1));
            t1_2 = new Triangle(new Vector3(-1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1));

            t2_1 = new Triangle(new Vector3(-1, 1, 2), new Vector3(1, 1, 2), new Vector3(-1, -1, 2));
            t2_2 = new Triangle(new Vector3(-1, 1, 2), new Vector3(1, -1, 2), new Vector3(-1, -1, 2));

            t3_1 = new Triangle(new Vector3(-1, 1, 3), new Vector3(1, 1, 3), new Vector3(-1, -1, 3));
            t3_2 = new Triangle(new Vector3(-1, 1, 3), new Vector3(1, -1, 3), new Vector3(-1, -1, 3));

            var t1 = new[] { t1_1, t1_2, t2_1, t2_2, t3_1, t3_2 };
            var t3 = new[] { t3_1, t3_2, t2_1, t2_2, t1_1, t1_2 };
            var t2 = new[] { t2_1, t2_2, t3_1, t3_2, t1_1, t1_2 };

            var p1 = t1.SelectMany(t => t.GetVertices()).ToArray();
            var p2 = t2.SelectMany(t => t.GetVertices()).ToArray();
            var p3 = t3.SelectMany(t => t.GetVertices()).ToArray();

            bsph1 = BoundingSphere.FromPoints(p1);
            bsph2 = BoundingSphere.FromPoints(p2);
            bsph3 = BoundingSphere.FromPoints(p3);

            bbox1 = BoundingBox.FromPoints(p1);
            bbox2 = BoundingBox.FromPoints(p2);
            bbox3 = BoundingBox.FromPoints(p3);

            var bbox1Tris = GeometryUtil.ComputeTriangleList(bbox1);
            var bbox2Tris = GeometryUtil.ComputeTriangleList(bbox2);
            var bbox3Tris = GeometryUtil.ComputeTriangleList(bbox3);

            toQuad1Position = new Vector3(0, 0, 1);
            toQuad2Position = new Vector3(0, 0, 2);
            toQuad3Position = new Vector3(0, 0, 3);

            mockQuad1 = new Mock<IRayPickable<Triangle>>();
            mockQuad2 = new Mock<IRayPickable<Triangle>>();
            mockQuad3 = new Mock<IRayPickable<Triangle>>();

            Setup(mockQuad1, bsph1, bbox1Tris, t1);
            Setup(mockQuad2, bsph2, bbox1Tris, t2);
            Setup(mockQuad3, bsph3, bbox1Tris, t3);
        }
        private static void Setup(Mock<IRayPickable<Triangle>> pickableMock, BoundingSphere sphere, Triangle[] boxTris, Triangle[] mesh)
        {
            pickableMock.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(sphere);
            pickableMock.Setup(o => o.GetGeometry(GeometryTypes.Picking, It.IsAny<bool>())).Returns(mesh);
            pickableMock.Setup(o => o.GetGeometry(GeometryTypes.PathFinding, It.IsAny<bool>())).Returns(boxTris);
            pickableMock.As<ISceneObject>().SetupAllProperties();
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void PickNearestTestDefault1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefault2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefault3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultReverse1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayDefaultReverse, out _);

            Assert.IsFalse(picked);
        }
        [TestMethod()]
        public void PickNearestTestDefaultReverse2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayDefaultReverse, out _);

            Assert.IsFalse(picked);
        }
        [TestMethod()]
        public void PickNearestTestDefaultReverse3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayDefaultReverse, out _);

            Assert.IsFalse(picked);
        }
        [TestMethod()]
        public void PickNearestTestDefaultNoContact1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultNoContact2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultNoContact3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickNearestTestPerfect1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfect2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfect3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfectReverse1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(t3_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfectReverse2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(t3_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfectReverse3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(t3_1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfectNoContact1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfectNoContact2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestPerfectNoContact3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickNearestTestCoarse1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarse2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarse3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarseReverse1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarseReverse2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarseReverse3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarseNoContact1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarseNoContact2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestCoarseNoContact3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickFirstTestDefault1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestDefault2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(2, res.Distance);
            Assert.AreEqual(toQuad2Position, res.Position);
            Assert.AreEqual(t2_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestDefault3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(3, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(t3_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestDefaultReverse1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayDefaultReverse, out _);

            Assert.IsFalse(picked);
        }
        [TestMethod()]
        public void PickFirstTestDefaultReverse2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayDefaultReverse, out _);

            Assert.IsFalse(picked);
        }
        [TestMethod()]
        public void PickFirstTestDefaultReverse3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayDefaultReverse, out _);

            Assert.IsFalse(picked);
        }
        [TestMethod()]
        public void PickFirstTestDefaultNoContact1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestDefaultNoContact2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestDefaultNoContact3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickFirstTestPerfect1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfect2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(2, res.Distance);
            Assert.AreEqual(toQuad2Position, res.Position);
            Assert.AreEqual(t2_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfect3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(3, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(t3_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfectReverse1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);
            Assert.AreEqual(3, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(t1_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfectReverse2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);
            Assert.AreEqual(2, res.Distance);
            Assert.AreEqual(toQuad2Position, res.Position);
            Assert.AreEqual(t2_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfectReverse3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);
            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(t3_1, res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfectNoContact1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfectNoContact2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestPerfectNoContact3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickFirstTestCoarse1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarse2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarse3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarseReverse1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarseReverse2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarseReverse3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarseNoContact1()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarseNoContact2()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickFirstTestCoarseNoContact3()
        {
            bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickAllTestDefault1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad1Position, res[0].Position);
            Assert.AreEqual(t1_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad1Position, res[1].Position);
            Assert.AreEqual(t1_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad3Position, res[4].Position);
            Assert.AreEqual(t3_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad3Position, res[5].Position);
            Assert.AreEqual(t3_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestDefault2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad1Position, res[0].Position);
            Assert.AreEqual(t1_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad1Position, res[1].Position);
            Assert.AreEqual(t1_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad3Position, res[4].Position);
            Assert.AreEqual(t3_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad3Position, res[5].Position);
            Assert.AreEqual(t3_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestDefault3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayDefault, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad1Position, res[0].Position);
            Assert.AreEqual(t1_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad1Position, res[1].Position);
            Assert.AreEqual(t1_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad3Position, res[4].Position);
            Assert.AreEqual(t3_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad3Position, res[5].Position);
            Assert.AreEqual(t3_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestDefaultReverse1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayDefaultReverse, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestDefaultReverse2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayDefaultReverse, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestDefaultReverse3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayDefaultReverse, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestDefaultNoContact1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestDefaultNoContact2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestDefaultNoContact3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayDefaultNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }

        [TestMethod()]
        public void PickAllTestReverse1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad1Position, res[0].Position);
            Assert.AreEqual(t1_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad1Position, res[1].Position);
            Assert.AreEqual(t1_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad3Position, res[4].Position);
            Assert.AreEqual(t3_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad3Position, res[5].Position);
            Assert.AreEqual(t3_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestReverse2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad1Position, res[0].Position);
            Assert.AreEqual(t1_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad1Position, res[1].Position);
            Assert.AreEqual(t1_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad3Position, res[4].Position);
            Assert.AreEqual(t3_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad3Position, res[5].Position);
            Assert.AreEqual(t3_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestReverse3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayAllDirections, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad1Position, res[0].Position);
            Assert.AreEqual(t1_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad1Position, res[1].Position);
            Assert.AreEqual(t1_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad3Position, res[4].Position);
            Assert.AreEqual(t3_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad3Position, res[5].Position);
            Assert.AreEqual(t3_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestReverseReverse1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad3Position, res[0].Position);
            Assert.AreEqual(t3_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad3Position, res[1].Position);
            Assert.AreEqual(t3_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad1Position, res[4].Position);
            Assert.AreEqual(t1_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad1Position, res[5].Position);
            Assert.AreEqual(t1_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestReverseReverse2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad3Position, res[0].Position);
            Assert.AreEqual(t3_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad3Position, res[1].Position);
            Assert.AreEqual(t3_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad1Position, res[4].Position);
            Assert.AreEqual(t1_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad1Position, res[5].Position);
            Assert.AreEqual(t1_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestReverseReverse3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayAllDirectionsReverse, out var res);

            Assert.IsTrue(picked);

            Assert.HasCount(6, res);

            Assert.AreEqual(1, res[0].Distance);
            Assert.AreEqual(toQuad3Position, res[0].Position);
            Assert.AreEqual(t3_1, res[0].Primitive);
            Assert.AreEqual(1, res[1].Distance);
            Assert.AreEqual(toQuad3Position, res[1].Position);
            Assert.AreEqual(t3_2, res[1].Primitive);

            Assert.AreEqual(2, res[2].Distance);
            Assert.AreEqual(toQuad2Position, res[2].Position);
            Assert.AreEqual(t2_1, res[2].Primitive);
            Assert.AreEqual(2, res[3].Distance);
            Assert.AreEqual(toQuad2Position, res[3].Position);
            Assert.AreEqual(t2_2, res[3].Primitive);

            Assert.AreEqual(3, res[4].Distance);
            Assert.AreEqual(toQuad1Position, res[4].Position);
            Assert.AreEqual(t1_1, res[4].Primitive);
            Assert.AreEqual(3, res[5].Distance);
            Assert.AreEqual(toQuad1Position, res[5].Position);
            Assert.AreEqual(t1_2, res[5].Primitive);
        }
        [TestMethod()]
        public void PickAllTestReverseNoContact1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestReverseNoContact2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestReverseNoContact3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayAllDirectionsNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }

        [TestMethod()]
        public void PickAllTestCoarse1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarse2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarse3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayCoarse, out var res);

            Assert.IsTrue(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarseReverse1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarseReverse2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarseReverse3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayCoarseReverse, out var res);

            Assert.IsTrue(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarseNoContact1()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarseNoContact2()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
        [TestMethod()]
        public void PickAllTestCoarseNoContact3()
        {
            bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ref rayCoarseNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.IsEmpty(res);
        }
    }
}