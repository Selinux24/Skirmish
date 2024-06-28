
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;

namespace Engine.Content.FmtColladaTests
{
    /// <summary>
    /// Assert matrix
    /// </summary>
    public sealed class AssertMatrix
    {

        private static AssertMatrix that;

        public static AssertMatrix That
        {
            get
            {
                that ??= new();

                return that;
            }
        }

        private AssertMatrix()
        {

        }

        public static void AreEqual(Matrix m1, Matrix m2)
        {
            AreEqual(m1, m2, 0, null, null);
        }
        public static void AreEqual(Matrix m1, Matrix m2, float tolerance)
        {
            AreEqual(m1, m2, tolerance, null, null);
        }
        public static void AreEqual(Matrix m1, Matrix m2, float tolerance, string message)
        {
            AreEqual(m1, m2, tolerance, message, null);
        }
        public static void AreEqual(Matrix m1, Matrix m2, string message)
        {
            AreEqual(m1, m2, 0, message, null);
        }
        public static void AreEqual(Matrix m1, Matrix m2, string message, params object[] parameters)
        {
            AreEqual(m1, m2, 0, message, parameters);
        }
        public static void AreEqual(Matrix m1, Matrix m2, float tolerance, string message, params object[] parameters)
        {
            Assert.AreEqual(m1.M11, m2.M11, tolerance, message, parameters);
            Assert.AreEqual(m1.M12, m2.M12, tolerance, message, parameters);
            Assert.AreEqual(m1.M13, m2.M13, tolerance, message, parameters);
            Assert.AreEqual(m1.M14, m2.M14, tolerance, message, parameters);

            Assert.AreEqual(m1.M21, m2.M21, tolerance, message, parameters);
            Assert.AreEqual(m1.M22, m2.M22, tolerance, message, parameters);
            Assert.AreEqual(m1.M23, m2.M23, tolerance, message, parameters);
            Assert.AreEqual(m1.M24, m2.M24, tolerance, message, parameters);

            Assert.AreEqual(m1.M31, m2.M31, tolerance, message, parameters);
            Assert.AreEqual(m1.M32, m2.M32, tolerance, message, parameters);
            Assert.AreEqual(m1.M33, m2.M33, tolerance, message, parameters);
            Assert.AreEqual(m1.M34, m2.M34, tolerance, message, parameters);

            Assert.AreEqual(m1.M41, m2.M41, tolerance, message, parameters);
            Assert.AreEqual(m1.M42, m2.M42, tolerance, message, parameters);
            Assert.AreEqual(m1.M43, m2.M43, tolerance, message, parameters);
            Assert.AreEqual(m1.M44, m2.M44, tolerance, message, parameters);
        }
    }
}
