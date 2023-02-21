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
    public class ContactDetectorBoxAndTriangleSoupTests
    {
        static TestContext _testContext;

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
    }
}
