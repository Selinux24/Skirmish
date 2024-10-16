﻿using Engine.Common;
using Engine.Physics;
using Engine.Physics.Colliders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorBoxAndBoxTests
    {
        static TestContext _testContext;

        static BoxCollider FromAABB(Vector3 extents, Matrix transform)
        {
            var box = new BoxCollider(extents);
            var boxBody = new RigidBody(new() { Mass = 1f, InitialTransform = transform });
            box.Attach(boxBody);

            return box;
        }
        static ConvexMeshCollider FromBox(Vector3 extents, Matrix transform)
        {
            var tris = Triangle.ComputeTriangleList(new BoundingBox(-extents, extents));
            var box = new ConvexMeshCollider(tris);
            var boxBody = new RigidBody(new() { Mass = 1f, InitialTransform = transform });
            box.Attach(boxBody);

            return box;
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
        public void ContactDetectorBoxAndBoxTest()
        {
            var data2 = new ContactResolver();
            var data3 = new ContactResolver();

            //Matrix r1 = Matrix.RotationAxis(Vector3.ForwardLH, MathUtil.PiOverFour);
            //Matrix r2 = Matrix.RotationAxis(r1.Up, -MathUtil.PiOverFour);
            //var boxTrn1 = r2 * r1 * Matrix.Translation(new Vector3(0, 2, 1.5f));

            var boxTrn1 = Matrix.Translation(new Vector3(0, 0, 0));
            var boxTrn2 = Matrix.Translation(new Vector3(2, 0, 0));

            var box1 = FromAABB(Vector3.One, boxTrn1);
            var box2 = FromAABB(Vector3.One, boxTrn2);
            var box3 = FromBox(Vector3.One, boxTrn2);

            bool intersectionWith2 = ContactDetector.BetweenObjects(box1, box2, data2);
            Assert.IsTrue(intersectionWith2);
            bool intersectionWith3 = ContactDetector.BetweenObjects(box1, box3, data3);
            Assert.IsTrue(intersectionWith3);

            var contactsWith2 = data2.GetContacts().Select(c => (c.Position, c.Normal, c.Penetration)).ToArray();
            Assert.IsTrue(contactsWith2.Length != 0);

            var contactsWith3 = data3.GetContacts().Select(c => (c.Position, c.Normal, c.Penetration)).ToArray();
            Assert.IsTrue(contactsWith3.Length != 0);

            var pointInBox2 = Intersection.ClosestPointInBox(contactsWith2[0].Position, box1.OrientedBoundingBox);
            Assert.AreEqual(contactsWith2[0].Position, pointInBox2);

            var pointInBox3 = Intersection.ClosestPointInBox(contactsWith3[0].Position, box1.OrientedBoundingBox);
            Assert.AreEqual(contactsWith3[0].Position, pointInBox3);
        }
    }
}
