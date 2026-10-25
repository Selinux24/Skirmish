using Engine.Physics;
using Engine.Physics.Colliders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Engine.PhysicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ContactDetectorHalfSpaceAndBoxTests
    {
        public TestContext TestContext { get; set; }

        static readonly Vector3 Epsilon = new(Helper.ZeroTolerance);

        static HalfSpaceCollider FromPlane(Vector3 normal, float d, Matrix4x4 transform)
        {
            var p = new Plane(normal, d);
            var plane = new HalfSpaceCollider(p);
            var planeBody = new RigidBody(new() { Mass = float.PositiveInfinity, InitialTransform = transform });
            plane.Attach(planeBody);

            return plane;
        }

        static BoxCollider FromAABB(Vector3 extents, Matrix4x4 transform)
        {
            var box = new BoxCollider(extents);
            var boxBody = new RigidBody(new() { Mass = 1, InitialTransform = transform });
            box.Attach(boxBody);

            return box;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        public struct HalfSpaceAndBoxData
        {
            public Matrix4x4 BoxTransform { get; set; }
            public bool IntersectioExpected { get; set; }
            public HalfSpaceAndBoxContactData[] Contacts { get; set; }
            public readonly int ContactCount
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
                return
                [
                    //Un-rotated box - Bottom face to plane
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectioExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.Identity,
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 2 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 2 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 3 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 3 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 6 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 6 },
                            ]
                        }
                    ],

                    //X-axis 90º - Front face to plane
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectioExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.PiOver2),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 2 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 2 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 3 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 3 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 6 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 6 },
                            ]
                        }
                    ],

                    //X-axis 180º - Top face to plane
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectioExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightTop , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftTop  , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftTop , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightTop , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftTop  , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftTop , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 4 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 4 },
                            ]
                        }
                    ],

                    //X-axis 270º - Back face to plane
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectioExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.BackRightTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi + Helper.PiOver2),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.BackRightTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationX(Helper.Pi + Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 4 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 4 },
                            ]
                        }
                    ],

                    //Z-axis 90º - Left face to plane
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectioExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.BackLeftTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftBottom , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftBottom, Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(Helper.PiOver2),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.BackLeftTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftBottom , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftBottom, Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 2 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 2 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 3 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 3 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 4 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 4 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 6 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 6 },
                            ]
                        }
                    ],

                    //Z-axis -90º - Right face to plane
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up * 5f),
                            IntersectioExpected = false,
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Up),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 0 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(-Helper.PiOver2),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 1 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 2 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 2 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 0 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 0 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 2f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 3 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 3 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 1 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 1 },
                            ]
                        }
                    ],
                    [
                        new HalfSpaceAndBoxData
                        {
                            BoxTransform = Matrix4x4.CreateRotationZ(-Helper.PiOver2) * Matrix4x4.CreateTranslation(Vector3Extensions.Down * 5f),
                            IntersectioExpected = true,
                            Contacts =
                            [
                                new (){ Corner = BoxVertices.FrontRightTop   , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackRightTop    , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackLeftTop     , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontLeftTop    , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontRightBottom, Penetration = 6 },
                                new (){ Corner = BoxVertices.BackRightBottom , Penetration = 6 },
                                new (){ Corner = BoxVertices.BackLeftBottom  , Penetration = 4 },
                                new (){ Corner = BoxVertices.FrontLeftBottom , Penetration = 4 },
                            ]
                        }
                    ],
                ];
            }
        }

        [TestMethod()]
        [DynamicData(nameof(HalfSpaceAndBoxTestData))]
        public void ContactDetectorHalfSpaceAndBoxTest(HalfSpaceAndBoxData testData)
        {
            var data = new ContactResolver();

            var plane = FromPlane(Vector3Extensions.Up, 0, Matrix4x4.Identity);
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

                Assert.IsTrue(Helper.NearEqual(expectedPenetration, contact.Penetration), $"Contact {i}. Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3Extensions.NearEqual(expectedPosition, contact.Position, Epsilon), $"Contact {i}. Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3Extensions.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Contact {i}. Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
