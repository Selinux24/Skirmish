using Engine.Physics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorSphereAndHalfSpaceTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new Vector3(MathUtil.ZeroTolerance);

        static CollisionPlane FromPlane(Vector3 normal, float d, Matrix transform)
        {
            Plane p = new Plane(normal, d);
            CollisionPlane plane = new CollisionPlane(p);
            RigidBody planeBody = new RigidBody(float.PositiveInfinity, transform);
            plane.Attach(planeBody);

            return plane;
        }

        static CollisionSphere FromRadius(float radius, Matrix transform)
        {
            CollisionSphere sphere = new CollisionSphere(radius);
            RigidBody boxBody = new RigidBody(1, transform);
            sphere.Attach(boxBody);

            return sphere;
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

        public struct SphereAndHalfSpaceData
        {
            public Matrix SphereTransform { get; set; }
            public bool IntersectioExpected { get; set; }
            public SphereAndHalfSpaceContactData Contact { get; set; }
        }

        public struct SphereAndHalfSpaceContactData
        {
            public Vector3 Point { get; set; }
            public float Penetration { get; set; }
        }

        public static IEnumerable<object[]> SphereAndHalfSpaceTestData
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        new SphereAndHalfSpaceData
                        {
                            SphereTransform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new SphereAndHalfSpaceData
                        {
                            SphereTransform = Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contact = new SphereAndHalfSpaceContactData{ Point = Vector3.Zero, Penetration = 0 },
                        }
                    },
                    new object[]
                    {
                        new SphereAndHalfSpaceData
                        {
                            SphereTransform = Matrix.Identity,
                            IntersectioExpected = true,
                            Contact = new SphereAndHalfSpaceContactData{ Point = Vector3.Zero, Penetration = 1 },
                        }
                    },
                    new object[]
                    {
                        new SphereAndHalfSpaceData
                        {
                            SphereTransform = Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contact = new SphereAndHalfSpaceContactData{ Point = Vector3.Zero, Penetration = 2 },
                        }
                    },
                    new object[]
                    {
                        new SphereAndHalfSpaceData
                        {
                            SphereTransform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contact = new SphereAndHalfSpaceContactData{ Point = Vector3.Zero, Penetration = 3 },
                        }
                    },
                    new object[]
                    {
                        new SphereAndHalfSpaceData
                        {
                            SphereTransform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contact = new SphereAndHalfSpaceContactData{ Point = Vector3.Zero, Penetration = 6 },
                        }
                    },
                };
            }
        }

        [TestMethod()]
        [DynamicData(nameof(SphereAndHalfSpaceTestData))]
        public void ContactDetectorSphereAndHalfSpaceTest(SphereAndHalfSpaceData testData)
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var sphere = FromRadius(1f, testData.SphereTransform);
            bool intersection = ContactDetector.SphereAndHalfSpace(sphere, plane, data);

            Assert.AreEqual(testData.IntersectioExpected, intersection, testData.IntersectioExpected ? "Intersection expected" : "No intersection expected");

            if (!intersection)
            {
                Assert.AreEqual(0, data.ContactCount, "Zero contacts expected");

                return;
            }

            Assert.AreEqual(1f, data.ContactCount, $"One contact expected");

            var contact = data.GetContact(0);

            var expectedContact = testData.Contact;
            var expectedPenetration = expectedContact.Penetration;
            var expectedPosition = expectedContact.Point;
            var expectedNormal = plane.Normal;

            Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Expected penetration {expectedPenetration} != {contact.Penetration}");
            Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Expected position {expectedPosition} != {contact.Position}");
            Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Expected normal {expectedNormal} != {contact.Normal}");
        }
    }
}
