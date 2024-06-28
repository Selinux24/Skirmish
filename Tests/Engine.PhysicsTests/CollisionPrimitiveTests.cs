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
            var normal = Vector3.Up;
            float distance = 1f;
            var sourcePlane = new Plane(normal, distance);

            var plane = new HalfSpaceCollider(sourcePlane);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());

            plane = new HalfSpaceCollider(normal, distance);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());

            var rbody = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            plane.Attach(rbody);
            Assert.AreEqual(plane.Normal, normal);
            Assert.AreEqual(plane.D, distance);
            Assert.AreEqual(plane.Plane, sourcePlane);
            Assert.AreEqual(plane.BoundingBox, new BoundingBox());
            Assert.AreEqual(plane.BoundingSphere, new BoundingSphere());
            Assert.AreEqual(plane.OrientedBoundingBox, new OrientedBoundingBox());

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(new() { Mass = 1f, InitialTransform = trn });

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
            var extents = Vector3.Up;
            var sourceBox = new BoundingBox(-extents, extents);
            var sphere = SharpDXExtensions.BoundingSphereFromPoints(sourceBox.GetVertices().ToArray());
            var obb = new OrientedBoundingBox(sourceBox.GetVertices().ToArray());

            var box = new BoxCollider(extents);
            Assert.AreEqual(box.Extents, extents);
            Assert.AreEqual(box.BoundingBox, sourceBox);
            Assert.AreEqual(box.BoundingSphere, sphere);
            Assert.AreEqual(box.OrientedBoundingBox, obb);

            var rbody = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            box.Attach(rbody);
            Assert.AreEqual(box.Extents, extents);
            Assert.AreEqual(box.BoundingBox, sourceBox);
            Assert.AreEqual(box.BoundingSphere, sphere);
            Assert.AreEqual(box.OrientedBoundingBox, obb);

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(new() { Mass = 1f, InitialTransform = trn });
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
            var sourceSphere = new BoundingSphere(Vector3.Zero, radius);
            var box = BoundingBox.FromSphere(sourceSphere);
            var obb = new OrientedBoundingBox(box.GetVertices().ToArray());

            var sphere = new SphereCollider(radius);
            Assert.AreEqual(sphere.Radius, radius);
            Assert.AreEqual(sphere.BoundingSphere, sourceSphere);
            Assert.AreEqual(sphere.BoundingBox, box);
            Assert.AreEqual(sphere.OrientedBoundingBox, obb);

            var rbody = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            sphere.Attach(rbody);
            Assert.AreEqual(sphere.Radius, radius);
            Assert.AreEqual(sphere.BoundingSphere, sourceSphere);
            Assert.AreEqual(sphere.BoundingBox, box);
            Assert.AreEqual(sphere.OrientedBoundingBox, obb);

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(new() { Mass = 1f, InitialTransform = trn });
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
            var t1 = new Triangle(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);
            var t2 = new Triangle(Vector3.UnitX, -Vector3.UnitY, Vector3.UnitZ);
            var t3 = new Triangle(Vector3.UnitX, -Vector3.UnitY, -Vector3.UnitZ);
            var t4 = new Triangle(Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ);
            Triangle[] allTris = new[] { t1, t2, t3, t4 };
            Triangle[] distinctTris = allTris.Distinct().ToArray();
            Vector3[] allPoints = allTris.SelectMany(t => t.GetVertices()).ToArray();
            Vector3[] distinctPoints = allPoints.Distinct().ToArray();
            var sphere = SharpDXExtensions.BoundingSphereFromPoints(distinctPoints);
            var box = SharpDXExtensions.BoundingBoxFromPoints(distinctPoints);
            var obb = new OrientedBoundingBox(distinctPoints);

            var soup = new ConvexMeshCollider(allTris);
            CollectionAssert.AreEquivalent(soup.GetTriangles(true).ToArray(), distinctTris);
            CollectionAssert.AreEquivalent(soup.GetVertices(true).ToArray(), distinctPoints);
            Assert.AreEqual(soup.BoundingSphere, sphere);
            Assert.AreEqual(soup.BoundingBox, box);
            Assert.AreEqual(soup.OrientedBoundingBox, obb);

            var rbody = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            soup.Attach(rbody);
            CollectionAssert.AreEquivalent(soup.GetTriangles(true).ToArray(), distinctTris);
            CollectionAssert.AreEquivalent(soup.GetVertices(true).ToArray(), distinctPoints);
            Assert.AreEqual(soup.BoundingSphere, sphere);
            Assert.AreEqual(soup.BoundingBox, box);
            Assert.AreEqual(soup.OrientedBoundingBox, obb);

            var trn = Matrix.Translation(Vector3.One);
            rbody = new RigidBody(new() { Mass = 1f, InitialTransform = trn });
            allTris = allTris.Select(t => Triangle.Transform(t, trn)).ToArray();
            distinctTris = allTris.Distinct().ToArray();
            allPoints = allTris.SelectMany(t => t.GetVertices()).ToArray();
            distinctPoints = allPoints.Distinct().ToArray();
            sphere = sphere.SetTransform(trn);
            box = box.SetTransform(trn);
            obb = obb.SetTransform(trn);
            soup.Attach(rbody);
            CollectionAssert.AreEquivalent(soup.GetTriangles(true).ToArray(), distinctTris);
            CollectionAssert.AreEquivalent(soup.GetVertices(true).ToArray(), distinctPoints);
            Assert.AreEqual(soup.BoundingSphere, sphere);
            Assert.AreEqual(soup.BoundingBox, box);
            Assert.AreEqual(soup.OrientedBoundingBox, obb);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConvexMeshCollider(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConvexMeshCollider(Enumerable.Empty<Triangle>()));
        }

        [TestMethod()]
        public void CollisionPrimitiveAttachTest()
        {
            var box = new BoxCollider(Vector3.One);

            var body = new RigidBody(new() { Mass = 1f, InitialTransform = Matrix.Identity });
            box.Attach(body);
            Assert.AreEqual(box.RigidBody, body);

            Assert.ThrowsException<ArgumentNullException>(() => box.Attach(null));
        }
    }
}
