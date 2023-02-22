using Engine.Physics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorBoxAndTriangleSoupTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new Vector3(MathUtil.ZeroTolerance);

        static CollisionBox FromAABB(Vector3 extents, Matrix transform)
        {
            CollisionBox box = new CollisionBox(extents);
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

        static CollisionTriangleSoup FromRectangle(RectangleF rect, float d, Matrix transform)
        {
            Vector3 p1 = new Vector3(rect.BottomLeft.X, d, rect.BottomLeft.Y);
            Vector3 p2 = new Vector3(rect.BottomRight.X, d, rect.BottomRight.Y);
            Vector3 p3 = new Vector3(rect.TopLeft.X, d, rect.TopLeft.Y);
            Vector3 p4 = new Vector3(rect.TopRight.X, d, rect.TopRight.Y);

            Triangle tri1 = new Triangle(p1, p2, p3);
            Triangle tri2 = new Triangle(p3, p2, p4);

            CollisionTriangleSoup ctri = new CollisionTriangleSoup(new[] { tri1, tri2 });
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
        public void ContactDetectorBoxAndTriangleSoup1Test()
        {
            ContactResolver data = new ContactResolver();

            float boxSize = 1f;
            var box = FromAABB(Vector3.One * boxSize, Matrix.Identity);

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
            var box = FromAABB(Vector3.One * boxSize, Matrix.Identity);

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
            var box = FromAABB(Vector3.One * boxSize, Matrix.Identity);

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

        public struct BoxAndTriangleSoupData
        {
            public Matrix BoxTransform { get; set; }
            public bool IntersectioExpected { get; set; }
            public BoxAndTriangleSoupContactData[] Contacts { get; set; }
            public int ContactCount
            {
                get { return Contacts?.Length ?? 0; }
            }
        }

        public struct BoxAndTriangleSoupContactData
        {
            public BoxCorners Corner { get; set; }
            public float Penetration { get; set; }
        }

        public static IEnumerable<object[]> BoxAndTriangleSoupTestData
        {
            get
            {
                return new[]
                {
                    //Un-rotated box - Bottom face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.Identity,
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 3 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 6 },
                            }
                        }
                    },

                    //X-axis 90º - Front face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 3 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 6 },
                            }
                        }
                    },

                    //X-axis 180º - Top face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 4 },
                            }
                        }
                    },

                    //X-axis 270º - Back face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 4 },
                            }
                        }
                    },

                    //Z-axis 90º - Left face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 3 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 6 },
                            }
                        }
                    },

                    //Z-axis -90º - Right face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 3 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 6 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 4 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 4 },
                            }
                        }
                    },
                };
            }
        }

        [TestMethod()]
        [DynamicData(nameof(BoxAndTriangleSoupTestData))]
        public void ContactDetectorBoxAndHalfSpaceTest(BoxAndTriangleSoupData testData)
        {
            ContactResolver data = new ContactResolver();

            RectangleF rect = new RectangleF(-100f, -100f, 200f, 200f);
            var soup = FromRectangle(rect, 0, Matrix.Identity);

            var box = FromAABB(Vector3.One, testData.BoxTransform);
            bool intersection = ContactDetector.BoxAndTriangleSoup(box, soup, data);

            Assert.AreEqual(testData.IntersectioExpected, intersection, testData.IntersectioExpected ? "Intersection expected" : "No intersection expected");
            Assert.AreEqual(testData.ContactCount, data.ContactCount, $"{testData.ContactCount} contacts expected");

            if (!intersection)
            {
                return;
            }

            for (int i = 0; i < testData.ContactCount; i++)
            {
                var contact = data.GetContact(i);

                var expectedContact = testData.Contacts[i];
                var expectedPenetration = expectedContact.Penetration;
                var expectedPosition = box.OrientedBoundingBox.GetCorner(expectedContact.Corner);
                var expectedNormal = Vector3.Up;

                Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Contact {i}. Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Contact {i}. Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Contact {i}. Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
