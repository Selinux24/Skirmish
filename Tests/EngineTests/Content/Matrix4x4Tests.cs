using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.Content.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class Matrix4x4Tests
    {
        static TestContext _testContext;

        static readonly float[] matrixValues = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

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
        public void MatrixZeroTest()
        {
            var res = Matrix4X4.Zero;
            var expected = new Matrix4X4(0);

            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixOneTest()
        {
            var res = Matrix4X4.Identity;
            var expected = new Matrix4X4 { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };

            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixValueTest()
        {
            var res = new Matrix4X4(1);
            var expected = new Matrix4X4
            {
                M11 = 1.0f,
                M12 = 1.0f,
                M13 = 1.0f,
                M14 = 1.0f,

                M21 = 1.0f,
                M22 = 1.0f,
                M23 = 1.0f,
                M24 = 1.0f,

                M31 = 1.0f,
                M32 = 1.0f,
                M33 = 1.0f,
                M34 = 1.0f,

                M41 = 1.0f,
                M42 = 1.0f,
                M43 = 1.0f,
                M44 = 1.0f
            };

            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixArrayTest()
        {
            var res = new Matrix4X4(matrixValues);
            var expected = new Matrix4X4
            {
                M11 = 1.0f,
                M12 = 2.0f,
                M13 = 3.0f,
                M14 = 4.0f,

                M21 = 5.0f,
                M22 = 6.0f,
                M23 = 7.0f,
                M24 = 8.0f,

                M31 = 9.0f,
                M32 = 10.0f,
                M33 = 11.0f,
                M34 = 12.0f,

                M41 = 13.0f,
                M42 = 14.0f,
                M43 = 15.0f,
                M44 = 16.0f
            };

            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixBadArrayTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Matrix4X4(null));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Matrix4X4(Array.Empty<float>()));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Matrix4X4(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }));
        }

        [TestMethod()]
        public void MatrixEqualsTest()
        {
            var res = Matrix4X4.Zero == new Matrix4X4(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void MatrixDistinctTest()
        {
            var res = Matrix4X4.Zero != new Matrix4X4(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void MatrixMultiplyTest()
        {
            Matrix4X4 res = new Matrix4X4(1) * new Matrix4X4(2);
            Matrix4X4 expected = new Matrix(1) * new Matrix(2);
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixScalingTest()
        {
            Matrix4X4 res = Matrix4X4.Scaling(new Scale3(2));
            Matrix4X4 expected = Matrix.Scaling(2);
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixRotationTest()
        {
            Matrix4X4 res = Matrix4X4.Rotation(RotationQ.RotationAxis(Direction3.Up, 10));
            Matrix4X4 expected = Matrix.RotationQuaternion(Quaternion.RotationAxis(Vector3.Up, 10));
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixTranslationTest()
        {
            Matrix4X4 res = Matrix4X4.Translation(new Position3(2));
            Matrix4X4 expected = Matrix.Translation(2, 2, 2);
            Assert.AreEqual(expected, res);
        }

        [TestMethod()]
        public void MatrixToMatrixTest()
        {
            Matrix res1 = new Matrix4X4(matrixValues);
            Matrix expected1 = new Matrix(matrixValues);
            Assert.AreEqual(expected1, res1);

            Matrix4X4 res2 = new Matrix(matrixValues);
            Matrix4X4 expected2 = new Matrix4X4(matrixValues);
            Assert.AreEqual(expected2, res2);
        }
    }
}