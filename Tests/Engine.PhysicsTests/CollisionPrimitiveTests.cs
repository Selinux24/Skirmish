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
    public class CollisionPrimitiveTests
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

        [TestMethod()]
        public void CollisionPlaneTest()
        {
            Vector3 normal = Vector3.Up;
            float distance = 1f;
            Plane sourcePlane = new Plane(normal, distance);

            CollisionPlane plane = new CollisionPlane(sourcePlane);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());

            plane = new CollisionPlane(normal, distance);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());

            RigidBody rbody = new RigidBody(1f, Matrix.Identity);
            plane.Attach(rbody);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(1f, trn);

            plane.Attach(rbody);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());
        }

        [TestMethod()]
        public void CollisionBoxTest()
        {
            Vector3 extents = Vector3.Up;
            BoundingBox sourceBox = new BoundingBox(-extents, extents);
            BoundingSphere sphere = BoundingSphere.FromPoints(sourceBox.GetVertices().ToArray());
            OrientedBoundingBox obb = new OrientedBoundingBox(sourceBox.GetVertices().ToArray());

            CollisionBox box = new CollisionBox(extents);
            Assert.AreEqual(box.Extents, extents);
            Assert.AreEqual(box.BoundingBox, sourceBox);
            Assert.AreEqual(box.BoundingSphere, sphere);
            Assert.AreEqual(box.OrientedBoundingBox, obb);

            RigidBody rbody = new RigidBody(1f, Matrix.Identity);
            box.Attach(rbody);
            Assert.AreEqual(box.Extents, extents);
            Assert.AreEqual(box.BoundingBox, sourceBox);
            Assert.AreEqual(box.BoundingSphere, sphere);
            Assert.AreEqual(box.OrientedBoundingBox, obb);

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(1f, trn);
            sourceBox = sourceBox.SetTransform(trn);
            sphere = sphere.SetTransform(trn);
            obb = obb.SetTransform(trn);
            box.Attach(rbody);
            Assert.AreEqual(box.Extents, extents);
            Assert.AreEqual(box.BoundingBox, sourceBox);
            Assert.AreEqual(box.BoundingSphere, sphere);
            Assert.AreEqual(box.OrientedBoundingBox, obb);
        }

        [TestMethod()]
        public void CollisionSphereTest()
        {
            float radius = 1f;
            BoundingSphere sourceSphere = new BoundingSphere(Vector3.Zero, radius);
            BoundingBox box = BoundingBox.FromSphere(sourceSphere);
            OrientedBoundingBox obb = new OrientedBoundingBox(box.GetVertices().ToArray());

            CollisionSphere sphere = new CollisionSphere(radius);
            Assert.AreEqual(sphere.Radius, radius);
            Assert.AreEqual(sphere.BoundingSphere, sourceSphere);
            Assert.AreEqual(sphere.BoundingBox, box);
            Assert.AreEqual(sphere.OrientedBoundingBox, obb);

            RigidBody rbody = new RigidBody(1f, Matrix.Identity);
            sphere.Attach(rbody);
            Assert.AreEqual(sphere.Radius, radius);
            Assert.AreEqual(sphere.BoundingSphere, sourceSphere);
            Assert.AreEqual(sphere.BoundingBox, box);
            Assert.AreEqual(sphere.OrientedBoundingBox, obb);

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(1f, trn);
            sourceSphere = sourceSphere.SetTransform(trn);
            box = box.SetTransform(trn);
            obb = obb.SetTransform(trn);
            sphere.Attach(rbody);
            Assert.AreEqual(sphere.Radius, radius);
            Assert.AreEqual(sphere.BoundingSphere, sourceSphere);
            Assert.AreEqual(sphere.BoundingBox, box);
            Assert.AreEqual(sphere.OrientedBoundingBox, obb);
        }

        [TestMethod()]
        public void CollisionTriangleSoupTest()
        {
            Triangle t1 = new Triangle(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);
            Triangle t2 = new Triangle(Vector3.UnitX, -Vector3.UnitY, Vector3.UnitZ);
            Triangle t3 = new Triangle(Vector3.UnitX, -Vector3.UnitY, -Vector3.UnitZ);
            Triangle t4 = new Triangle(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);
            Triangle[] allTris = new[] { t1, t2, t3, t4 };
            Triangle[] distinctTris = allTris.Distinct().ToArray();
            Vector3[] allPoints = allTris.SelectMany(t => t.GetVertices()).ToArray();
            Vector3[] distinctPoints = allPoints.Distinct().ToArray();
            BoundingSphere sphere = BoundingSphere.FromPoints(distinctPoints);
            BoundingBox box = BoundingBox.FromPoints(distinctPoints);
            OrientedBoundingBox obb = new OrientedBoundingBox(distinctPoints);

            CollisionTriangleSoup soup = new CollisionTriangleSoup(allTris);
            CollectionAssert.AreEquivalent(soup.Triangles.ToArray(), distinctTris);
            CollectionAssert.AreEquivalent(soup.Vertices.ToArray(), distinctPoints);
            Assert.AreEqual(soup.BoundingSphere, sphere);
            Assert.AreEqual(soup.BoundingBox, box);
            Assert.AreEqual(soup.OrientedBoundingBox, obb);

            RigidBody rbody = new RigidBody(1f, Matrix.Identity);
            soup.Attach(rbody);
            CollectionAssert.AreEquivalent(soup.Triangles.ToArray(), distinctTris);
            CollectionAssert.AreEquivalent(soup.Vertices.ToArray(), distinctPoints);
            Assert.AreEqual(soup.BoundingSphere, sphere);
            Assert.AreEqual(soup.BoundingBox, box);
            Assert.AreEqual(soup.OrientedBoundingBox, obb);

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(1f, trn);
            allTris = allTris.Select(t => Triangle.Transform(t, trn)).ToArray();
            distinctTris = allTris.Distinct().ToArray();
            allPoints = allTris.SelectMany(t => t.GetVertices()).ToArray();
            distinctPoints = allPoints.Distinct().ToArray();
            sphere = sphere.SetTransform(trn);
            box = box.SetTransform(trn);
            obb = obb.SetTransform(trn);
            soup.Attach(rbody);
            CollectionAssert.AreEquivalent(soup.Triangles.ToArray(), distinctTris);
            CollectionAssert.AreEquivalent(soup.Vertices.ToArray(), distinctPoints);
            Assert.AreEqual(soup.BoundingSphere, sphere);
            Assert.AreEqual(soup.BoundingBox, box);
            Assert.AreEqual(soup.OrientedBoundingBox, obb);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionTriangleSoup(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CollisionTriangleSoup(Enumerable.Empty<Triangle>()));
        }

        [TestMethod()]
        public void CollisionPrimitiveAttachTest()
        {
            CollisionBox box = new CollisionBox(Vector3.One);

            RigidBody body = new RigidBody(1, Matrix.Identity);
            box.Attach(body);
            Assert.AreEqual(box.RigidBody, body);

            Assert.ThrowsException<ArgumentNullException>(() => box.Attach(null));
        }
    }
}
