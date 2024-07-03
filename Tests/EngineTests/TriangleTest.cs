using Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class TriangleTest
    {
        static TestContext _testContext;

        static readonly float third = 1f / 3f;
        static readonly float delta = 0.0001f;

        static Vector3 p1 = new(1, 0, 1);
        static Vector3 p2 = new(-1, 0, -1);
        static Vector3 p3 = new(-1, 0, 1);
        static Vector3 c = Vector3.Multiply(p1 + p2 + p3, third);
        static Plane p = new(0, 1, 0, 0);
        static Vector3 n = new(0, 1, 0);
        static readonly float a = 2;
        static readonly float r = Vector3.Distance(c, p1);
        static readonly float inc0 = 0;

        static Vector3 p1i45 = new(1, 1, 1);
        static Vector3 p2i45 = new(-1, -1, -1);
        static Vector3 p3i45 = new(-1, -1, 1);
        static readonly float inc45 = MathUtil.PiOverFour;

        static Vector3 p1i90 = new(0, 1, 1);
        static Vector3 p2i90 = new(0, -1, -1);
        static Vector3 p3i90 = new(0, -1, 1);
        static readonly float inc90 = MathUtil.PiOverTwo;

        static Vector3 p1r2 = new(2, 0, 1);
        static Vector3 p2r2 = new(-1, 0, -1);
        static Vector3 p3r2 = new(-1, 0, 1);
        static Vector3 c2 = Vector3.Multiply(p1r2 + p2r2 + p3r2, third);
        static readonly float r2 = Vector3.Distance(c2, p1r2);

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void ConstructorTest()
        {
            Triangle t1 = new();
            Triangle t2 = new(p1, p2, p3);
            Triangle t3 = new(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, p3.X, p3.Y, p3.Z);
            Triangle t4 = new([p1, p2, p3]);

            Assert.AreEqual(Vector3.Zero, t1.Point1);
            Assert.AreEqual(Vector3.Zero, t1.Point2);
            Assert.AreEqual(Vector3.Zero, t1.Point3);
            Assert.AreEqual(3, t1.GetStride());
            Assert.AreEqual(Topology.TriangleList, t1.GetTopology());

            Assert.AreEqual(p1, t2.Point1);
            Assert.AreEqual(p2, t2.Point2);
            Assert.AreEqual(p3, t2.Point3);
            Assert.AreEqual(n, t2.Normal);

            Assert.AreEqual(p1, t3.Point1);
            Assert.AreEqual(p2, t3.Point2);
            Assert.AreEqual(p3, t3.Point3);
            Assert.AreEqual(n, t2.Normal);

            Assert.AreEqual(p1, t4.Point1);
            Assert.AreEqual(p2, t4.Point2);
            Assert.AreEqual(p3, t4.Point3);
            Assert.AreEqual(n, t2.Normal);
        }
        [TestMethod()]
        public void ComparerTest()
        {
            Triangle t1 = new();
            Triangle t2 = new(p1, p2, p3);
            Triangle t3 = new(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, p3.X, p3.Y, p3.Z);

            Assert.IsTrue(t1 != t2);
            Assert.IsFalse(t1 == t2);
            Assert.IsFalse(t1.Equals(t2));
            Assert.IsFalse(t1.Equals((object)t2));
            Assert.IsFalse(t1.Equals(ref t2));

            Assert.IsTrue(t2 == t3);
            Assert.IsFalse(t2 != t3);
            Assert.IsTrue(t2.Equals(t3));
            Assert.IsTrue(t2.Equals((object)t3));
            Assert.IsTrue(t2.Equals(ref t3));
        }
        [TestMethod()]
        public void ReverseNormalTest()
        {
            Triangle t = new(p1, p2, p3);
            Vector3 n = t.Normal;
            Triangle rt = t.ReverseNormal();
            Vector3 rn = rt.Normal;

            Assert.AreEqual(-n, rn);
        }
        [TestMethod()]
        public void ReverseTest()
        {
            Triangle t = new(p1, p2, p3);
            Vector3 n = t.Normal;
            t.Reverse();
            Vector3 rn = t.Normal;

            Assert.AreEqual(-n, rn);
            Assert.AreEqual(p1, t.Point1);
            Assert.AreEqual(p2, t.Point3);
            Assert.AreEqual(p3, t.Point2);
        }
        [TestMethod()]
        public void PlaneTest()
        {
            Triangle t2 = new(p1, p2, p3);

            Assert.AreEqual(p, t2.GetPlane());
        }
        [TestMethod()]
        public void MinMaxTest()
        {
            Triangle t2 = new(p1, p2, p3);

            Assert.AreEqual(p2, t2.GetMinPoint());
            Assert.AreEqual(p1, t2.GetMaxPoint());
        }
        [TestMethod()]
        public void AreaTest()
        {
            Triangle t2 = new(p1, p2, p3);

            Assert.AreEqual(a, t2.GetArea(), delta);
        }
        [TestMethod()]
        public void CenterTest()
        {
            Triangle t2 = new(p1, p2, p3);

            Assert.AreEqual(c, t2.GetCenter());
        }
        [TestMethod()]
        public void BarycenterTest()
        {
            Triangle t2 = new(p1, p2, p3);

            Assert.AreEqual(new Vector3(1, 0, 0), t2.GetBarycenter(p1));
            Assert.AreEqual(new Vector3(0, 1, 0), t2.GetBarycenter(p2));
            Assert.AreEqual(new Vector3(0, 0, 1), t2.GetBarycenter(p3));
            Assert.AreEqual(new Vector3(0.5f, 0.5f, 0), t2.GetBarycenter(Vector3.Zero));
            Assert.AreEqual(new Vector3(5.5f, -4.5f, 0), t2.GetBarycenter(Vector3.One * 10f));
        }
        [TestMethod()]
        public void RadiusTest()
        {
            Triangle tr1 = new(p1, p2, p3);
            Triangle tr2 = new(p1r2, p2r2, p3r2);

            Assert.AreEqual(r, tr1.GetRadius());
            Assert.AreEqual(r2, tr2.GetRadius());
        }
        [TestMethod()]
        public void InclinationTest()
        {
            Triangle t1 = new(p1, p2, p3);
            Triangle t45 = new(p1i45, p2i45, p3i45);
            Triangle t90 = new(p1i90, p2i90, p3i90);

            Assert.AreEqual(inc0, t1.GetInclination());
            Assert.AreEqual(inc45, t45.GetInclination());
            Assert.AreEqual(inc90, t90.GetInclination());
        }

        [TestMethod()]
        public void PointsTest()
        {
            Triangle t1 = new(p1, p2, p3);

            Assert.AreEqual(p1, t1[0]);
            Assert.AreEqual(p2, t1[1]);
            Assert.AreEqual(p3, t1[2]);

            var points = t1.GetVertices();

            Assert.AreEqual(p1, points.ElementAt(0));
            Assert.AreEqual(p2, points.ElementAt(1));
            Assert.AreEqual(p3, points.ElementAt(2));
        }
        [TestMethod()]
        public void EdgesTest()
        {
            Triangle t1 = new(p1, p2, p3);

            Assert.AreEqual(p2 - p1, t1.GetEdge1());
            Assert.AreEqual(p3 - p2, t1.GetEdge2());
            Assert.AreEqual(p1 - p3, t1.GetEdge3());
        }

        [TestMethod()]
        public void ProjectToVectorTest()
        {
            Triangle t1 = new(p1, p2, p3);
            Vector3 vx = new(1, 0, 0);
            Vector3 vy = new(0, 1, 0);
            Vector3 vz = new(0, 0, 1);
            Vector3 vOne = Vector3.One;
            float prOne = 2.3094f;

            var dx = t1.ProjectToVector(vx);
            var dy = t1.ProjectToVector(vy);
            var dz = t1.ProjectToVector(vz);
            var dOne = t1.ProjectToVector(vOne);

            Assert.AreEqual(new Vector3(2, 0, 0), dx);
            Assert.AreEqual(2, dx.Length());
          
            Assert.AreEqual(new Vector3(0, 0, 0), dy);
            Assert.AreEqual(0, dy.Length());
         
            Assert.AreEqual(new Vector3(0, 0, 2), dz);
            Assert.AreEqual(2, dz.Length());
         
            Assert.AreEqual(1.3333f, dOne.X, delta);
            Assert.AreEqual(1.3333f, dOne.Y, delta);
            Assert.AreEqual(1.3333f, dOne.Z, delta);
            Assert.AreEqual(prOne, dOne.Length(), delta);


            dx = t1.ProjectToVector(-vx);
            dy = t1.ProjectToVector(-vy);
            dz = t1.ProjectToVector(-vz);
            dOne = t1.ProjectToVector(-vOne);

            Assert.AreEqual(new Vector3(2, 0, 0), dx);
            Assert.AreEqual(2, dx.Length());

            Assert.AreEqual(new Vector3(0, 0, 0), dy);
            Assert.AreEqual(0, dy.Length());

            Assert.AreEqual(new Vector3(0, 0, 2), dz);
            Assert.AreEqual(2, dz.Length());

            Assert.AreEqual(1.3333f, dOne.X, delta);
            Assert.AreEqual(1.3333f, dOne.Y, delta);
            Assert.AreEqual(1.3333f, dOne.Z, delta);
            Assert.AreEqual(prOne, dOne.Length(), delta);
        }
    }
}
