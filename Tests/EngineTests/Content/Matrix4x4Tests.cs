using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.Content
{
    using Engine.Content;
    using Engine.Mathematics;

    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class Matrix4x4Tests
    {
        public TestContext TestContext { get; set; }

        static readonly float[] matrixValues = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public void MatrixZeroTest()
        {
            var res = Engine.Content.Matrix4x4.Zero;
            var expected = new Engine.Content.Matrix4x4(0);

            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixOneTest()
        {
            var res = Engine.Content.Matrix4x4.Identity;
            var expected = new Engine.Content.Matrix4x4 { M11 = 1.0f, M22 = 1.0f, M33 = 1.0f, M44 = 1.0f };

            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixValueTest()
        {
            var res = new Engine.Content.Matrix4x4(1);
            var expected = new Engine.Content.Matrix4x4
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
            var res = new Engine.Content.Matrix4x4(matrixValues);
            var expected = new Engine.Content.Matrix4x4
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
            Assert.Throws<ArgumentNullException>(() => new Engine.Content.Matrix4x4(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Engine.Content.Matrix4x4([]));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Engine.Content.Matrix4x4([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17]));
        }

        [TestMethod()]
        public void MatrixEqualsTest()
        {
            var res = Engine.Content.Matrix4x4.Zero == new Engine.Content.Matrix4x4(0);

            Assert.IsTrue(res);
        }
        [TestMethod()]
        public void MatrixDistinctTest()
        {
            var res = Engine.Content.Matrix4x4.Zero != new Engine.Content.Matrix4x4(1);

            Assert.IsTrue(res);
        }

        [TestMethod()]
        public void MatrixMultiplyTest()
        {
            Engine.Content.Matrix4x4 res = new Engine.Content.Matrix4x4(1) * new Engine.Content.Matrix4x4(2);
            Engine.Content.Matrix4x4 expected = new Engine.Content.Matrix4x4(1) * new Engine.Content.Matrix4x4(2);
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixScalingTest()
        {
            Engine.Content.Matrix4x4 res = Engine.Content.Matrix4x4.Scaling(new Scale3(2));
            Engine.Content.Matrix4x4 expected = Engine.Mathematics.Matrix4x4.Scaling(2);
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixRotationTest()
        {
            Engine.Content.Matrix4x4 res = Engine.Content.Matrix4x4.Rotation(RotationQ.RotationAxis(Direction3.Up, 10));
            Engine.Content.Matrix4x4 expected = Engine.Mathematics.Matrix4x4.RotationQuaternion(Quaternion.RotationAxis(Vector3Extensions.Up, 10));
            Assert.AreEqual(expected, res);
        }
        [TestMethod()]
        public void MatrixTranslationTest()
        {
            Engine.Content.Matrix4x4 res = Engine.Content.Matrix4x4.Translation(new Position3(2));
            Engine.Content.Matrix4x4 expected = Engine.Mathematics.Matrix4x4.Translation(2, 2, 2);
            Assert.AreEqual(expected, res);
        }
    }
}