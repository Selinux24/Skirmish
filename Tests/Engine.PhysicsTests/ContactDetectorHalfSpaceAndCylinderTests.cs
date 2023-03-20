using Engine.Physics;
using Engine.Physics.Colliders;
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
            public Matrix PlaneTransform { get; set; } = Matrix.Identity;
            public Matrix CylinderTransform { get; set; } = Matrix.Identity;
            public bool IntersectionExpected { get; set; } = false;
            public IEnumerable<HalfSpaceAndCylinderContactData> Contacts { get; set; }

            public HalfSpaceAndCylinderData()
            {

            }
        }

        public struct HalfSpaceAndCylinderContactData
        {
            public Vector3 Point { get; set; }
            public Vector3 Normal { get; set; }
            public float Penetration { get; set; }
        }

        private const float PiOverFourDisplacement = 0.41421354f;
        private const float PiOverFourNormal = 0.7071067f;
        private static readonly Vector3 PiOverFourPlaneNormal = Vector3.TransformNormal(Vector3.Up, Matrix.RotationX(MathUtil.PiOverFour));

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
                            CylinderTransform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with perfect base intersection.",
                            CylinderTransform = Matrix.Translation(Vector3.Up),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = Vector3.Zero, Normal = Vector3.Up, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with perfect cap intersection and penetration of 2.",
                            CylinderTransform = Matrix.Translation(Vector3.Down),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2, 0), Normal = Vector3.Up, Penetration = 2 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -3, 0), Normal = Vector3.Up, Penetration = 3 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 0), Normal = Vector3.Up, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -6, 0), Normal = Vector3.Up, Penetration = 6 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -4, 0), Normal = Vector3.Up, Penetration = 4 },
                            }
                        }
                    },

                    // 90º rotated Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with perfect side intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 1f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, -1), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 1), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, -1), Normal = Vector3.Up, Penetration = 1f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 1), Normal = Vector3.Up, Penetration = 1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2, -1), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, -1), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2, 1), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 1), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -3, -1), Normal = Vector3.Up, Penetration = 3f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, -1), Normal = Vector3.Up, Penetration = 1f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -3, 1), Normal = Vector3.Up, Penetration = 3f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 1), Normal = Vector3.Up, Penetration = 1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -6, -1), Normal = Vector3.Up, Penetration = 6f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -4, -1), Normal = Vector3.Up, Penetration = 4f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -6, 1), Normal = Vector3.Up, Penetration = 6f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -4, 1), Normal = Vector3.Up, Penetration = 4f },
                            }
                        }
                    },

                    // -90º rotated Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with perfect side intersection.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 1f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 1), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, -1), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 1), Normal = Vector3.Up, Penetration = 1f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, -1), Normal = Vector3.Up, Penetration = 1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2, 1), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 1), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2, -1), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, -1), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -3, 1), Normal = Vector3.Up, Penetration = 3f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 1), Normal = Vector3.Up, Penetration = 1f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -3, -1), Normal = Vector3.Up, Penetration = 3f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, -1), Normal = Vector3.Up, Penetration = 1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -6, 1), Normal = Vector3.Up, Penetration = 6f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -4, 1), Normal = Vector3.Up, Penetration = 4f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -6, -1), Normal = Vector3.Up, Penetration = 6f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -4, -1), Normal = Vector3.Up, Penetration = 4f },
                            }
                        }
                    },
         
                    // 180º rotated Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with perfect cap intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up * 1f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 0), Normal = Vector3.Up, Penetration = 1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with perfect base intersection and penetration of 2.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2, 0), Normal = Vector3.Up, Penetration = 2f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 0), Normal = Vector3.Up, Penetration = 1f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -3, 0), Normal = Vector3.Up, Penetration = 3f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -4, 0), Normal = Vector3.Up, Penetration = 4f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -6, 0), Normal = Vector3.Up, Penetration = 6f },
                            }
                        }
                    },

                    // 45º rotated Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "45º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "45º rotated Cylinder over the plane with perfect point intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Up * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder over the plane with intersection and penetration of {1 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 1 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 1 + PiOverFourDisplacement), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with perfect side intersection and penetration of {(1 + PiOverFourDisplacement) * 2f}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Down * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement) * 2f, 0), Normal = Vector3.Up, Penetration = (1 + PiOverFourDisplacement) * 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement), -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 1 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement), +(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 1 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with intersection and penetration of {3 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(3 + PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 3 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2f, -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2f, +(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 - PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 1 - PiOverFourDisplacement },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with intersection and penetration of {6 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(6 + PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 6 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -5f, -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 5f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -5f, +(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 5f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(4 - PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 4 - PiOverFourDisplacement },
                            }
                        }
                    },

                    // 135º rotated Cylinder
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "135º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "135º rotated Cylinder over the plane with perfect point intersection.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder over the plane with intersection and penetration of {1 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 0f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 1 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 1 + PiOverFourDisplacement), Normal = Vector3.Up, Penetration = 0f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with perfect side intersection and penetration of {(1 + PiOverFourDisplacement) * 2f}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement), -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 1 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = Vector3.Up, Penetration = 0 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement) * 2f, 0), Normal = Vector3.Up, Penetration = (1 + PiOverFourDisplacement) * 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 + PiOverFourDisplacement), +(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 1 + PiOverFourDisplacement },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with intersection and penetration of {3 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2f, -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 2f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(1 - PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 1 - PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(3 + PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 3 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -2f, +(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 2f },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with intersection and penetration of {6 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -5f, -(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 5f },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(4 - PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 4 - PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -(6 + PiOverFourDisplacement), 0), Normal = Vector3.Up, Penetration = 6 + PiOverFourDisplacement },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -5f, +(1 + PiOverFourDisplacement)), Normal = Vector3.Up, Penetration = 5f },
                            }
                        }
                    },
             
                    // Axis aligned cylinder's cap to 45º rotated plane - To test when both rigid body has transforms
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane over the plane with no intersection.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Up * -PiOverFourDisplacement),
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane over the plane with perfect base intersection.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Up) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = PiOverFourPlaneNormal, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane over the plane with intersection and penetration of 1.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Zero) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -PiOverFourNormal, -PiOverFourNormal), Normal = PiOverFourPlaneNormal, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane below the plane with perfect cap intersection and penetration of 2.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform =  Matrix.Translation(Vector3.Down) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -PiOverFourNormal * 2, -PiOverFourNormal * 2), Normal = PiOverFourPlaneNormal, Penetration = 2 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 0, 0), Normal = PiOverFourPlaneNormal, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane below the plane with intersection and penetration of 3.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Down * 2) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -PiOverFourNormal * 3, -PiOverFourNormal * 3), Normal = PiOverFourPlaneNormal, Penetration = 3 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -PiOverFourNormal, -PiOverFourNormal), Normal = PiOverFourPlaneNormal, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane below the plane with intersection and penetration of 6.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Down * 5) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contacts = new[]
                            {
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -PiOverFourNormal * 6, -PiOverFourNormal * 6), Normal = PiOverFourPlaneNormal, Penetration = 6 },
                                new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -PiOverFourNormal * 4, -PiOverFourNormal * 4), Normal = PiOverFourPlaneNormal, Penetration = 4 },
                            }
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

            var plane = FromPlane(Vector3.Zero, Vector3.Up, testData.PlaneTransform);

            var cylinder = FromRadius(1f, 2f, testData.CylinderTransform);
            bool intersection = ContactDetector.BetweenObjects(cylinder, plane, data);

            Assert.AreEqual(testData.IntersectionExpected, intersection, testData.IntersectionExpected ? "Intersection expected" : "No intersection expected");

            if (!intersection)
            {
                Assert.AreEqual(0, data.ContactCount, "Zero contacts expected");

                return;
            }

            int expectedCount = testData.Contacts.Count();

            Assert.AreEqual(expectedCount, data.ContactCount, $"{expectedCount} contacts expected");

            for (int i = 0; i < expectedCount; i++)
            {
                var contact = data.GetContact(i);

                var expectedContact = testData.Contacts.ElementAt(i);
                var expectedPenetration = expectedContact.Penetration;
                var expectedPosition = expectedContact.Point;
                var expectedNormal = expectedContact.Normal;

                Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
