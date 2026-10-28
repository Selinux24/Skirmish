using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace Engine.PhysicsTests
{
    using Engine.Mathematics;
    using Engine.Physics;
    using Engine.Physics.Colliders;

    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorHalfSpaceAndCylinderTests
    {
        public TestContext TestContext { get; set; }

        static readonly Vector3 Epsilon = new(Helper.ZeroTolerance);

        static HalfSpaceCollider FromPlane(Vector3 point, Vector3 normal, Matrix4x4 transform)
        {
            var p = PlaneExtensions.CreatePlane(point, normal);
            var plane = new HalfSpaceCollider(p);
            var planeBody = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            plane.Attach(planeBody);

            return plane;
        }

        static CylinderCollider FromRadius(float radius, float height, Matrix4x4 transform)
        {
            var cylinder = new CylinderCollider(radius, height);
            var boxBody = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            cylinder.Attach(boxBody);

            return cylinder;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        public struct HalfSpaceAndCylinderData
        {
            public string Description { get; set; }
            public Matrix4x4 PlaneTransform { get; set; } = Matrix4x4.Identity;
            public Matrix4x4 CylinderTransform { get; set; } = Matrix4x4.Identity;
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
        private static readonly Vector3 PiOverFourPlaneNormal = Vector3.TransformNormal(Vector3Extensions.Up, Matrix4x4.CreateRotationX(Helper.PiOver4));

        public static IEnumerable<object[]> HalfSpaceAndCylinderTestData
        {
            get
            {
                return
                [
                    // Axis Aligned Cylinder
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with perfect base intersection.",
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = Vector3.Zero, Normal = Vector3Extensions.Up, Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = Vector3Extensions.Down, Normal = Vector3Extensions.Up, Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with perfect cap intersection and penetration of 2.",
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -2, 0), Normal = Vector3Extensions.Up, Penetration = 2 },
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -3, 0), Normal = Vector3Extensions.Up, Penetration = 3 },
                                new (){ Point = new Vector3(0, -1, 0), Normal = Vector3Extensions.Up, Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -6, 0), Normal = Vector3Extensions.Up, Penetration = 6 },
                                new (){ Point = new Vector3(0, -4, 0), Normal = Vector3Extensions.Up, Penetration = 4 },
                            ]
                        }
                    ],

                    // 90º rotated Cylinder
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with perfect side intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 1f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, -1), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, 0, 1), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -1, -1), Normal = Vector3Extensions.Up, Penetration = 1f },
                                new (){ Point = new Vector3(0, -1, 1), Normal = Vector3Extensions.Up, Penetration = 1f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 1f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -2, -1), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, 0, -1), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, -2, 1), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, 0, 1), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -3, -1), Normal = Vector3Extensions.Up, Penetration = 3f },
                                new (){ Point = new Vector3(0, -1, -1), Normal = Vector3Extensions.Up, Penetration = 1f },
                                new (){ Point = new Vector3(0, -3, 1), Normal = Vector3Extensions.Up, Penetration = 3f },
                                new (){ Point = new Vector3(0, -1, 1), Normal = Vector3Extensions.Up, Penetration = 1f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -6, -1), Normal = Vector3Extensions.Up, Penetration = 6f },
                                new (){ Point = new Vector3(0, -4, -1), Normal = Vector3Extensions.Up, Penetration = 4f },
                                new (){ Point = new Vector3(0, -6, 1), Normal = Vector3Extensions.Up, Penetration = 6f },
                                new (){ Point = new Vector3(0, -4, 1), Normal = Vector3Extensions.Up, Penetration = 4f },
                            ]
                        }
                    ],

                    // -90º rotated Cylinder
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with perfect side intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 1f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, 1), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, 0, -1), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix4x4.CreateRotationX(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -1, 1), Normal = Vector3Extensions.Up, Penetration = 1f },
                                new (){ Point = new Vector3(0, -1, -1), Normal = Vector3Extensions.Up, Penetration = 1f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            CylinderTransform = Matrix4x4.CreateRotationX(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 1f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -2, 1), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, 0, 1), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, -2, -1), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, 0, -1), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix4x4.CreateRotationX(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -3, 1), Normal = Vector3Extensions.Up, Penetration = 3f },
                                new (){ Point = new Vector3(0, -1, 1), Normal = Vector3Extensions.Up, Penetration = 1f },
                                new (){ Point = new Vector3(0, -3, -1), Normal = Vector3Extensions.Up, Penetration = 3f },
                                new (){ Point = new Vector3(0, -1, -1), Normal = Vector3Extensions.Up, Penetration = 1f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix4x4.CreateRotationX(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -6, 1), Normal = Vector3Extensions.Up, Penetration = 6f },
                                new (){ Point = new Vector3(0, -4, 1), Normal = Vector3Extensions.Up, Penetration = 4f },
                                new (){ Point = new Vector3(0, -6, -1), Normal = Vector3Extensions.Up, Penetration = 6f },
                                new (){ Point = new Vector3(0, -4, -1), Normal = Vector3Extensions.Up, Penetration = 4f },
                            ]
                        }
                    ],
         
                    // 180º rotated Cylinder
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with perfect cap intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 1f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -1, 0), Normal = Vector3Extensions.Up, Penetration = 1f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with perfect base intersection and penetration of 2.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 1f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, -2, 0), Normal = Vector3Extensions.Up, Penetration = 2f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -1, 0), Normal = Vector3Extensions.Up, Penetration = 1f },
                                new (){ Point = new Vector3(0, -3, 0), Normal = Vector3Extensions.Up, Penetration = 3f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -4, 0), Normal = Vector3Extensions.Up, Penetration = 4f },
                                new (){ Point = new Vector3(0, -6, 0), Normal = Vector3Extensions.Up, Penetration = 6f },
                            ]
                        }
                    ],

                    // 45º rotated Cylinder
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "45º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "45º rotated Cylinder over the plane with perfect point intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder over the plane with intersection and penetration of {1 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 1 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, 0, -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, 0, 1 + PiOverFourDisplacement), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with perfect side intersection and penetration of {(1 + PiOverFourDisplacement) * 2f}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement) * 2f, 0), Normal = Vector3Extensions.Up, Penetration = (1 + PiOverFourDisplacement) * 2f },
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement), -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 1 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement), +(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 1 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with intersection and penetration of {3 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -(3 + PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 3 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -2f, -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, -2f, +(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, -(1 - PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 1 - PiOverFourDisplacement },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with intersection and penetration of {6 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -(6 + PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 6 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -5f, -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 5f },
                                new (){ Point = new Vector3(0, -5f, +(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 5f },
                                new (){ Point = new Vector3(0, -(4 - PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 4 - PiOverFourDisplacement },
                            ]
                        }
                    ],

                    // 135º rotated Cylinder
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "135º rotated Cylinder over the plane with no intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4 + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "135º rotated Cylinder over the plane with perfect point intersection.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4 + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder over the plane with intersection and penetration of {1 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4 + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 0f },
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 1 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, 0, 1 + PiOverFourDisplacement), Normal = Vector3Extensions.Up, Penetration = 0f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with perfect side intersection and penetration of {(1 + PiOverFourDisplacement) * 2f}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4 + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement), -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 1 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, 0, 0), Normal = Vector3Extensions.Up, Penetration = 0 },
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement) * 2f, 0), Normal = Vector3Extensions.Up, Penetration = (1 + PiOverFourDisplacement) * 2f },
                                new (){ Point = new Vector3(0, -(1 + PiOverFourDisplacement), +(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 1 + PiOverFourDisplacement },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with intersection and penetration of {3 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4 + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -2f, -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 2f },
                                new (){ Point = new Vector3(0, -(1 - PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 1 - PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -(3 + PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 3 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -2f, +(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 2f },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with intersection and penetration of {6 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4 + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -5f, -(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 5f },
                                new (){ Point = new Vector3(0, -(4 - PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 4 - PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -(6 + PiOverFourDisplacement), 0), Normal = Vector3Extensions.Up, Penetration = 6 + PiOverFourDisplacement },
                                new (){ Point = new Vector3(0, -5f, +(1 + PiOverFourDisplacement)), Normal = Vector3Extensions.Up, Penetration = 5f },
                            ]
                        }
                    ],
             
                    // Axis aligned cylinder's cap to 45º rotated plane - To test when both rigid body has transforms
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane over the plane with no intersection.",
                            PlaneTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * -PiOverFourDisplacement),
                            CylinderTransform = Matrix4x4.CreateRotationX(Helper.PiOver4) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectionExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane over the plane with perfect base intersection.",
                            PlaneTransform = Matrix4x4.CreateRotationX(Helper.PiOver4),
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Up) * Matrix4x4.CreateRotationX(Helper.PiOver4),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, 0, 0), Normal = PiOverFourPlaneNormal, Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane over the plane with intersection and penetration of 1.",
                            PlaneTransform = Matrix4x4.CreateRotationX(Helper.PiOver4),
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3.Zero) * Matrix4x4.CreateRotationX(Helper.PiOver4),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -PiOverFourNormal, -PiOverFourNormal), Normal = PiOverFourPlaneNormal, Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane below the plane with perfect cap intersection and penetration of 2.",
                            PlaneTransform = Matrix4x4.CreateRotationX(Helper.PiOver4),
                            CylinderTransform =  Matrix4x4.CreateTranslation(Vector3Extensions.Down) * Matrix4x4.CreateRotationX(Helper.PiOver4),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -PiOverFourNormal * 2, -PiOverFourNormal * 2), Normal = PiOverFourPlaneNormal, Penetration = 2 },
                                new (){ Point = new Vector3(0, 0, 0), Normal = PiOverFourPlaneNormal, Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane below the plane with intersection and penetration of 3.",
                            PlaneTransform = Matrix4x4.CreateRotationX(Helper.PiOver4),
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2) * Matrix4x4.CreateRotationX(Helper.PiOver4),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -PiOverFourNormal * 3, -PiOverFourNormal * 3), Normal = PiOverFourPlaneNormal, Penetration = 3 },
                                new (){ Point = new Vector3(0, -PiOverFourNormal, -PiOverFourNormal), Normal = PiOverFourPlaneNormal, Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis aligned cylinder's cap to 45º rotated plane below the plane with intersection and penetration of 6.",
                            PlaneTransform = Matrix4x4.CreateRotationX(Helper.PiOver4),
                            CylinderTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5) * Matrix4x4.CreateRotationX(Helper.PiOver4),
                            IntersectionExpected = true,
                            Contacts =
                            [
                                new (){ Point = new Vector3(0, -PiOverFourNormal * 6, -PiOverFourNormal * 6), Normal = PiOverFourPlaneNormal, Penetration = 6 },
                                new (){ Point = new Vector3(0, -PiOverFourNormal * 4, -PiOverFourNormal * 4), Normal = PiOverFourPlaneNormal, Penetration = 4 },
                            ]
                        }
                    ],
                ];
            }
        }

        [TestMethod()]
        [DynamicData(nameof(HalfSpaceAndCylinderTestData))]
        public void ContactDetectorHalfSpaceAndCylinderTest(HalfSpaceAndCylinderData testData)
        {
            Console.WriteLine(testData.Description);

            var data = new ContactResolver();

            var plane = FromPlane(Vector3.Zero, Vector3Extensions.Up, testData.PlaneTransform);

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

                Assert.IsTrue(Helper.NearEqual(expectedPenetration, contact.Penetration), $"Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3Extensions.NearEqual(expectedPosition, contact.Position, Epsilon), $"Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3Extensions.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
