using Engine.Physics;
using Engine.Physics.Colliders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorSphereTests
    {
        static TestContext _testContext;

        static readonly float Epsilon = 0.0005f;

        static HalfSpaceCollider FromPlane(Vector3 point, Vector3 normal, Matrix transform)
        {
            var p = new Plane(point, normal);
            var c = new HalfSpaceCollider(p);
            var rb = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }
        static TriangleCollider FromTriangle(Triangle tri, Matrix transform)
        {
            var c = new TriangleCollider(tri);
            var rb = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }
        static ConvexMeshCollider FromTriangles(IEnumerable<Triangle> tris, Matrix transform)
        {
            var c = new ConvexMeshCollider(tris);
            var rb = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }

        static SphereCollider FromRadius(float radius, Matrix transform)
        {
            var c = new SphereCollider(radius);
            var rb = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }

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
        public void ContactDetectorSphereTest()
        {
            var data1 = new ContactResolver();
            var data2 = new ContactResolver();
            var data3 = new ContactResolver();

            float ah = 0;
            var trnA = Matrix.Translation(0, 0, 0);

            var t = new Triangle(new Vector3(-10, ah, -10), new Vector3(0, ah, 10), new Vector3(10, ah, -10));
            var t1 = new Triangle(new Vector3(-10, ah, -10), new Vector3(-10, ah, 10), new Vector3(10, ah, -10));
            var t2 = new Triangle(new Vector3(-10, ah, 10), new Vector3(10, ah, 10), new Vector3(10, ah, -10));

            var c1 = FromPlane(new Vector3(0, ah, 0), Vector3.Up, trnA);
            var c2 = FromTriangle(t, trnA);
            var c3 = FromTriangles([t1, t2], trnA);

            float bh = -0.5f;
            Matrix trnB = Matrix.Translation(0, bh, 0);
            float r = 1f;

            var b = FromRadius(r, trnB);

            float penetration = r - (ah - bh);
            float penetrationHf = penetration + r;

            bool intersection1 = ContactDetector.BetweenObjects(c1, b, data1);
            bool intersection2 = ContactDetector.BetweenObjects(c2, b, data2);
            bool intersection3 = ContactDetector.BetweenObjects(c3, b, data3);

            Assert.IsTrue(intersection1, "Intersection expected.");
            Assert.IsTrue(intersection2, "Intersection expected.");
            Assert.IsTrue(intersection3, "Intersection expected.");

            Assert.AreEqual(1, data1.ContactCount, "One contact expected");
            Assert.AreEqual(1, data2.ContactCount, "One contact expected");
            Assert.AreEqual(1, data3.ContactCount, "One contact expected");

            var contact1 = data1.GetContact(0);
            var contact2 = data2.GetContact(0);
            var contact3 = data3.GetContact(0);

            Assert.AreEqual(penetrationHf, contact1.Penetration, Epsilon); //It's a half space calculation
            Assert.AreEqual(penetrationHf, contact2.Penetration, Epsilon); //It's a half space calculation
            Assert.AreEqual(penetration, contact3.Penetration, Epsilon); //Not's a half space

            Assert.IsTrue(Vector3.NearEqual(Vector3.Up, contact1.Normal, new Vector3(Epsilon)), $"Expected normal {Vector3.Up} != {contact1.Normal}"); //Engine reverses the colliders order when half spaces colliders
            Assert.IsTrue(Vector3.NearEqual(Vector3.Down, contact2.Normal, new Vector3(Epsilon)), $"Expected normal {Vector3.Down} != {contact2.Normal}");
            Assert.IsTrue(Vector3.NearEqual(Vector3.Down, contact3.Normal, new Vector3(Epsilon)), $"Expected normal {Vector3.Down} != {contact3.Normal}");

            Assert.IsTrue(Vector3.NearEqual(Vector3.Zero, contact1.Position, new Vector3(Epsilon)), $"Expected position {Vector3.Zero} != {contact1.Position}");
            Assert.IsTrue(Vector3.NearEqual(new Vector3(0, -r, 0), contact2.Position, new Vector3(Epsilon)), $"Expected position {new Vector3(0, -r, 0)} != {contact2.Position}");
            Assert.IsTrue(Vector3.NearEqual(Vector3.Zero, contact3.Position, new Vector3(Epsilon)), $"Expected position {Vector3.Zero} != {contact3.Position}");
        }
    }
}
