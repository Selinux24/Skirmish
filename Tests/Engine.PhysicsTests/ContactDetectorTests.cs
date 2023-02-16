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
    public class ContactDetectorTests
    {
        static TestContext _testContext;

        static CollisionPlane FromPlane(Vector3 normal, float d, Matrix transform)
        {
            Plane p = new Plane(normal, d);
            CollisionPlane plane = new CollisionPlane(p);
            RigidBody planeBody = new RigidBody(float.PositiveInfinity, transform);
            plane.Attach(planeBody);

            return plane;
        }

        static CollisionBox FromAABB(Vector3 min, Vector3 max, Matrix transform)
        {
            BoundingBox aabb = new BoundingBox(min, max);
            CollisionBox box = new CollisionBox(aabb);
            RigidBody boxBody = new RigidBody(1, transform);
            box.Attach(boxBody);

            return box;
        }

        static CollisionTriangleSoup FromTriangle(Triangle tri, Matrix transform)
        {
            CollisionTriangleSoup ctri = new CollisionTriangleSoup(new[] { tri });
            RigidBody triBody = new RigidBody(1, transform);
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
        public void ContactDetectorBoxAndHalfSpaceOverNoIntersectionTest()
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Up * 5f));
            Assert.IsFalse(ContactDetector.BoxAndHalfSpace(box, plane, data), "No intersection expected");
            Assert.IsTrue(data.ContactCount == 0, "Zero contacts expected");
        }
        [TestMethod()]
        public void ContactDetectorBoxAndHalfSpaceBottomFaceIntersectionTest()
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Up));
            Assert.IsTrue(ContactDetector.BoxAndHalfSpace(box, plane, data), "Intersection expected");
            Assert.IsTrue(data.ContactCount == 4, "Four contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 0, "Penetration 0 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[4], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[5], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[6], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[7], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            data.Reset();
        }
        [TestMethod()]
        public void ContactDetectorBoxAndHalfSpaceMiddleIntersectionTest()
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(-Vector3.One, Vector3.One, Matrix.Identity);
            Assert.IsTrue(ContactDetector.BoxAndHalfSpace(box, plane, data), "Intersection expected");
            Assert.IsTrue(data.ContactCount == 4, "Four contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 1, "Penetration 1 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[4], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[5], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[6], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[7], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            data.Reset();
        }
        [TestMethod()]
        public void ContactDetectorBoxAndHalfSpaceUpperFaceIntersectionTest()
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Down));
            Assert.IsTrue(ContactDetector.BoxAndHalfSpace(box, plane, data), "Intersection expected");
            Assert.IsTrue(data.ContactCount == 8, "Eight contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 0, "Penetration 0 expected");
            Assert.IsTrue(data.GetContact(4).Penetration == 2, "Penetration 2 expected");
            Assert.IsTrue(data.GetContact(5).Penetration == 2, "Penetration 2 expected");
            Assert.IsTrue(data.GetContact(6).Penetration == 2, "Penetration 2 expected");
            Assert.IsTrue(data.GetContact(7).Penetration == 2, "Penetration 2 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[0], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[1], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[2], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[3], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(4).Position, box.OrientedBoundingBox.GetCorners()[4], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(5).Position, box.OrientedBoundingBox.GetCorners()[5], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(6).Position, box.OrientedBoundingBox.GetCorners()[6], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(7).Position, box.OrientedBoundingBox.GetCorners()[7], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(4).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(5).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(6).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(7).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            data.Reset();
        }
        [TestMethod()]
        public void ContactDetectorBoxAndHalfSpaceBellowIntersectionTest()
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(-Vector3.One, Vector3.One, Matrix.Translation(Vector3.Down * 2f));
            Assert.IsTrue(ContactDetector.BoxAndHalfSpace(box, plane, data), "Intersection expected");
            Assert.IsTrue(data.ContactCount == 8, "Eight contacts expected");
            Assert.IsTrue(data.GetContact(0).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(1).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(2).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(3).Penetration == 1, "Penetration 1 expected");
            Assert.IsTrue(data.GetContact(4).Penetration == 3, "Penetration 3 expected");
            Assert.IsTrue(data.GetContact(5).Penetration == 3, "Penetration 3 expected");
            Assert.IsTrue(data.GetContact(6).Penetration == 3, "Penetration 3 expected");
            Assert.IsTrue(data.GetContact(7).Penetration == 3, "Penetration 3 expected");
            Assert.AreEqual(data.GetContact(0).Position, box.OrientedBoundingBox.GetCorners()[0], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(1).Position, box.OrientedBoundingBox.GetCorners()[1], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(2).Position, box.OrientedBoundingBox.GetCorners()[2], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(3).Position, box.OrientedBoundingBox.GetCorners()[3], "Upper vertex expected");
            Assert.AreEqual(data.GetContact(4).Position, box.OrientedBoundingBox.GetCorners()[4], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(5).Position, box.OrientedBoundingBox.GetCorners()[5], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(6).Position, box.OrientedBoundingBox.GetCorners()[6], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(7).Position, box.OrientedBoundingBox.GetCorners()[7], "Bottom vertex expected");
            Assert.AreEqual(data.GetContact(0).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(1).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(2).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(3).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(4).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(5).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(6).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            Assert.AreEqual(data.GetContact(7).Normal, plane.Normal, "Contact normal equals to plane normal vertex expected");
            data.Reset();
        }

        [TestMethod()]
        public void ContactDetectorBoxAndTriangleSoup1Test()
        {
            ContactResolver data = new ContactResolver();

            float boxSize = 1f;
            var box = FromAABB(-Vector3.One * boxSize, Vector3.One * boxSize, Matrix.Identity);

            float triSize = 1f;
            var xTri = new Triangle(new Vector3(1, 0, -1) * triSize, new Vector3(-1, 0, -1) * triSize, new Vector3(0, 0, 1) * triSize);
            var yTri = new Triangle(new Vector3(-1, -1, 0) * triSize, new Vector3(1, -1, 0) * triSize, new Vector3(0, 1, 0) * triSize);
            var zTri = new Triangle(new Vector3(0, -1, -1) * triSize, new Vector3(0, -1, 1) * triSize, new Vector3(0, 1, 0) * triSize);




            var tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle over the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle bellow the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();


            float p = 0f;

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the top plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the bottom plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();



            p = -1f + 0.99f;

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();






            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in front of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle behind the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();




            p = 0f;

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the forward plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the backward plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();



            p = -1f + 0.99f;

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();





            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle at left of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle at right of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();




            p = 0f;

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the left plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the right plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();




            p = -1f + 0.99f;

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();
        }
        [TestMethod()]
        public void ContactDetectorBoxAndTriangleSoup2Test()
        {
            ContactResolver data = new ContactResolver();

            float boxSize = 10f;
            var box = FromAABB(-Vector3.One * boxSize, Vector3.One * boxSize, Matrix.Identity);

            float triSize = 1f;
            var xTri = new Triangle(new Vector3(1, 0, -1) * triSize, new Vector3(-1, 0, -1) * triSize, new Vector3(0, 0, 1) * triSize);
            var yTri = new Triangle(new Vector3(-1, -1, 0) * triSize, new Vector3(1, -1, 0) * triSize, new Vector3(0, 1, 0) * triSize);
            var zTri = new Triangle(new Vector3(0, -1, -1) * triSize, new Vector3(0, -1, 1) * triSize, new Vector3(0, 1, 0) * triSize);




            var tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle over the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle bellow the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();


            float p = 0f;

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the top plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the bottom plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();



            p = -boxSize + (boxSize * 0.99f);

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();






            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in front of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle behind the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();




            p = 0f;

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the forward plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the backward plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();



            p = -boxSize + (boxSize * 0.99f);

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();





            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle at left of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle at right of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();




            p = 0f;

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the left plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the right plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();




            p = -boxSize + (boxSize * 0.99f);

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 3);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                tri.Triangles.ElementAt(0).GetVertices().ToArray());
            data.Reset();
        }
        [TestMethod()]
        public void ContactDetectorBoxAndTriangleSoup3Test()
        {
            ContactResolver data = new ContactResolver();

            float boxSize = 1f;
            var box = FromAABB(-Vector3.One * boxSize, Vector3.One * boxSize, Matrix.Identity);

            float triSize = 10f;
            var xTri = new Triangle(new Vector3(1, 0, -1) * triSize, new Vector3(-1, 0, -1) * triSize, new Vector3(0, 0, 1) * triSize);
            var yTri = new Triangle(new Vector3(-1, -1, 0) * triSize, new Vector3(1, -1, 0) * triSize, new Vector3(0, 1, 0) * triSize);
            var zTri = new Triangle(new Vector3(0, -1, -1) * triSize, new Vector3(0, -1, 1) * triSize, new Vector3(0, 1, 0) * triSize);



            var tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle over the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle bellow the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();



            float p = 0f;

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the top plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(0), box.OrientedBoundingBox.GetCorner(1), box.OrientedBoundingBox.GetCorner(2), box.OrientedBoundingBox.GetCorner(3) });
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the bottom plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(4), box.OrientedBoundingBox.GetCorner(5), box.OrientedBoundingBox.GetCorner(6), box.OrientedBoundingBox.GetCorner(7) });
            data.Reset();



            p = -1f + 0.99f;
            var v = Vector3.Up * p;

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Up * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the top plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(0) + v, box.OrientedBoundingBox.GetCorner(1) + v, box.OrientedBoundingBox.GetCorner(2) + v, box.OrientedBoundingBox.GetCorner(3) + v });
            data.Reset();

            tri = FromTriangle(xTri, Matrix.Translation(Vector3.Down * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the bottom plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(4) - v, box.OrientedBoundingBox.GetCorner(5) - v, box.OrientedBoundingBox.GetCorner(6) - v, box.OrientedBoundingBox.GetCorner(7) - v });
            data.Reset();



            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in front of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle behind the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();



            p = 0f;

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the forward plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(0), box.OrientedBoundingBox.GetCorner(3), box.OrientedBoundingBox.GetCorner(4), box.OrientedBoundingBox.GetCorner(7) });
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the backward plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(1), box.OrientedBoundingBox.GetCorner(2), box.OrientedBoundingBox.GetCorner(5), box.OrientedBoundingBox.GetCorner(6) });
            data.Reset();


            p = -1f + 0.99f;
            v = Vector3.ForwardLH * p;

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.ForwardLH * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(0) + v, box.OrientedBoundingBox.GetCorner(3) + v, box.OrientedBoundingBox.GetCorner(4) + v, box.OrientedBoundingBox.GetCorner(7) + v });
            data.Reset();

            tri = FromTriangle(yTri, Matrix.Translation(Vector3.BackwardLH * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(1) - v, box.OrientedBoundingBox.GetCorner(2) - v, box.OrientedBoundingBox.GetCorner(5) - v, box.OrientedBoundingBox.GetCorner(6) - v });
            data.Reset();




            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle at left of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize * 2f));
            Assert.IsFalse(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle at right of the box. Not intersection expected");
            Assert.IsTrue(data.ContactCount == 0);
            data.Reset();



            p = 0f;

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the left plane of the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(2), box.OrientedBoundingBox.GetCorner(3), box.OrientedBoundingBox.GetCorner(6), box.OrientedBoundingBox.GetCorner(7) });
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle in the right plane the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(0), box.OrientedBoundingBox.GetCorner(1), box.OrientedBoundingBox.GetCorner(4), box.OrientedBoundingBox.GetCorner(5) });
            data.Reset();



            p = -1f + 0.99f;
            v = Vector3.Left * p;

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Left * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(2) + v, box.OrientedBoundingBox.GetCorner(3) + v, box.OrientedBoundingBox.GetCorner(6) + v, box.OrientedBoundingBox.GetCorner(7) + v });
            data.Reset();

            tri = FromTriangle(zTri, Matrix.Translation(Vector3.Right * boxSize * 0.99f));
            Assert.IsTrue(ContactDetector.BoxAndTriangleSoup(box, tri, data), "Triangle into the box. Intersection expected");
            Assert.IsTrue(data.ContactCount == 4);
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Penetration).ToArray(),
                new[] { p, p, p, p });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Normal).ToArray(),
                new[] { tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal, tri.Triangles.ElementAt(0).Normal });
            CollectionAssert.AreEquivalent(
                data.GetContacts().Select(p => p.Position).ToArray(),
                new[] { box.OrientedBoundingBox.GetCorner(0) - v, box.OrientedBoundingBox.GetCorner(1) - v, box.OrientedBoundingBox.GetCorner(4) - v, box.OrientedBoundingBox.GetCorner(5) - v });
            data.Reset();
        }
    }
}
