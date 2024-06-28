using Engine.Physics;
using Engine.Physics.Colliders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorHalfSpaceAndSphereTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new(MathUtil.ZeroTolerance);

        static HalfSpaceCollider FromPlane(Vector3 normal, float d, Matrix transform)
        {
            var p = new Plane(normal, d);
            var plane = new HalfSpaceCollider(p);
            var planeBody = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            plane.Attach(planeBody);

            return plane;
        }

        static SphereCollider FromRadius(float radius, Matrix transform)
        {
            var sphere = new SphereCollider(radius);
            var boxBody = new RigidBody(new() { Mass = 1, InitialTransform = transform });
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

        public struct HalfSpaceAndSphereData
        {
            public Matrix Transform { get; set; }
            public bool IntersectionExpected { get; set; }
            public HalfSpaceAndSphereContactData Contact { get; set; }
        }

        public struct HalfSpaceAndSphereContactData
        {
            public Vector3 Point { get; set; }
            public float Penetration { get; set; }
        }

        public static IEnumerable<object[]> HalfSpaceAndSphereTestData
        {
            get
            {
                return new[]
                {
                    new object[]
                    {
                        new HalfSpaceAndSphereData
                        {
                            Transform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndSphereData
                        {
                            Transform = Matrix.Translation(Vector3.Up),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndSphereContactData{ Point = Vector3.Zero, Penetration = 0 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndSphereData
                        {
                            Transform = Matrix.Identity,
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndSphereContactData{ Point = Vector3.Zero, Penetration = 1 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndSphereData
                        {
                            Transform = Matrix.Translation(Vector3.Down),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndSphereContactData{ Point = Vector3.Zero, Penetration = 2 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndSphereData
                        {
                            Transform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndSphereContactData{ Point = Vector3.Zero, Penetration = 3 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndSphereData
                        {
                            Transform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndSphereContactData{ Point = Vector3.Zero, Penetration = 6 },
                        }
                    },
                };
            }
        }

        [TestMethod()]
        [DynamicData(nameof(HalfSpaceAndSphereTestData))]
        public void ContactDetectorHalfSpaceAndSphereTest(HalfSpaceAndSphereData testData)
        {
            var data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var sphere = FromRadius(1f, testData.Transform);
            bool intersection = ContactDetector.BetweenObjects(sphere, plane, data);

            Assert.AreEqual(testData.IntersectionExpected, intersection, testData.IntersectionExpected ? "Intersection expected" : "No intersection expected");

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
