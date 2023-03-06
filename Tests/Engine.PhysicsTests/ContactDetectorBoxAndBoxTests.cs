using Engine.Common;
using Engine.Physics;
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

        static CollisionBox FromAABB(Vector3 extents, Matrix transform)
        {
            CollisionBox box = new CollisionBox(extents);
            RigidBody boxBody = new RigidBody(1, transform);
            box.Attach(boxBody);

            return box;
        }
        static CollisionTriangleSoup FromBox(Vector3 extents, Matrix transform)
        {
            var tris = Triangle.ComputeTriangleList(Topology.TriangleList, new BoundingBox(-extents, extents));
            CollisionTriangleSoup box = new CollisionTriangleSoup(tris);
            RigidBody boxBody = new RigidBody(1, transform);
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
            ContactResolver data2 = new ContactResolver();
            ContactResolver data3 = new ContactResolver();

            Matrix r1 = Matrix.RotationAxis(Vector3.ForwardLH, MathUtil.PiOverFour);
            Matrix r2 = Matrix.RotationAxis(r1.Up, -MathUtil.PiOverFour);
            var boxTrn1 = r2 * r1 * Matrix.Translation(new Vector3(0, 2, 1.5f));
            var boxTrn2 = Matrix.Translation(Vector3.Zero);

            var box1 = FromAABB(Vector3.One, boxTrn1);
            var box2 = FromAABB(Vector3.One, boxTrn2);
            var box3 = FromBox(Vector3.One, boxTrn2);

            bool intersectionWith2 = ContactDetector.BoxAndBox(box1, box2, data2);
            Assert.AreEqual(true, intersectionWith2);
            bool intersectionWith3 = ContactDetector.BoxAndTriangleSoup(box1, box3, data3);
            Assert.AreEqual(true, intersectionWith3);

            var contactsWith2 = data2.GetContacts().Select(c => (c.Position, c.Normal, c.Penetration)).ToArray();
            Assert.IsTrue(contactsWith2.Any());

            var contactsWith3 = data3.GetContacts().Select(c => (c.Position, c.Normal, c.Penetration)).ToArray();
            Assert.IsTrue(contactsWith3.Any());

            var pointInBox2 = Intersection.ClosestPointInBox(contactsWith2[0].Position, box1.OrientedBoundingBox);
            Assert.AreEqual(contactsWith2[0].Position, pointInBox2);

            var pointInBox3 = Intersection.ClosestPointInBox(contactsWith3[0].Position, box1.OrientedBoundingBox);
            Assert.AreEqual(contactsWith3[0].Position, pointInBox3);
        }
    }
}
