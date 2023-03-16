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
    public class ContactDetectorHalfSpaceAndCylinderTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new Vector3(MathUtil.ZeroTolerance);

        static HalfSpaceCollider FromPlane(Vector3 point, Vector3 normal, Matrix transform)
        {
            Plane p = new Plane(point, normal);
            HalfSpaceCollider plane = new HalfSpaceCollider(p);
            RigidBody planeBody = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            plane.Attach(planeBody);

            return plane;
        }

        static CylinderCollider FromRadius(float radius, float height, Matrix transform)
        {
            CylinderCollider cylinder = new CylinderCollider(radius, height);
            RigidBody boxBody = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            cylinder.Attach(boxBody);

            return cylinder;
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

        public struct HalfSpaceAndCylinderData
        {
            public string Description { get; set; }
            public Matrix Transform { get; set; }
            public bool IntersectionExpected { get; set; }
            public HalfSpaceAndCylinderContactData Contact { get; set; }
        }

        public struct HalfSpaceAndCylinderContactData
        {
            public Vector3 Point { get; set; }
            public Vector3 Normal { get; set; }
            public float Penetration { get; set; }
        }

        public static IEnumerable<object[]> HalfSpaceAndCylinderTestData
        {
            get
            {
                return new[]
                {
                    // Axis Aligned Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with no intersection.",
                            Transform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with perfect base intersection.",
                            Transform = Matrix.Translation(Vector3.Up),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 0 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with intersection and penetration of 1.",
                            Transform = Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 1 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with perfect cap intersection and penetration of 2.",
                            Transform = Matrix.Translation(Vector3.Down),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 2 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 3.",
                            Transform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 3 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 6.",
                            Transform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 6 },
                        }
                    },

                    // 90º rotated Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with no intersection.",
                            Transform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with perfect side intersection.",
                            Transform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 1f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Vector3.BackwardLH, Penetration = 0f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            Transform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Vector3.BackwardLH, Penetration = 1f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            Transform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Vector3.BackwardLH, Penetration = 2f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            Transform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Vector3.BackwardLH, Penetration = 3f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            Transform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Vector3.BackwardLH, Penetration = 6f },
                        }
                    },
                };
            }
        }

        [TestMethod()]
        [DynamicData(nameof(HalfSpaceAndCylinderTestData))]
        public void ContactDetectorHalfSpaceAndCylinderTest(HalfSpaceAndCylinderData testData)
        {
            Console.WriteLine(testData.Description);

            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Zero, Vector3.Up, Matrix.Identity);

            var cylinder = FromRadius(1f, 2f, testData.Transform);
            bool intersection = ContactDetector.BetweenObjects(cylinder, plane, data);

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
            var expectedNormal = expectedContact.Normal;

            Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Expected penetration {expectedPenetration} != {contact.Penetration}");
            Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Expected position {expectedPosition} != {contact.Position}");
            Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Expected normal {expectedNormal} != {contact.Normal}");
        }
    }
}
