using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Linq;

namespace Engine.Common.Tests
{
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

        static Ray ray;
        static Ray rayReverse;
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

            toQuad1Position = new Vector3(0, 0, 1);
            toQuad2Position = new Vector3(0, 0, 2);
            toQuad3Position = new Vector3(0, 0, 3);

            mockQuad1 = new Mock<IRayPickable<Triangle>>();
            mockQuad1.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(bsph1);
            mockQuad1.Setup(o => o.GetVolume(true)).Returns(t1);
            mockQuad1.Setup(o => o.GetVolume(false)).Returns(Triangle.ComputeTriangleList(Topology.TriangleList, bbox1));

            mockQuad2 = new Mock<IRayPickable<Triangle>>();
            mockQuad2.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(bsph2);
            mockQuad2.Setup(o => o.GetVolume(true)).Returns(t2);
            mockQuad2.Setup(o => o.GetVolume(false)).Returns(Triangle.ComputeTriangleList(Topology.TriangleList, bbox2));

            mockQuad3 = new Mock<IRayPickable<Triangle>>();
            mockQuad3.Setup(o => o.GetBoundingSphere(It.IsAny<bool>())).Returns(bsph3);
            mockQuad3.Setup(o => o.GetVolume(true)).Returns(t3);
            mockQuad3.Setup(o => o.GetVolume(false)).Returns(Triangle.ComputeTriangleList(Topology.TriangleList, bbox3));
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void PickNearestTestDefault()
        {
            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ray, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ray, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ray, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }




            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayReverse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayReverse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayReverse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }
        }
        [TestMethod()]
        public void PickNearestTestPerfect()
        {
            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, t1_1);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, t1_1);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, t1_1);
            }




            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, t3_1);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, t3_1);
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, t3_1);
            }
        }
        [TestMethod()]
        public void PickNearestTestCoarse()
        {
            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }




            {
                bool picked = RayPickingHelper.PickNearest(mockQuad1.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad2.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickNearest(mockQuad3.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }
        }

        [TestMethod()]
        public void PickFirstTestDefault()
        {
            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ray, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ray, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ray, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }




            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayReverse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayReverse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayReverse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
            }
        }
        [TestMethod()]
        public void PickFirstTestPerfect()
        {
            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, t1_1);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 3);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, t3_1);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 2);
                Assert.AreEqual(res.Position, toQuad2Position);
                Assert.AreEqual(res.Primitive, t2_1);
            }



            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 3);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, t1_1);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad3Position);
                Assert.AreEqual(res.Primitive, t3_1);
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 2);
                Assert.AreEqual(res.Position, toQuad2Position);
                Assert.AreEqual(res.Primitive, t2_1);
            }
        }
        [TestMethod()]
        public void PickFirstTestCoarse()
        {
            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 1);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }



            {
                bool picked = RayPickingHelper.PickFirst(mockQuad1.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 3);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad2.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 3);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }

            {
                bool picked = RayPickingHelper.PickFirst(mockQuad3.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);
                Assert.AreEqual(res.Distance, 3);
                Assert.AreEqual(res.Position, toQuad1Position);
                Assert.AreEqual(res.Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
            }
        }

        [TestMethod()]
        public void PickAllTestDefault()
        {
            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 2);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 2);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ray, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 2);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));
            }




            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 2);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 2);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayReverse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 2);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));
            }
        }
        [TestMethod()]
        public void PickAllTestReverse()
        {
            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 6);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, t1_1);
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, t1_2);

                Assert.AreEqual(res.ElementAt(2).Distance, 2);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, t2_1);
                Assert.AreEqual(res.ElementAt(3).Distance, 2);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, t2_2);

                Assert.AreEqual(res.ElementAt(4).Distance, 3);
                Assert.AreEqual(res.ElementAt(4).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(4).Primitive, t3_1);
                Assert.AreEqual(res.ElementAt(5).Distance, 3);
                Assert.AreEqual(res.ElementAt(5).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(5).Primitive, t3_2);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 6);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, t1_1);
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, t1_2);

                Assert.AreEqual(res.ElementAt(2).Distance, 2);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, t2_1);
                Assert.AreEqual(res.ElementAt(3).Distance, 2);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, t2_2);

                Assert.AreEqual(res.ElementAt(4).Distance, 3);
                Assert.AreEqual(res.ElementAt(4).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(4).Primitive, t3_1);
                Assert.AreEqual(res.ElementAt(5).Distance, 3);
                Assert.AreEqual(res.ElementAt(5).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(5).Primitive, t3_2);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ray, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 6);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, t1_1);
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, t1_2);

                Assert.AreEqual(res.ElementAt(2).Distance, 2);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, t2_1);
                Assert.AreEqual(res.ElementAt(3).Distance, 2);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, t2_2);

                Assert.AreEqual(res.ElementAt(4).Distance, 3);
                Assert.AreEqual(res.ElementAt(4).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(4).Primitive, t3_1);
                Assert.AreEqual(res.ElementAt(5).Distance, 3);
                Assert.AreEqual(res.ElementAt(5).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(5).Primitive, t3_2);
            }



            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 6);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, t3_1);
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, t3_2);

                Assert.AreEqual(res.ElementAt(2).Distance, 2);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, t2_1);
                Assert.AreEqual(res.ElementAt(3).Distance, 2);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, t2_2);

                Assert.AreEqual(res.ElementAt(4).Distance, 3);
                Assert.AreEqual(res.ElementAt(4).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(4).Primitive, t1_1);
                Assert.AreEqual(res.ElementAt(5).Distance, 3);
                Assert.AreEqual(res.ElementAt(5).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(5).Primitive, t1_2);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 6);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, t3_1);
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, t3_2);

                Assert.AreEqual(res.ElementAt(2).Distance, 2);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, t2_1);
                Assert.AreEqual(res.ElementAt(3).Distance, 2);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, t2_2);

                Assert.AreEqual(res.ElementAt(4).Distance, 3);
                Assert.AreEqual(res.ElementAt(4).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(4).Primitive, t1_1);
                Assert.AreEqual(res.ElementAt(5).Distance, 3);
                Assert.AreEqual(res.ElementAt(5).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(5).Primitive, t1_2);
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayReverse, rayPickingParamsPerfect, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 6);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, t3_1);
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, t3_2);

                Assert.AreEqual(res.ElementAt(2).Distance, 2);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, t2_1);
                Assert.AreEqual(res.ElementAt(3).Distance, 2);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad2Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, t2_2);

                Assert.AreEqual(res.ElementAt(4).Distance, 3);
                Assert.AreEqual(res.ElementAt(4).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(4).Primitive, t1_1);
                Assert.AreEqual(res.ElementAt(5).Distance, 3);
                Assert.AreEqual(res.ElementAt(5).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(5).Primitive, t1_2);
            }
        }
        [TestMethod()]
        public void PickAllTestCoarse()
        {
            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 4);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));

                Assert.AreEqual(res.ElementAt(2).Distance, 3);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(3).Distance, 3);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 4);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));

                Assert.AreEqual(res.ElementAt(2).Distance, 3);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(3).Distance, 3);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, ray, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 4);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));

                Assert.AreEqual(res.ElementAt(2).Distance, 3);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(3).Distance, 3);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));
            }



            {
                bool picked = RayPickingHelper.PickAll(mockQuad1.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 4);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));

                Assert.AreEqual(res.ElementAt(2).Distance, 3);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(3).Distance, 3);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad2.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 4);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));

                Assert.AreEqual(res.ElementAt(2).Distance, 3);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(3).Distance, 3);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));
            }

            {
                bool picked = RayPickingHelper.PickAll(mockQuad3.Object, rayReverse, rayPickingParamsCoarse, out var res);

                Assert.IsTrue(picked);

                Assert.AreEqual(res.Count(), 4);

                Assert.AreEqual(res.ElementAt(0).Distance, 1);
                Assert.AreEqual(res.ElementAt(0).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(0).Primitive, new Triangle(-1, -1, 3, 1, -1, 3, 1, 1, 3));
                Assert.AreEqual(res.ElementAt(1).Distance, 1);
                Assert.AreEqual(res.ElementAt(1).Position, toQuad3Position);
                Assert.AreEqual(res.ElementAt(1).Primitive, new Triangle(-1, -1, 3, 1, 1, 3, -1, 1, 3));

                Assert.AreEqual(res.ElementAt(2).Distance, 3);
                Assert.AreEqual(res.ElementAt(2).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(2).Primitive, new Triangle(-1, -1, 1, -1, 1, 1, 1, 1, 1));
                Assert.AreEqual(res.ElementAt(3).Distance, 3);
                Assert.AreEqual(res.ElementAt(3).Position, toQuad1Position);
                Assert.AreEqual(res.ElementAt(3).Primitive, new Triangle(-1, -1, 1, 1, 1, 1, 1, -1, 1));
            }
        }
    }
}