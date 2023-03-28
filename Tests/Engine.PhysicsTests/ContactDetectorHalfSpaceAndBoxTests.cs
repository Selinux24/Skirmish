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
    public class ContactDetectorHalfSpaceAndBoxTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new Vector3(MathUtil.ZeroTolerance);

        static HalfSpaceCollider FromPlane(Vector3 normal, float d, Matrix transform)
        {
            Plane p = new Plane(normal, d);
            HalfSpaceCollider plane = new HalfSpaceCollider(p);
            RigidBody planeBody = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            plane.Attach(planeBody);

            return plane;
        }

        static BoxCollider FromAABB(Vector3 extents, Matrix transform)
        {
            BoxCollider box = new BoxCollider(extents);
            RigidBody boxBody = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            box.Attach(boxBody);

            return box;
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

        public struct HalfSpaceAndBoxData
        {
            public Matrix BoxTransform { get; set; }
            public bool IntersectioExpected { get; set; }
            public HalfSpaceAndBoxContactData[] Contacts { get; set; }
            public int ContactCount
            {
                get { return Contacts?.Length ?? 0; }
            }
        }

        public struct HalfSpaceAndBoxContactData
        {
            public BoxVertices Corner { get; set; }
            public float Penetration { get; set; }
        }

        public static IEnumerable<object[]> HalfSpaceAndBoxTestData
        {
            get
            {
                return new[]
                {
                    //Un-rotated box - Bottom face to plane
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.Identity,
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 3 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 6 },
                            }
                        }
                    },

                    //X-axis 90º - Front face to plane
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 3 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 6 },
                            }
                        }
                    },

                    //X-axis 180º - Top face to plane
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop  , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop  , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 4 },
                            }
                        }
                    },

                    //X-axis 270º - Back face to plane
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 4 },
                            }
                        }
                    },

                    //Z-axis 90º - Left face to plane
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 3 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 6 },
                            }
                        }
                    },

                    //Z-axis -90º - Right face to plane
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 2 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 3 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = true,
                            Contacts = new HalfSpaceAndBoxContactData[]
                            {
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightTop   , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightTop    , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftTop     , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftTop    , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontRightBottom, Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackRightBottom , Penetration = 6 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.BackLeftBottom  , Penetration = 4 },
                                new HalfSpaceAndBoxContactData{ Corner = BoxVertices.FrontLeftBottom , Penetration = 4 },
                            }
                        }
                    },
                };
            }
        }

        [TestMethod()]
        [DynamicData(nameof(HalfSpaceAndBoxTestData))]
        public void ContactDetectorHalfSpaceAndBoxTest(HalfSpaceAndBoxData testData)
        {
            ContactResolver data = new ContactResolver();

            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);
            var box = FromAABB(Vector3.One, testData.BoxTransform);

            bool intersection = ContactDetector.BetweenObjects(box, plane, data);

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
                var expectedPosition = box.OrientedBoundingBox.GetVertex(expectedContact.Corner);
                var expectedNormal = plane.Normal;

                Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Contact {i}. Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Contact {i}. Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Contact {i}. Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
