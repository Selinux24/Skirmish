using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Engine.Common.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class RayPickingHelperTests
    {
        static TestContext _testContext;

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

        static Triangle bbox1t1;
        static Triangle bbox1t2;
        static Triangle bbox1t3;
        static Triangle bbox1t4;
        static Triangle bbox2t1;
        static Triangle bbox2t2;
        static Triangle bbox2t3;
        static Triangle bbox2t4;
        static Triangle bbox3t1;
        static Triangle bbox3t2;
        static Triangle bbox3t3;
        static Triangle bbox3t4;

        static Ray ray;
        static Ray rayReverse;
        static Ray rayNoContact;
        static RayPickingParams rayPickingParamsPerfect;
        static RayPickingParams rayPickingParamsCoarse;

        static Vector3 toQuad1Position;
        static Vector3 toQuad2Position;
        static Vector3 toQuad3Position;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            ray = new Ray(Vector3.Zero, Vector3.ForwardLH);
            rayReverse = new Ray(Vector3.ForwardLH * 4, Vector3.BackwardLH);
            rayNoContact = new Ray(Vector3.Zero, Vector3.BackwardLH);
            rayPickingParamsPerfect = RayPickingParams.Perfect;
            rayPickingParamsCoarse = RayPickingParams.Coarse;

            t1_1 = new Triangle(new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(-1, -1, 1));
            t1_2 = new Triangle(new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1));

            t2_1 = new Triangle(new Vector3(-1, 1, 2), new Vector3(1, 1, 2), new Vector3(-1, -1, 2));
            t2_2 = new Triangle(new Vector3(1, 1, 2), new Vector3(1, -1, 2), new Vector3(-1, -1, 2));

            t3_1 = new Triangle(new Vector3(-1, 1, 3), new Vector3(1, 1, 3), new Vector3(-1, -1, 3));
            t3_2 = new Triangle(new Vector3(1, 1, 3), new Vector3(1, -1, 3), new Vector3(-1, -1, 3));

            var t1 = new[] { t1_1, t1_2, t2_1, t2_2, t3_1, t3_2 };
            var t2 = new[] { t3_1, t3_2, t2_1, t2_2, t1_1, t1_2 };
            var t3 = new[] { t2_1, t2_2, t3_1, t3_2, t1_1, t1_2 };

            var p1 = t1.SelectMany(t => t.GetVertices()).ToArray();
            var p2 = t2.SelectMany(t => t.GetVertices()).ToArray();
            var p3 = t3.SelectMany(t => t.GetVertices()).ToArray();

            bsph1 = BoundingSphere.FromPoints(p1);
            bsph2 = BoundingSphere.FromPoints(p2);
            bsph3 = BoundingSphere.FromPoints(p3);

            bbox1 = BoundingBox.FromPoints(p1);
            bbox2 = BoundingBox.FromPoints(p2);
            bbox3 = BoundingBox.FromPoints(p3);

            var bbox1Tris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox1);
            var bbox2Tris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox2);
            var bbox3Tris = Triangle.ComputeTriangleList(Topology.TriangleList, bbox3);

            bbox1t1 = bbox1Tris.ElementAt(0);
            bbox1t2 = bbox1Tris.ElementAt(1);
            bbox1t3 = bbox1Tris.ElementAt(2);
            bbox1t4 = bbox1Tris.ElementAt(3);

            bbox2t1 = bbox2Tris.ElementAt(0);
            bbox2t2 = bbox2Tris.ElementAt(1);
            bbox2t3 = bbox2Tris.ElementAt(2);
            bbox2t4 = bbox2Tris.ElementAt(3);

            bbox3t1 = bbox3Tris.ElementAt(0);
            bbox3t2 = bbox3Tris.ElementAt(1);
            bbox3t3 = bbox3Tris.ElementAt(2);
            bbox3t4 = bbox3Tris.ElementAt(3);

            toQuad1Position = new Vector3(0, 0, 1);
            toQuad2Position = new Vector3(0, 0, 2);
            toQuad3Position = new Vector3(0, 0, 3);

            mockQuad1 = new Mock<IRayPickable<Triangle>>();
            mockQuad1.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(bsph1);
            mockQuad1.Setup(o => o.GetVolume(true)).Returns(t1);
            mockQuad1.Setup(o => o.GetVolume(false)).Returns(bbox1Tris);

            mockQuad2 = new Mock<IRayPickable<Triangle>>();
            mockQuad2.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(bsph2);
            mockQuad2.Setup(o => o.GetVolume(true)).Returns(t2);
            mockQuad2.Setup(o => o.GetVolume(false)).Returns(bbox2Tris);

            mockQuad3 = new Mock<IRayPickable<Triangle>>();
            mockQuad3.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(bsph3);
            mockQuad3.Setup(o => o.GetVolume(true)).Returns(t3);
            mockQuad3.Setup(o => o.GetVolume(false)).Returns(bbox3Tris);
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void PickNearestTestDefault1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ray, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(bbox1t1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefault2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ray, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(bbox2t1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefault3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ray, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad1Position, res.Position);
            Assert.AreEqual(bbox3t1, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultReverse1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(bbox1t3, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultReverse2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(bbox2t3, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultReverse3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayReverse, out var res);

            Assert.IsTrue(picked);

            Assert.AreEqual(1, res.Distance);
            Assert.AreEqual(toQuad3Position, res.Position);
            Assert.AreEqual(bbox3t3, res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultNoContact1()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultNoContact2()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }
        [TestMethod()]
        public void PickNearestTestDefaultNoContact3()
        {
            bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayNoContact, out var res);

            Assert.IsFalse(picked);

            Assert.AreEqual(float.MaxValue, res.Distance);
            Assert.AreEqual(Vector3.Zero, res.Position);
            Assert.AreEqual(new Triangle(), res.Primitive);
        }

        [TestMethod()]
        public void PickNearestTestPerfect()
        {
            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(t1_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(t1_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(t1_1, res.Primitive);
            }




            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(t3_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(t3_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(t3_1, res.Primitive);
            }




            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }
        }
        [TestMethod()]
        public void PickNearestTestCoarse()
        {
            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1), res.Primitive);
            }




            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3), res.Primitive);
            }





            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }
        }

        [TestMethod()]
        public void PickFirstTestDefault()
        {
            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox1t1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox2t1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox3t1, res.Primitive);
            }




            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(bbox1t3, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(bbox2t3, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(bbox3t3, res.Primitive);
            }




            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayNoContact, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayNoContact, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayNoContact, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }
        }
        [TestMethod()]
        public void PickFirstTestPerfect()
        {
            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(t1_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(3, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(t3_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(2, res.Distance);
                Assert.AreEqual(toQuad2Position, res.Position);
                Assert.AreEqual(t2_1, res.Primitive);
            }



            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(3, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(t1_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad3Position, res.Position);
                Assert.AreEqual(t3_1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(2, res.Distance);
                Assert.AreEqual(toQuad2Position, res.Position);
                Assert.AreEqual(t2_1, res.Primitive);
            }



            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }
        }
        [TestMethod()]
        public void PickFirstTestCoarse()
        {
            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox1t1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox2t1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(1, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox3t1, res.Primitive);
            }



            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(3, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox1t1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(3, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox2t1, res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(3, res.Distance);
                Assert.AreEqual(toQuad1Position, res.Position);
                Assert.AreEqual(bbox3t1, res.Primitive);
            }



            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(float.MaxValue, res.Distance);
                Assert.AreEqual(Vector3.Zero, res.Position);
                Assert.AreEqual(new Triangle(), res.Primitive);
            }
        }

        [TestMethod()]
        public void PickAllTestDefault()
        {
            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(2, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox1t1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox1t2, res.ElementAt(1).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(2, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox2t1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox2t2, res.ElementAt(1).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(2, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox3t1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox3t2, res.ElementAt(1).Primitive);
            }




            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(2, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox1t3, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox1t4, res.ElementAt(1).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(2, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox2t3, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox2t4, res.ElementAt(1).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(2, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox3t3, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox3t4, res.ElementAt(1).Primitive);
            }




            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayNoContact, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayNoContact, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayNoContact, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }
        }
        [TestMethod()]
        public void PickAllTestReverse()
        {
            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(6, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(t1_1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(t1_2, res.ElementAt(1).Primitive);

                Assert.AreEqual(2, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(2).Position);
                Assert.AreEqual(t2_1, res.ElementAt(2).Primitive);
                Assert.AreEqual(2, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(3).Position);
                Assert.AreEqual(t2_2, res.ElementAt(3).Primitive);

                Assert.AreEqual(3, res.ElementAt(4).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(4).Position);
                Assert.AreEqual(t3_1, res.ElementAt(4).Primitive);
                Assert.AreEqual(3, res.ElementAt(5).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(5).Position);
                Assert.AreEqual(t3_2, res.ElementAt(5).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(6, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(t1_1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(t1_2, res.ElementAt(1).Primitive);

                Assert.AreEqual(2, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(2).Position);
                Assert.AreEqual(t2_1, res.ElementAt(2).Primitive);
                Assert.AreEqual(2, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(3).Position);
                Assert.AreEqual(t2_2, res.ElementAt(3).Primitive);

                Assert.AreEqual(3, res.ElementAt(4).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(4).Position);
                Assert.AreEqual(t3_1, res.ElementAt(4).Primitive);
                Assert.AreEqual(3, res.ElementAt(5).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(5).Position);
                Assert.AreEqual(t3_2, res.ElementAt(5).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(6, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(t1_1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(t1_2, res.ElementAt(1).Primitive);

                Assert.AreEqual(2, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(2).Position);
                Assert.AreEqual(t2_1, res.ElementAt(2).Primitive);
                Assert.AreEqual(2, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(3).Position);
                Assert.AreEqual(t2_2, res.ElementAt(3).Primitive);

                Assert.AreEqual(3, res.ElementAt(4).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(4).Position);
                Assert.AreEqual(t3_1, res.ElementAt(4).Primitive);
                Assert.AreEqual(3, res.ElementAt(5).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(5).Position);
                Assert.AreEqual(t3_2, res.ElementAt(5).Primitive);
            }



            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(6, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(t3_1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(t3_2, res.ElementAt(1).Primitive);

                Assert.AreEqual(2, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(2).Position);
                Assert.AreEqual(t2_1, res.ElementAt(2).Primitive);
                Assert.AreEqual(2, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(3).Position);
                Assert.AreEqual(t2_2, res.ElementAt(3).Primitive);

                Assert.AreEqual(3, res.ElementAt(4).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(4).Position);
                Assert.AreEqual(t1_1, res.ElementAt(4).Primitive);
                Assert.AreEqual(3, res.ElementAt(5).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(5).Position);
                Assert.AreEqual(t1_2, res.ElementAt(5).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(6, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(t3_1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(t3_2, res.ElementAt(1).Primitive);

                Assert.AreEqual(2, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(2).Position);
                Assert.AreEqual(t2_1, res.ElementAt(2).Primitive);
                Assert.AreEqual(2, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(3).Position);
                Assert.AreEqual(t2_2, res.ElementAt(3).Primitive);

                Assert.AreEqual(3, res.ElementAt(4).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(4).Position);
                Assert.AreEqual(t1_1, res.ElementAt(4).Primitive);
                Assert.AreEqual(3, res.ElementAt(5).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(5).Position);
                Assert.AreEqual(t1_2, res.ElementAt(5).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(6, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(t3_1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(t3_2, res.ElementAt(1).Primitive);

                Assert.AreEqual(2, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(2).Position);
                Assert.AreEqual(t2_1, res.ElementAt(2).Primitive);
                Assert.AreEqual(2, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad2Position, res.ElementAt(3).Position);
                Assert.AreEqual(t2_2, res.ElementAt(3).Primitive);

                Assert.AreEqual(3, res.ElementAt(4).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(4).Position);
                Assert.AreEqual(t1_1, res.ElementAt(4).Primitive);
                Assert.AreEqual(3, res.ElementAt(5).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(5).Position);
                Assert.AreEqual(t1_2, res.ElementAt(5).Primitive);
            }



            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayNoContact, rayPickingParamsPerfect, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }
        }
        [TestMethod()]
        public void PickAllTestCoarse()
        {
            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(4, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox1t1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox1t2, res.ElementAt(1).Primitive);

                Assert.AreEqual(3, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(2).Position);
                Assert.AreEqual(bbox1t3, res.ElementAt(2).Primitive);
                Assert.AreEqual(3, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(3).Position);
                Assert.AreEqual(bbox1t4, res.ElementAt(3).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(4, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox2t1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox2t2, res.ElementAt(1).Primitive);

                Assert.AreEqual(3, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(2).Position);
                Assert.AreEqual(bbox2t3, res.ElementAt(2).Primitive);
                Assert.AreEqual(3, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(3).Position);
                Assert.AreEqual(bbox2t4, res.ElementAt(3).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(4, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox3t1, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox3t2, res.ElementAt(1).Primitive);

                Assert.AreEqual(3, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(2).Position);
                Assert.AreEqual(bbox3t3, res.ElementAt(2).Primitive);
                Assert.AreEqual(3, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(3).Position);
                Assert.AreEqual(bbox3t4, res.ElementAt(3).Primitive);
            }




            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(4, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox1t3, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox1t4, res.ElementAt(1).Primitive);

                Assert.AreEqual(3, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(2).Position);
                Assert.AreEqual(bbox1t1, res.ElementAt(2).Primitive);
                Assert.AreEqual(3, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(3).Position);
                Assert.AreEqual(bbox1t2, res.ElementAt(3).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(4, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox2t3, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox2t4, res.ElementAt(1).Primitive);

                Assert.AreEqual(3, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(2).Position);
                Assert.AreEqual(bbox2t1, res.ElementAt(2).Primitive);
                Assert.AreEqual(3, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(3).Position);
                Assert.AreEqual(bbox2t2, res.ElementAt(3).Primitive);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(4, res.Count());

                Assert.AreEqual(1, res.ElementAt(0).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(0).Position);
                Assert.AreEqual(bbox3t3, res.ElementAt(0).Primitive);
                Assert.AreEqual(1, res.ElementAt(1).Distance);
                Assert.AreEqual(toQuad3Position, res.ElementAt(1).Position);
                Assert.AreEqual(bbox3t4, res.ElementAt(1).Primitive);

                Assert.AreEqual(3, res.ElementAt(2).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(2).Position);
                Assert.AreEqual(bbox3t1, res.ElementAt(2).Primitive);
                Assert.AreEqual(3, res.ElementAt(3).Distance);
                Assert.AreEqual(toQuad1Position, res.ElementAt(3).Position);
                Assert.AreEqual(bbox3t2, res.ElementAt(3).Primitive);
            }




            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayNoContact, rayPickingParamsCoarse, out var res);

                Assert.IsFalse(picked);

                Assert.AreEqual(0, res.Count());
            }
        }
    }
}