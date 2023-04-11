﻿using Engine.Physics;
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

        static readonly float Epsilon = 0.0001f;

        static HalfSpaceCollider FromPlane(Vector3 point, Vector3 normal, Matrix transform)
        {
            Plane p = new Plane(point, normal);
            HalfSpaceCollider c = new HalfSpaceCollider(p);
            RigidBody rb = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }
        static TriangleCollider FromTriangle(Triangle tri, Matrix transform)
        {
            TriangleCollider c = new TriangleCollider(tri);
            RigidBody rb = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }
        static MeshCollider FromTriangles(IEnumerable<Triangle> tris, Matrix transform)
        {
            MeshCollider c = new MeshCollider(tris);
            RigidBody rb = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }

        static SphereCollider FromRadius(float radius, Matrix transform)
        {
            SphereCollider c = new SphereCollider(radius);
            RigidBody rb = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            c.Attach(rb);

            return c;
        }
        static BoxCollider FromSize(Vector3 extents, Matrix transform)
        {
            BoxCollider c = new BoxCollider(extents);
            RigidBody rb = new RigidBody(new() { Mass = 1, InitialTransform = transform });
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
            ContactResolver data1 = new ContactResolver();
            ContactResolver data2 = new ContactResolver();
            ContactResolver data3 = new ContactResolver();

            float ah = 0;
            Matrix trnA = Matrix.Translation(0, 0, 0);

            Triangle t = new(new Vector3(-10, ah, -10), new Vector3(0, ah, 10), new Vector3(10, ah, -10));
            Triangle t1 = new(new Vector3(-10, ah, -10), new Vector3(-10, ah, 10), new Vector3(10, ah, -10));
            Triangle t2 = new(new Vector3(-10, ah, 10), new Vector3(10, ah, 10), new Vector3(10, ah, -10));

            var c1 = FromPlane(new Vector3(0, ah, 0), Vector3.Up, trnA);
            var c2 = FromTriangle(t, trnA);
            var c3 = FromTriangles(new[] { t1, t2 }, trnA);

            float bh = -0.5f;
            Matrix trnB = Matrix.Translation(0, bh, 0);
            float r = 1f;

            var b = FromRadius(r, trnB);

            float penetration = r - (ah - bh) + r;

            bool intersection1 = ContactDetector.BetweenObjects(b, c1, data1);
            bool intersection2 = ContactDetector.BetweenObjects(b, c2, data2);
            bool intersection3 = ContactDetector.BetweenObjects(b, c3, data3);

            Assert.AreEqual(true, intersection1, "Intersection expected.");
            Assert.AreEqual(true, intersection2, "Intersection expected.");
            Assert.AreEqual(true, intersection3, "Intersection expected.");

            Assert.AreEqual(1, data1.ContactCount, "One contact expected");
            Assert.AreEqual(1, data2.ContactCount, "One contact expected");
            Assert.AreEqual(1, data3.ContactCount, "One contact expected");

            var contact1 = data1.GetContact(0);
            var contact2 = data2.GetContact(0);
            var contact3 = data3.GetContact(0);

            Assert.AreEqual(penetration, contact1.Penetration, Epsilon);
            Assert.AreEqual(penetration, contact2.Penetration, Epsilon);
            Assert.AreEqual(penetration, contact3.Penetration, Epsilon);

            Assert.AreEqual(contact1.Normal, contact2.Normal);
            Assert.AreEqual(contact1.Normal, contact3.Normal);

            Assert.AreEqual(contact1.Position, contact2.Position);
            Assert.AreEqual(contact1.Position, contact3.Position);
        }
    }
}
