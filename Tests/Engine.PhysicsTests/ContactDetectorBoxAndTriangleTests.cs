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
    public class ContactDetectorBoxAndTriangleTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new(MathUtil.ZeroTolerance);

        static BoxCollider FromAABB(Vector3 extents, Matrix transform)
        {
            var box = new BoxCollider(extents);
            var boxBody = new RigidBody(new() { Mass = 1f, InitialTransform = transform });
            box.Attach(boxBody);

            return box;
        }
        static HalfSpaceCollider FromPlane(Plane plane, Matrix transform)
        {
            var p = new HalfSpaceCollider(plane);
            var triBody = new RigidBody(new() { Mass = 2f, InitialTransform = transform });
            p.Attach(triBody);

            return p;
        }
        static ConvexMeshCollider FromTriangle(Triangle tri, Matrix transform)
        {
            var ctri = new ConvexMeshCollider(new[] { tri });
            var triBody = new RigidBody(new() { Mass = 2f, InitialTransform = transform });
            ctri.Attach(triBody);

            return ctri;
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
        public void ContactDetectorBoxAndTriangleTest()
        {
            var dataTri = new ContactResolver();

            var box = FromAABB(Vector3.One, Matrix.Translation(Vector3.Up * 0.99f));

            var p1 = new Vector3(0f, 0f, 100f);
            var p2 = new Vector3(100f, 0f, -100f);
            var p3 = new Vector3(-100f, 0f, -100f);
            var tri = new Triangle(p1, p2, p3);

            var triSoup = FromTriangle(tri, Matrix.Identity);

            bool intersectionTri = ContactDetector.BetweenObjects(box, triSoup, dataTri);
            Assert.AreEqual(true, intersectionTri);

            var contactsTri = dataTri.GetContacts().Select(c => (c.Position, c.Normal, c.Penetration)).ToArray();
            Assert.AreEqual(1, contactsTri.Length);
        }

        [TestMethod()]
        public void ContactDetectorBoxAndTriangleTest2()
        {
            var dataPln = new ContactResolver();
            var dataTri = new ContactResolver();

            var box = FromAABB(Vector3.One, Matrix.Translation(Vector3.Down));
            var plane = FromPlane(new Plane(Vector3.Up, 0), Matrix.Identity);

            var p1 = new Vector3(0f, -0.1f, 0f);
            var p2 = new Vector3(5f, 5f, 0f);
            var p3 = new Vector3(-5f, 5f, 0f);
            var tri = new Triangle(p1, p2, p3);
            var triSoup = FromTriangle(tri, Matrix.Identity);

            bool intersectionPln = ContactDetector.BetweenObjects(triSoup, plane, dataPln);
            Assert.AreEqual(true, intersectionPln);

            bool intersectionTri = ContactDetector.BetweenObjects(box, triSoup, dataTri);
            Assert.AreEqual(true, intersectionTri);

            var contactsPln = dataPln.GetContacts();
            var contactsTri = dataTri.GetContacts();

            for (int i = 0; i < contactsPln.Count(); i++)
            {
                var contact = contactsPln.ElementAt(i);

                var expectedContact = contactsTri.FirstOrDefault(c => c.Position == contact.Position);
                if (expectedContact == null)
                {
                    continue;
                }

                var expectedPenetration = expectedContact.Penetration;
                var expectedPosition = expectedContact.Position;
                var expectedNormal = Vector3.Up;

                Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Contact {i}. Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Contact {i}. Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Contact {i}. Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
