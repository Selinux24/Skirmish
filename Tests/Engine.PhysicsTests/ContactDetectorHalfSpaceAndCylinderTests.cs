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
            public Matrix PlaneTransform { get; set; } = Matrix.Identity;
            public Matrix CylinderTransform { get; set; } = Matrix.Identity;
            public bool IntersectionExpected { get; set; } = false;
            public HalfSpaceAndCylinderContactData Contact { get; set; }

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
        private static readonly Vector3 Normal45 = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.PiOverFour));
        private static readonly Vector3 Normal135 = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.PiOverFour + MathUtil.PiOverTwo));
        private static readonly Vector3 Normal10 = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(10)));
        private static readonly Vector3 Normal10m = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(-10)));
        private static readonly Vector3 Normal100 = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(100)));
        private static readonly Vector3 Normal100m = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(-100)));
        private static readonly Vector3 Normal190 = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(190)));
        private static readonly Vector3 Normal190m = Vector3.TransformNormal(Vector3.Up, Matrix.RotationAxis(Vector3.Left, MathUtil.DegreesToRadians(-190)));

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
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 0 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 1 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with perfect cap intersection and penetration of 2.",
                            CylinderTransform = Matrix.Translation(Vector3.Down),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 2 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 3 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Axis Aligned Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.Translation(Vector3.Down * 5f),
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
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 1), Normal = Vector3.BackwardLH, Penetration = 0f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 1), Normal = Vector3.BackwardLH, Penetration = 1f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Vector3.BackwardLH, Penetration = 2f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 1), Normal = Vector3.BackwardLH, Penetration = 3f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, -1, 1), Normal = Vector3.BackwardLH, Penetration = 6f },
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
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Vector3.ForwardLH, Penetration = 0f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Vector3.ForwardLH, Penetration = 1f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with perfect side intersection and penetration of 2.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Vector3.ForwardLH, Penetration = 2f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, -1), Normal = Vector3.ForwardLH, Penetration = 3f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-90º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.RotationX(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, -1), Normal = Vector3.ForwardLH, Penetration = 6f },
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
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 0), Normal = Vector3.Down, Penetration = 0f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder over the plane with intersection and penetration of 1.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 0), Normal = Vector3.Down, Penetration = 1f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with perfect base intersection and penetration of 2.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 1f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 0), Normal = Vector3.Down, Penetration = 2f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with intersection and penetration of 3.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 0), Normal = Vector3.Down, Penetration = 3f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "180º rotated Cylinder below the plane with intersection and penetration of 6.",
                            CylinderTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0, 1, 0), Normal = Vector3.Down, Penetration = 6f },
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
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal45, Penetration = 0f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder over the plane with intersection and penetration of {1 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal45, Penetration = 1 + PiOverFourDisplacement },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with perfect side intersection and penetration of {(1 + PiOverFourDisplacement) * 2f}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Down * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal45, Penetration = (1 + PiOverFourDisplacement) * 2f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with intersection and penetration of {3 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal45, Penetration = 3 + PiOverFourDisplacement },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"45º rotated Cylinder below the plane with intersection and penetration of {6 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal45, Penetration = 6 + PiOverFourDisplacement },
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
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal135, Penetration = 0f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder over the plane with intersection and penetration of {1 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Zero),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal135, Penetration = 1 + PiOverFourDisplacement },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with perfect side intersection and penetration of {(1 + PiOverFourDisplacement) * 2f}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * (1 + PiOverFourDisplacement)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal135, Penetration = (1 + PiOverFourDisplacement) * 2f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with intersection and penetration of {3 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal135, Penetration = 3 + PiOverFourDisplacement },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = $"135º rotated Cylinder below the plane with intersection and penetration of {6 + PiOverFourDisplacement}.",
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal135, Penetration = 6 + PiOverFourDisplacement },
                        }
                    },
             
                    // Cylinder's cap parallel to 45º rotated plane - To test when both rigid body has transforms
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder's cap parallel to 45º rotated plane over the plane with no intersection.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Up * -PiOverFourDisplacement),
                            CylinderTransform = Matrix.RotationX(MathUtil.PiOverFour) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectionExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder's cap parallel to 45º rotated plane over the plane with perfect base intersection.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Up) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 0 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder's cap parallel to 45º rotated plane over the plane with intersection and penetration of 1.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Zero) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 1 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder's cap parallel to 45º rotated plane below the plane with perfect cap intersection and penetration of 2.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform =  Matrix.Translation(Vector3.Down) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 2 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder's cap parallel to 45º rotated plane below the plane with intersection and penetration of 3.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Down * 2) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 3 },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder's cap parallel to 45º rotated plane below the plane with intersection and penetration of 6.",
                            PlaneTransform = Matrix.RotationX(MathUtil.PiOverFour),
                            CylinderTransform = Matrix.Translation(Vector3.Down * 5) * Matrix.RotationX(MathUtil.PiOverFour),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = Vector3.Down, Normal = Vector3.Up, Penetration = 6 },
                        }
                    },

                    // 10º Maximum penetration test
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "10º rotated Cylinder with plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(10)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal10, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-10º rotated Cylinder with plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(-10)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, -1), Normal = Normal10m, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder with -10º rotated plane cut.",
                            PlaneTransform = Matrix.RotationX(MathUtil.DegreesToRadians(-10)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, 1), Normal = Normal10, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder with 10º rotated plane cut.",
                            PlaneTransform = Matrix.RotationX(MathUtil.DegreesToRadians(10)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, -1, -1), Normal = Normal10m, Penetration = 1.158456f },
                        }
                    },

                    // 100º Maximum penetration test
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "100º rotated Cylinder with plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(100)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal100, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-100º rotated Cylinder with plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(-100)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Normal100m, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder with -100º rotated plane cut.",
                            PlaneTransform = Matrix.RotationX(MathUtil.DegreesToRadians(-100)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal100, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder with 100º rotated plane cut.",
                            PlaneTransform = Matrix.RotationX(MathUtil.DegreesToRadians(100)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Normal100m, Penetration = 1.158456f },
                        }
                    },

                    // 190º Maximum penetration test
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "190º rotated Cylinder with plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(190)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Normal190, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "-190º rotated Cylinder with plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(-190)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal190m, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder with -190º rotated plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(-190)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, 1), Normal = Normal190m, Penetration = 1.158456f },
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndCylinderData
                        {
                            Description = "Cylinder with 190º rotated plane cut.",
                            CylinderTransform = Matrix.RotationX(MathUtil.DegreesToRadians(190)),
                            IntersectionExpected = true,
                            Contact = new HalfSpaceAndCylinderContactData{ Point = new Vector3(0f, 1, -1), Normal = Normal190, Penetration = 1.158456f },
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
