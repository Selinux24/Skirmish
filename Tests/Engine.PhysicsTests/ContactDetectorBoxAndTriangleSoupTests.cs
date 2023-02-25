using Engine.Physics;
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
    public class ContactDetectorBoxAndTriangleSoupTests
    {
        static TestContext _testContext;

        static readonly Vector3 Epsilon = new Vector3(MathUtil.ZeroTolerance);

        static CollisionBox FromAABB(Vector3 extents, Matrix transform)
        {
            CollisionBox box = new CollisionBox(extents);
            RigidBody boxBody = new RigidBody(1, transform);
            box.Attach(boxBody);

            return box;
        }

        static CollisionTriangleSoup FromRectangle(RectangleF rect, float d, Matrix transform)
        {
            Vector3 p1 = new Vector3(rect.BottomLeft.X, d, rect.BottomLeft.Y);
            Vector3 p2 = new Vector3(rect.BottomRight.X, d, rect.BottomRight.Y);
            Vector3 p3 = new Vector3(rect.TopLeft.X, d, rect.TopLeft.Y);
            Vector3 p4 = new Vector3(rect.TopRight.X, d, rect.TopRight.Y);

            Triangle tri1 = new Triangle(p1, p2, p3);
            Triangle tri2 = new Triangle(p3, p2, p4);

            CollisionTriangleSoup ctri = new CollisionTriangleSoup(new[] { tri1, tri2 });
            RigidBody triBody = new RigidBody(2, transform);
            ctri.Attach(triBody);

            return ctri;
        }
        static CollisionPlane FromPlane(Vector3 normal, float d, Matrix transform)
        {
            Plane p = new Plane(normal, d);
            CollisionPlane plane = new CollisionPlane(p);
            RigidBody planeBody = new RigidBody(float.PositiveInfinity, transform);
            plane.Attach(planeBody);

            return plane;
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

        public class BoxAndTriangleSoupData
        {
            public string CaseName { get; set; }
            public Matrix BoxTransform { get; set; }
            public bool IntersectioExpected { get; set; }
            public BoxAndTriangleSoupContactData[] Contacts { get; set; }
            public int ContactCount { get; set; }
        }

        public class BoxAndTriangleSoupContactData
        {
            public BoxCorners Corner { get; set; }
            public float Penetration { get; set; }
        }

        public static IEnumerable<object[]> BoxAndTriangleSoupTestData
        {
            get
            {
                return new[]
                {
                    //Un-rotated box - Bottom face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Over the plane",
                            BoxTransform = Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Bottom face perfect contact",
                            BoxTransform = Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Bottom face 0.1 penetration",
                            BoxTransform = Matrix.Translation(Vector3.Up * 0.9f),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0.1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Plane cuts in the middle",
                            BoxTransform = Matrix.Identity,
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Top face perfect contact. Bottom face below the plane",
                            BoxTransform = Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Below the plane",
                            BoxTransform = Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = false
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Un-rotated box - Bottom face to plane. Far below the plane",
                            BoxTransform = Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = false
                        }
                    },

                    //X-axis 90º - Front face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Over the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Front face perfect contact",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Front face 0.1 penetration",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Plane cuts in the middle",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Back face perfect contact. Front face below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = false
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Front face to plane. Far below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = false
                        }
                    },

                    //X-axis 180º - Top face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 180º - Top face to plane. Over the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 180º - Top face to plane. Top face perfect contact",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 5,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop  , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 90º - Top face to plane. Top face 0.1 penetration",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Up * 0.9f),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop, Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop  , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop , Penetration = 0.1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 180º - Top face to plane. Plane cuts in the middle",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop  , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 180º - Top face to plane. Bottom face perfect contact. Top face below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 180º - Top face to plane. Below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = false
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 180º - Top face to plane. Far below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = false
                        }
                    },

                    //X-axis 270º - Back face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Over the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Back face perfect contact",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Back face 0.1 penetration",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 0.9f),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop   , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom, Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 0.1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Plane cuts in the middle",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Front face perfect contact. Back face below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = false
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "X-axis 270º - Back face to plane. Far below the plane",
                            BoxTransform = Matrix.RotationX(MathUtil.Pi + MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = false
                        }
                    },

                    //Z-axis 90º - Left face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Over the plane",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Left face perfect contact",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom, Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Left face 0.1 penetration",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 0.9f),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop   , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom, Penetration = 0.1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Plane cuts in the middle",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom, Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Right face perfect contact. Left face below the plane",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftTop     , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackLeftBottom  , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontLeftBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Below the plane",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = false
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis 90º - Left face to plane. Far below the plane",
                            BoxTransform = Matrix.RotationZ(MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = false
                        }
                    },

                    //Z-axis -90º - Right face to plane
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Over the plane",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 5f),
                            IntersectioExpected = false,
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Right face perfect contact",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Right face 0.1 penetration",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Up * 0.9f),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 0.1f },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 0.1f },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Plane cuts in the middle",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo),
                            IntersectioExpected = true,
                            ContactCount = 6,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 1 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 1 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Left face perfect contact. Right face below the plane",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down),
                            IntersectioExpected = true,
                            ContactCount = 7,
                            Contacts = new BoxAndTriangleSoupContactData[]
                            {
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightTop   , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightTop    , Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.FrontRightBottom, Penetration = 2 },
                                new BoxAndTriangleSoupContactData{ Corner = BoxCorners.BackRightBottom , Penetration = 2 },
                            }
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Below the plane",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 2f),
                            IntersectioExpected = false
                        }
                    },
                    new object[]
                    {
                        new BoxAndTriangleSoupData
                        {
                            CaseName = "Z-axis -90º - Right face to plane. Far below the plane",
                            BoxTransform = Matrix.RotationZ(-MathUtil.PiOverTwo) * Matrix.Translation(Vector3.Down * 5f),
                            IntersectioExpected = false
                        }
                    },
                };
            }
        }

        [TestMethod()]
        [DynamicData(nameof(BoxAndTriangleSoupTestData))]
        public void ContactDetectorBoxAndTriangleSoupTest(BoxAndTriangleSoupData testData)
        {
            Console.WriteLine(testData.CaseName);

            ContactResolver data = new ContactResolver();
            ContactResolver data1 = new ContactResolver();

            var soup = FromRectangle(new RectangleF(-100f, -100f, 200f, 200f), 0, Matrix.Identity);
            var plane = FromPlane(Vector3.Up, 0, Matrix.Identity);

            var box = FromAABB(Vector3.One, testData.BoxTransform);

            bool intersection = ContactDetector.BoxAndTriangleSoup(box, soup, data);
            bool intersection2 = ContactDetector.BoxAndHalfSpace(box, plane, data1);

            Assert.AreEqual(testData.IntersectioExpected, intersection, testData.IntersectioExpected ? "Intersection expected" : "No intersection expected");
            Assert.AreEqual(testData.ContactCount, data.ContactCount, $"{testData.ContactCount} contacts expected");

            if (!intersection)
            {
                return;
            }

            var contacts = data.GetContacts();
            for (int i = 0; i < contacts.Count(); i++)
            {
                var contact = contacts.ElementAt(i);

                var expectedContact = testData.Contacts.FirstOrDefault(c => box.OrientedBoundingBox.GetVertex(c.Corner) == contact.Position);
                if (expectedContact == null)
                {
                    continue;
                }

                var expectedPenetration = expectedContact.Penetration;
                var expectedPosition = box.OrientedBoundingBox.GetVertex(expectedContact.Corner);
                var expectedNormal = Vector3.Up;

                Assert.IsTrue(MathUtil.NearEqual(expectedPenetration, contact.Penetration), $"Contact {i}. Expected penetration {expectedPenetration} != {contact.Penetration}");
                Assert.IsTrue(Vector3.NearEqual(expectedPosition, contact.Position, Epsilon), $"Contact {i}. Expected position {expectedPosition} != {contact.Position}");
                Assert.IsTrue(Vector3.NearEqual(expectedNormal, contact.Normal, Epsilon), $"Contact {i}. Expected normal {expectedNormal} != {contact.Normal}");
            }
        }
    }
}
