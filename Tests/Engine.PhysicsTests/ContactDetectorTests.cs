using Engine.Physics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorTests
    {
        static TestContext _testContext;

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

        private static CollisionPlane FromPlane(Vector3 normal, float d, Matrix transform)
        {
            Plane p = new Plane(normal, d);
            CollisionPlane plane = new CollisionPlane(p);
            RigidBody planeBody = new RigidBody(float.PositiveInfinity, transform);
            plane.Attach(planeBody);

            return plane;
        }

        private static CollisionBox FromAABB(Vector3 min, Vector3 max, Matrix transform)
        {
            BoundingBox aabb = new BoundingBox(min, max);
            CollisionBox box = new CollisionBox(aabb);
            RigidBody boxBody = new RigidBody(1, transform);
            box.Attach(boxBody);

            return box;
        }

        [TestMethod()]
        public void ContactDetectorBoxAndHalfSpacePlaneTest()
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(-Vector3.One, Vector3.One, Matrix.Identity);
            Assert.IsTrue(ContactDetector.BetweenObjects(box, plane, data), "Plane intersects in the middle of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4, "Plane intersects in the middle of the box. Four contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 1, "Plane intersects in the middle of the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 1, "Plane intersects in the middle of the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 1, "Plane intersects in the middle of the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 1, "Plane intersects in the middle of the box. Penetration 1 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[4], "Plane intersects in the middle of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[5], "Plane intersects in the middle of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[6], "Plane intersects in the middle of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[7], "Plane intersects in the middle of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Plane intersects in the middle of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Plane intersects in the middle of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Plane intersects in the middle of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Plane intersects in the middle of the box. Contact normal equals to plane normal vertex expected");
            data.Reset();

            box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Up));
            Assert.IsTrue(ContactDetector.BetweenObjects(box, plane, data), "Plane intersects in the bottom face of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4, "Plane intersects in the bottom face of the box. Four contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 0, "Plane intersects in the bottom face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 0, "Plane intersects in the bottom face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 0, "Plane intersects in the bottom face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 0, "Plane intersects in the bottom face of the box. Penetration 0 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[4], "Plane intersects in the bottom face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[5], "Plane intersects in the bottom face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[6], "Plane intersects in the bottom face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[7], "Plane intersects in the bottom face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Plane intersects in the bottom face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Plane intersects in the bottom face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Plane intersects in the bottom face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Plane intersects in the bottom face of the box. Contact normal equals to plane normal vertex expected");
            data.Reset();

            box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Down));
            Assert.IsTrue(ContactDetector.BetweenObjects(box, plane, data), "Plane intersects in the upper face of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 8, "Plane intersects in the upper face of the box. Four contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 0, "Plane intersects in the upper face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 0, "Plane intersects in the upper face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 0, "Plane intersects in the upper face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 0, "Plane intersects in the upper face of the box. Penetration 0 expected");
            Assert.IsTrue(data.GetContact(4).Penetration == 2, "Plane intersects in the upper face of the box. Penetration 2 expected");
            Assert.IsTrue(data.GetContact(5).Penetration == 2, "Plane intersects in the upper face of the box. Penetration 2 expected");
            Assert.IsTrue(data.GetContact(6).Penetration == 2, "Plane intersects in the upper face of the box. Penetration 2 expected");
            Assert.IsTrue(data.GetContact(7).Penetration == 2, "Plane intersects in the upper face of the box. Penetration 2 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[0], "Plane intersects in the upper face of the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[1], "Plane intersects in the upper face of the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[2], "Plane intersects in the upper face of the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[3], "Plane intersects in the upper face of the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(4).Position, box.OrientedBoundingBox.GetCorners()[4], "Plane intersects in the upper face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(5).Position, box.OrientedBoundingBox.GetCorners()[5], "Plane intersects in the upper face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(6).Position, box.OrientedBoundingBox.GetCorners()[6], "Plane intersects in the upper face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(7).Position, box.OrientedBoundingBox.GetCorners()[7], "Plane intersects in the upper face of the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(4).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(5).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(6).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(7).Normal, plane.Normal, "Plane intersects in the upper face of the box. Contact normal equals to plane normal vertex expected");
            data.Reset();

            box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Down * 2f));
            Assert.IsTrue(ContactDetector.BetweenObjects(box, plane, data), "Plane over the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 8, "Plane over the box. Four contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 1, "Plane over the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 1, "Plane over the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 1, "Plane over the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 1, "Plane over the box. Penetration 1 expected");
            Assert.IsTrue(data.GetContact(4).Penetration == 3, "Plane over the box. Penetration 3 expected");
            Assert.IsTrue(data.GetContact(5).Penetration == 3, "Plane over the box. Penetration 3 expected");
            Assert.IsTrue(data.GetContact(6).Penetration == 3, "Plane over the box. Penetration 3 expected");
            Assert.IsTrue(data.GetContact(7).Penetration == 3, "Plane over the box. Penetration 3 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[0], "Plane over the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[1], "Plane over the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[2], "Plane over the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[3], "Plane over the box. Upper vertex expected");
            Assert.AreEqual(data.GetContact(4).Position, box.OrientedBoundingBox.GetCorners()[4], "Plane over the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(5).Position, box.OrientedBoundingBox.GetCorners()[5], "Plane over the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(6).Position, box.OrientedBoundingBox.GetCorners()[6], "Plane over the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(7).Position, box.OrientedBoundingBox.GetCorners()[7], "Plane over the box. Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(4).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(5).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(6).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(7).Normal, plane.Normal, "Plane over the box. Contact normal equals to plane normal vertex expected");
            data.Reset();
        }
    }
}
