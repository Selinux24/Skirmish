using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class Matrix4x4Tests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        const float ZeroTolerance = 0.0001f;

        #region Static fields

        [TestMethod]
        public void StaticFields_SizeInBytes_ZeroAndIdentity_WorkAsExpected()
        {
            Assert.AreEqual(64, Matrix4x4.SizeInBytes);
            Assert.AreEqual(0f, Matrix4x4.Zero.M11);
            Assert.AreEqual(1f, Matrix4x4.Identity.M11);
            Assert.AreEqual(1f, Matrix4x4.Identity.M22);
            Assert.AreEqual(1f, Matrix4x4.Identity.M33);
            Assert.AreEqual(1f, Matrix4x4.Identity.M44);
            Assert.IsTrue(Matrix4x4.Identity.IsIdentity);
            Assert.IsFalse(Matrix4x4.Zero.IsIdentity);
        }

        #endregion

        #region Constructors

        [TestMethod]
        public void Constructors_ScalarValue_SetsAllComponents()
        {
            var matrix = new Matrix4x4(5f);
            Assert.AreEqual(5f, matrix.M11);
            Assert.AreEqual(5f, matrix.M44);
        }

        [TestMethod]
        public void Constructors_ComponentValues_SetsAllComponents()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(6f, matrix.M22);
            Assert.AreEqual(11f, matrix.M33);
            Assert.AreEqual(16f, matrix.M44);
        }

        [TestMethod]
        public void Constructors_Default_SetsAllToZero()
        {
            var matrix = new Matrix4x4();
            Assert.AreEqual(0f, matrix.M11);
            Assert.AreEqual(0f, matrix.M44);
        }

        [TestMethod]
        public void Constructors_Array_SetsAllComponents()
        {
            var matrix = new Matrix4x4([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f]);
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(6f, matrix.M22);
            Assert.AreEqual(16f, matrix.M44);
        }

        [TestMethod]
        public void Constructor_Array_Null_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new Matrix4x4(null));
        }

        [TestMethod]
        public void Constructor_Array_WrongLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Matrix4x4([1f, 2f]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Matrix4x4([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f]));
        }

        #endregion

        #region Properties

        [TestMethod]
        public void Properties_Rows_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4(0f);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row1);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row2);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row3);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row4);

            matrix.Row1 = new Vector4(1f, 2f, 3f, 4f);
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(2f, matrix.M12);
            Assert.AreEqual(3f, matrix.M13);
            Assert.AreEqual(4f, matrix.M14);

            matrix.Row2 = new Vector4(5f, 6f, 7f, 8f);
            Assert.AreEqual(5f, matrix.M21);
            Assert.AreEqual(6f, matrix.M22);
            Assert.AreEqual(7f, matrix.M23);
            Assert.AreEqual(8f, matrix.M24);

            matrix.Row3 = new Vector4(9f, 10f, 11f, 12f);
            Assert.AreEqual(9f, matrix.M31);
            Assert.AreEqual(10f, matrix.M32);
            Assert.AreEqual(11f, matrix.M33);
            Assert.AreEqual(12f, matrix.M34);

            matrix.Row4 = new Vector4(13f, 14f, 15f, 16f);
            Assert.AreEqual(13f, matrix.M41);
            Assert.AreEqual(14f, matrix.M42);
            Assert.AreEqual(15f, matrix.M43);
            Assert.AreEqual(16f, matrix.M44);
        }

        [TestMethod]
        public void Properties_Columns_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4(0f);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row1);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row2);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row3);
            Assert.AreEqual(new Vector4(0f, 0f, 0f, 0f), matrix.Row4);

            matrix.Column1 = new Vector4(1f, 2f, 3f, 4f);
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(2f, matrix.M21);
            Assert.AreEqual(3f, matrix.M31);
            Assert.AreEqual(4f, matrix.M41);

            matrix.Column2 = new Vector4(5f, 6f, 7f, 8f);
            Assert.AreEqual(5f, matrix.M12);
            Assert.AreEqual(6f, matrix.M22);
            Assert.AreEqual(7f, matrix.M32);
            Assert.AreEqual(8f, matrix.M42);

            matrix.Column3 = new Vector4(9f, 10f, 11f, 12f);
            Assert.AreEqual(9f, matrix.M13);
            Assert.AreEqual(10f, matrix.M23);
            Assert.AreEqual(11f, matrix.M33);
            Assert.AreEqual(12f, matrix.M43);

            matrix.Column4 = new Vector4(13f, 14f, 15f, 16f);
            Assert.AreEqual(13f, matrix.M14);
            Assert.AreEqual(14f, matrix.M24);
            Assert.AreEqual(15f, matrix.M34);
            Assert.AreEqual(16f, matrix.M44);
        }

        [TestMethod]
        public void Properties_TranslationVector_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4
            {
                TranslationVector = new Vector3(1f, 2f, 3f)
            };
            Assert.AreEqual(1f, matrix.M41);
            Assert.AreEqual(2f, matrix.M42);
            Assert.AreEqual(3f, matrix.M43);
        }

        [TestMethod]
        public void Properties_ScaleVector_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4(2f, 3f, 4f, 0f, 0f, 5f, 0f, 0f, 0f, 0f, 6f, 0f, 0f, 0f, 0f, 1f);
            Assert.AreEqual(new Vector3(2f, 5f, 6f), matrix.ScaleVector);
            matrix.ScaleVector = new Vector3(10f, 20f, 30f);
            Assert.AreEqual(10f, matrix.M11);
            Assert.AreEqual(20f, matrix.M22);
            Assert.AreEqual(30f, matrix.M33);
        }

        [TestMethod]
        public void Properties_DirectionVectors_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4
            {
                Right = new Vector3(1f, 0f, 0f)
            };
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(0f, matrix.M12);
            Assert.AreEqual(0f, matrix.M13);

            matrix.Up = new Vector3(0f, 1f, 0f);
            Assert.AreEqual(0f, matrix.M21);
            Assert.AreEqual(1f, matrix.M22);
            Assert.AreEqual(0f, matrix.M23);

            matrix.Forward = new Vector3(0f, 0f, 1f);
            Assert.AreEqual(0f, matrix.M31);
            Assert.AreEqual(0f, matrix.M32);
            Assert.AreEqual(-1f, matrix.M33);
        }

        #endregion

        #region Indexers

        [TestMethod]
        public void Indexer_SingleIndex_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4(1f);
            Assert.AreEqual(1f, matrix[0]);
            Assert.AreEqual(1f, matrix[15]);
            matrix[0] = 10f;
            Assert.AreEqual(10f, matrix.M11);
            matrix[15] = 20f;
            Assert.AreEqual(20f, matrix.M44);
        }

        [TestMethod]
        public void Indexer_SingleIndex_OutOfRange_Throws()
        {
            var matrix = new Matrix4x4();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[-1]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[16]; });
        }

        [TestMethod]
        public void Indexer_RowColumn_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix4x4(1f);
            Assert.AreEqual(1f, matrix[0, 0]);
            Assert.AreEqual(1f, matrix[3, 3]);
            matrix[0, 3] = 10f;
            Assert.AreEqual(10f, matrix.M14);
            matrix[3, 0] = 20f;
            Assert.AreEqual(20f, matrix.M41);
        }

        [TestMethod]
        public void Indexer_RowColumn_OutOfRange_Throws()
        {
            var matrix = new Matrix4x4();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[-1, 0]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[4, 0]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[0, -1]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[0, 4]; });
        }

        #endregion

        #region ToArray and Determinant

        [TestMethod]
        public void ToArray_ReturnsSixteenElementsInOrder()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var array = matrix.ToArray();
            Assert.HasCount(16, array);
            AssertAreSequenceEqual(array, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f]);
        }

        [TestMethod]
        public void Determinant_CorrectForKnownMatrices()
        {
            Assert.AreEqual(1f, Matrix4x4.Identity.Determinant(), ZeroTolerance);
            Assert.AreEqual(0f, Matrix4x4.Zero.Determinant(), ZeroTolerance);

            var diag = Matrix4x4.Scaling(2f, 3f, 4f) * Matrix4x4.Translation(1f, 2f, 3f);
            Assert.AreEqual(24f, diag.Determinant(), ZeroTolerance);
        }

        #endregion

        #region Invert and Transpose (instance)

        [TestMethod]
        public void Instance_Invert_InvertsMatrix()
        {
            var matrix = Matrix4x4.Scaling(2f, 3f, 4f);
            matrix.Invert();
            var expected = Matrix4x4.Scaling(0.5f, 1f / 3f, 0.25f);
            Assert.IsTrue(Matrix4x4.NearEqual(matrix, expected));
        }

        [TestMethod]
        public void Instance_Transpose_InvertsOffDiagonalElements()
        {
            var matrix = new Matrix4x4(
                1f, 2f, 3f, 4f,
                5f, 6f, 7f, 8f,
                9f, 10f, 11f, 12f,
                13f, 14f, 15f, 16f);
            matrix.Transpose();

            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(6f, matrix.M22);
            Assert.AreEqual(11f, matrix.M33);
            Assert.AreEqual(16f, matrix.M44);

            Assert.AreEqual(2f, matrix.M21);
            Assert.AreEqual(3f, matrix.M31);
            Assert.AreEqual(4f, matrix.M41);

            Assert.AreEqual(5f, matrix.M12);
            Assert.AreEqual(7f, matrix.M32);
            Assert.AreEqual(8f, matrix.M42);

            Assert.AreEqual(9f, matrix.M13);
            Assert.AreEqual(10f, matrix.M23);
            Assert.AreEqual(12f, matrix.M43);

            Assert.AreEqual(13f, matrix.M14);
            Assert.AreEqual(14f, matrix.M24);
            Assert.AreEqual(15f, matrix.M34);
        }

        [TestMethod]
        public void Static_Invert_InvertsIdentity()
        {
            var identity = Matrix4x4.Identity;
            Matrix4x4.Invert(ref identity, out var result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
        }

        [TestMethod]
        public void Static_Invert_SingularMatrix_ReturnsZero()
        {
            var singular = new Matrix4x4(1f, 2f, 3f, 4f, 2f, 4f, 6f, 8f, 3f, 6f, 9f, 12f, 4f, 8f, 12f, 16f);
            Matrix4x4.Invert(ref singular, out var result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Zero));
        }

        [TestMethod]
        public void Static_Invert_ReturnsMatrix()
        {
            var identity = Matrix4x4.Identity;
            var result = Matrix4x4.Invert(identity);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
        }

        [TestMethod]
        public void Static_TransposeRef_TransposesMatrix()
        {
            var matrix = new Matrix4x4(
                1f, 2f, 3f, 4f,
                5f, 6f, 7f, 8f,
                9f, 10f, 11f, 12f,
                13f, 14f, 15f, 16f);
            Matrix4x4.Transpose(ref matrix, out var result);

            Assert.AreEqual(matrix.M11, result.M11);
            Assert.AreEqual(matrix.M22, result.M22);
            Assert.AreEqual(matrix.M33, result.M33);
            Assert.AreEqual(matrix.M44, result.M44);

            Assert.AreEqual(matrix.M12, result.M21);
            Assert.AreEqual(matrix.M13, result.M31);
            Assert.AreEqual(matrix.M14, result.M41);

            Assert.AreEqual(matrix.M21, result.M12);
            Assert.AreEqual(matrix.M23, result.M32);
            Assert.AreEqual(matrix.M24, result.M42);

            Assert.AreEqual(matrix.M31, result.M13);
            Assert.AreEqual(matrix.M32, result.M23);
            Assert.AreEqual(matrix.M34, result.M43);

            Assert.AreEqual(matrix.M41, result.M14);
            Assert.AreEqual(matrix.M42, result.M24);
            Assert.AreEqual(matrix.M43, result.M34);
        }

        [TestMethod]
        public void Static_Transpose_TransposesMatrix()
        {
            var matrix = new Matrix4x4(
                1f, 2f, 3f, 4f,
                5f, 6f, 7f, 8f,
                9f, 10f, 11f, 12f,
                13f, 14f, 15f, 16f);
            var result = Matrix4x4.Transpose(matrix);

            Assert.AreEqual(matrix.M11, result.M11);
            Assert.AreEqual(matrix.M22, result.M22);
            Assert.AreEqual(matrix.M33, result.M33);
            Assert.AreEqual(matrix.M44, result.M44);

            Assert.AreEqual(matrix.M12, result.M21);
            Assert.AreEqual(matrix.M13, result.M31);
            Assert.AreEqual(matrix.M14, result.M41);

            Assert.AreEqual(matrix.M21, result.M12);
            Assert.AreEqual(matrix.M23, result.M32);
            Assert.AreEqual(matrix.M24, result.M42);

            Assert.AreEqual(matrix.M31, result.M13);
            Assert.AreEqual(matrix.M32, result.M23);
            Assert.AreEqual(matrix.M34, result.M43);

            Assert.AreEqual(matrix.M41, result.M14);
            Assert.AreEqual(matrix.M42, result.M24);
            Assert.AreEqual(matrix.M43, result.M34);
        }

        #endregion

        [TestMethod]
        public void Transform_TransformsVector()
        {
            var matrix = Matrix4x4.Scaling(2f, 3f, 4f);
            var result = Vector3.Transform(new Vector3(1f, 1f, 1f), matrix);
            Assert.AreEqual(2f, result.X, ZeroTolerance);
            Assert.AreEqual(3f, result.Y, ZeroTolerance);
            Assert.AreEqual(4f, result.Z, ZeroTolerance);
        }

        #region Orthogonalize and Orthonormalize

        [TestMethod]
        public void Instance_Orthogonalize_MakesRowsOrthogonal()
        {
            var matrix = new Matrix4x4(1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 1f);
            matrix.Orthogonalize();
            var dot = Vector4.Dot(matrix.Row1, matrix.Row2);
            Assert.AreEqual(0f, dot, ZeroTolerance);
        }

        [TestMethod]
        public void OrthogonalizeRef_MakesRowsOrthogonal()
        {
            var matrix = new Matrix4x4(1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 1f);
            Matrix4x4.Orthogonalize(ref matrix, out var result);
            var dot = Vector4.Dot(result.Row1, result.Row2);
            Assert.AreEqual(0f, dot, ZeroTolerance);
        }

        [TestMethod]
        public void Orthogonalize_MakesRowsOrthogonal()
        {
            var matrix = new Matrix4x4(1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 1f);
            var result = Matrix4x4.Orthogonalize(matrix);
            var dot = Vector4.Dot(result.Row1, result.Row2);
            Assert.AreEqual(0f, dot, ZeroTolerance);
        }

        [TestMethod]
        public void Instance_Orthonormalize_MakesRowsOrthonormal()
        {
            var matrix = new Matrix4x4(3f, 0f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 5f, 0f, 0f, 0f, 0f, 1f);
            matrix.Orthonormalize();
            Assert.AreEqual(1f, matrix.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, matrix.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, matrix.Row3.Length(), ZeroTolerance);
            Assert.AreEqual(1f, matrix.Row4.Length(), ZeroTolerance);
        }

        [TestMethod]
        public void OrthonormalizeRef_MakesRowsOrthonormal()
        {
            var matrix = new Matrix4x4(3f, 0f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 5f, 0f, 0f, 0f, 0f, 1f);
            Matrix4x4.Orthonormalize(ref matrix, out var result);
            Assert.AreEqual(1f, result.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row3.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row4.Length(), ZeroTolerance);
        }

        [TestMethod]
        public void Orthonormalize_MakesRowsOrthonormal()
        {
            var matrix = new Matrix4x4(3f, 0f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 0f, 5f, 0f, 0f, 0f, 0f, 1f);
            var result = Matrix4x4.Orthonormalize(matrix);
            Assert.AreEqual(1f, result.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row3.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row4.Length(), ZeroTolerance);
        }

        #endregion

        #region DecomposeQR and DecomposeLQ

        [TestMethod]
        public void DecomposeQR_DecomposesCorrectly()
        {
            var matrix = new Matrix4x4(1f, 1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
            matrix.DecomposeQR(out var Q, out var R);
            Assert.AreEqual(1f, Q.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(0f, R.M21, ZeroTolerance);
            Assert.AreEqual(0f, R.M31, ZeroTolerance);
        }

        [TestMethod]
        public void DecomposeLQ_DecomposesCorrectly()
        {
            var matrix = new Matrix4x4(1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 1f);
            matrix.DecomposeLQ(out var L, out var Q);
            Assert.AreEqual(0f, L.M12, ZeroTolerance);
            Assert.AreEqual(0f, L.M13, ZeroTolerance);
            Assert.AreEqual(1f, Q.Row1.Length(), ZeroTolerance);
        }

        #endregion

        #region Decompose and DecomposeUniformScale

        [TestMethod]
        public void Decompose_ScaleRotationTranslation_ReturnsTrue()
        {
            var matrix = Matrix4x4.Scaling(2f, 3f, 4f) * Matrix4x4.Translation(1f, 2f, 3f);
            var result = matrix.Decompose(out var scale, out _, out var translation);
            Assert.IsTrue(result);
            Assert.AreEqual(2f, scale.X, ZeroTolerance);
            Assert.AreEqual(3f, scale.Y, ZeroTolerance);
            Assert.AreEqual(4f, scale.Z, ZeroTolerance);
            Assert.AreEqual(1f, translation.X, ZeroTolerance);
            Assert.AreEqual(2f, translation.Y, ZeroTolerance);
            Assert.AreEqual(3f, translation.Z, ZeroTolerance);
        }

        [TestMethod]
        public void Decompose_ZeroScale_ReturnsFalse()
        {
            var matrix = new Matrix4x4();
            var result = matrix.Decompose(out _, out var rotation, out _);
            Assert.IsFalse(result);
            Assert.AreEqual(Quaternion.Identity, rotation);
        }

        [TestMethod]
        public void DecomposeUniformScale_ReturnsCorrectScale()
        {
            var matrix = Matrix4x4.Scaling(5f) * Matrix4x4.Translation(1f, 2f, 3f);
            var result = matrix.DecomposeUniformScale(out var scale, out _, out var translation);
            Assert.IsTrue(result);
            Assert.AreEqual(5f, scale, ZeroTolerance);
            Assert.AreEqual(1f, translation.X, ZeroTolerance);
        }

        [TestMethod]
        public void DecomposeUniformScale_ZeroScale_ReturnsFalse()
        {
            var matrix = new Matrix4x4();
            var result = matrix.DecomposeUniformScale(out _, out _, out _);
            Assert.IsFalse(result);
        }

        #endregion

        #region ExchangeRows and ExchangeColumns

        [TestMethod]
        public void ExchangeRows_SwapsRowsCorrectly()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            matrix.ExchangeRows(0, 1);
            Assert.AreEqual(5f, matrix.M11);
            Assert.AreEqual(1f, matrix.M21);
        }

        [TestMethod]
        public void ExchangeRows_SameRow_NoChange()
        {
            var matrix = new Matrix4x4(1f);
            matrix.ExchangeRows(0, 0);
            Assert.AreEqual(1f, matrix.M11);
        }

        [TestMethod]
        public void ExchangeColumns_SwapsColumnsCorrectly()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            matrix.ExchangeColumns(0, 1);
            Assert.AreEqual(2f, matrix.M11);
            Assert.AreEqual(1f, matrix.M12);
        }

        [TestMethod]
        public void ExchangeRows_InvalidIndices_Throws()
        {
            var matrix = new Matrix4x4();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeRows(-1, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeRows(0, 4));
        }

        [TestMethod]
        public void ExchangeColumns_InvalidIndices_Throws()
        {
            var matrix = new Matrix4x4();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeColumns(-1, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeColumns(0, 4));
        }

        #endregion

        #region Static math operations

        [TestMethod]
        public void AddRef_AddsMatrices()
        {
            var left = new Matrix4x4(1f);
            var right = new Matrix4x4(2f);
            Matrix4x4.Add(ref left, ref right, out var result);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void Add_AddsMatrices()
        {
            var left = new Matrix4x4(1f);
            var right = new Matrix4x4(2f);
            var result = Matrix4x4.Add(left, right);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void SubtractRef_SubtractsMatrices()
        {
            var left = new Matrix4x4(5f);
            var right = new Matrix4x4(2f);
            Matrix4x4.Subtract(ref left, ref right, out var result);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void Subtract_SubtractsMatrices()
        {
            var left = new Matrix4x4(5f);
            var right = new Matrix4x4(2f);
            var result = Matrix4x4.Subtract(left, right);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void MultiplyRef_Scalar_ScalesMatrix()
        {
            var matrix = new Matrix4x4(2f);
            Matrix4x4.Multiply(ref matrix, 3f, out var result);
            Assert.AreEqual(6f, result.M11);
        }

        [TestMethod]
        public void Multiply_Scalar_ScalesMatrix()
        {
            var matrix = new Matrix4x4(2f);
            var result = Matrix4x4.Multiply(matrix, 3f);
            Assert.AreEqual(6f, result.M11);
        }

        [TestMethod]
        public void MultiplyRef_Matrix_MultipliesCorrectly()
        {
            var a = Matrix4x4.Identity;
            var b = Matrix4x4.Scaling(2f, 3f, 4f);
            Matrix4x4.Multiply(ref a, ref b, out var result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Multiply_Matrix_MultipliesCorrectly()
        {
            var a = Matrix4x4.Identity;
            var b = Matrix4x4.Scaling(2f, 3f, 4f);
            var result = Matrix4x4.Multiply(a, b);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void DivideRef_Scalar_DividesMatrix()
        {
            var matrix = new Matrix4x4(6f);
            Matrix4x4.Divide(ref matrix, 2f, out var result);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void Divide_Scalar_DividesMatrix()
        {
            var matrix = new Matrix4x4(6f);
            var result = Matrix4x4.Divide(matrix, 2f);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void DivideRef_Matrix_DividesComponentWise()
        {
            var left = new Matrix4x4(6f);
            var right = new Matrix4x4(2f, 3f, 1f, 6f, 2f, 3f, 1f, 6f, 2f, 3f, 1f, 6f, 2f, 3f, 1f, 6f);
            Matrix4x4.Divide(ref left, ref right, out var result);
            Assert.AreEqual(3f, result.M11);
            Assert.AreEqual(2f, result.M12);
        }

        [TestMethod]
        public void Divide_Matrix_DividesComponentWise()
        {
            var left = new Matrix4x4(6f);
            var right = new Matrix4x4(2f, 3f, 1f, 6f, 2f, 3f, 1f, 6f, 2f, 3f, 1f, 6f, 2f, 3f, 1f, 6f);
            var result = Matrix4x4.Divide(left, right);
            Assert.AreEqual(3f, result.M11);
            Assert.AreEqual(2f, result.M12);
        }

        [TestMethod]
        public void NegateRef_NegatesAllComponents()
        {
            var matrix = new Matrix4x4(5f);
            Matrix4x4.Negate(ref matrix, out var result);
            Assert.AreEqual(-5f, result.M11);
        }

        [TestMethod]
        public void Negate_NegatesAllComponents()
        {
            var matrix = new Matrix4x4(5f);
            var result = Matrix4x4.Negate(matrix);
            Assert.AreEqual(-5f, result.M11);
        }

        #endregion

        #region Exponent

        [TestMethod]
        public void Exponent_Zero_ReturnsIdentity()
        {
            var matrix = new Matrix4x4(5f);
            var result = Matrix4x4.Exponent(matrix, 0);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));

            Matrix4x4.Exponent(ref matrix, 0, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
        }

        [TestMethod]
        public void Exponent_One_ReturnsSameMatrix()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var result = Matrix4x4.Exponent(matrix, 1);
            Assert.IsTrue(Matrix4x4.NearEqual(result, matrix));

            Matrix4x4.Exponent(ref matrix, 1, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, matrix));
        }

        [TestMethod]
        public void Exponent_Two_SquaresMatrix()
        {
            var matrix = Matrix4x4.Scaling(3f);
            var result = Matrix4x4.Exponent(matrix, 2);
            Assert.AreEqual(9f, result.M11, ZeroTolerance);
            Assert.AreEqual(9f, result.M22, ZeroTolerance);
            Assert.AreEqual(9f, result.M33, ZeroTolerance);

            Matrix4x4.Exponent(ref matrix, 2, out result);
            Assert.AreEqual(9f, result.M11, ZeroTolerance);
            Assert.AreEqual(9f, result.M22, ZeroTolerance);
            Assert.AreEqual(9f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Exponent_Negative_ThrowsArgumentOutOfRangeException()
        {
            var matrix = new Matrix4x4();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Matrix4x4.Exponent(matrix, -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Matrix4x4.Exponent(ref matrix, -1, out _));
        }

        #endregion

        #region Lerp and SmoothStep

        [TestMethod]
        public void Lerp_InterpolatesCorrectly()
        {
            var start = new Matrix4x4(0f);
            var end = new Matrix4x4(10f);

            var result = Matrix4x4.Lerp(start, end, 0.5f);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);

            Matrix4x4.Lerp(ref start, ref end, 0.5f, out result);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void Lerp_AmountZero_ReturnsStart()
        {
            var start = new Matrix4x4(1f);
            var end = new Matrix4x4(2f);

            var result = Matrix4x4.Lerp(start, end, 0f);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);

            Matrix4x4.Lerp(ref start, ref end, 0f, out result);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void Lerp_AmountOne_ReturnsEnd()
        {
            var start = new Matrix4x4(1f);
            var end = new Matrix4x4(2f);

            var result = Matrix4x4.Lerp(start, end, 1f);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);

            Matrix4x4.Lerp(ref start, ref end, 1f, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void SmoothStep_InterpolatesWithCubicEase()
        {
            var start = new Matrix4x4(0f);
            var end = new Matrix4x4(1f);

            var result = Matrix4x4.SmoothStep(start, end, 0.5f);
            Assert.AreEqual(0.5f, result.M11, ZeroTolerance);

            Matrix4x4.SmoothStep(ref start, ref end, 0.5f, out result);
            Assert.AreEqual(0.5f, result.M11, ZeroTolerance);
        }

        #endregion

        #region Triangular and Echelon Forms

        [TestMethod]
        public void UpperTriangularFormRef_ZerosBelowDiagonal()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            Matrix4x4.UpperTriangularForm(ref matrix, out var result);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);
            Assert.AreEqual(0f, result.M41, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);
            Assert.AreEqual(0f, result.M42, ZeroTolerance);
            Assert.AreEqual(0f, result.M43, ZeroTolerance);
        }

        [TestMethod]
        public void UpperTriangularForm_ZerosBelowDiagonal()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var result = Matrix4x4.UpperTriangularForm(matrix);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);
            Assert.AreEqual(0f, result.M41, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);
            Assert.AreEqual(0f, result.M42, ZeroTolerance);
            Assert.AreEqual(0f, result.M43, ZeroTolerance);
        }

        [TestMethod]
        public void LowerTriangularFormRef_ZerosAboveDiagonal()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            Matrix4x4.LowerTriangularForm(ref matrix, out var result);
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M14, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
            Assert.AreEqual(0f, result.M24, ZeroTolerance);
            Assert.AreEqual(0f, result.M34, ZeroTolerance);
        }

        [TestMethod]
        public void LowerTriangularForm_ZerosAboveDiagonal()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var result = Matrix4x4.LowerTriangularForm(matrix);
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M14, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
            Assert.AreEqual(0f, result.M24, ZeroTolerance);
            Assert.AreEqual(0f, result.M34, ZeroTolerance);
        }

        [TestMethod]
        public void RowEchelonFormRef_ProducesEchelonStructure()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            Matrix4x4.RowEchelonForm(ref matrix, out var result);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void RowEchelonForm_ProducesEchelonStructure()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var result = Matrix4x4.RowEchelonForm(matrix);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void ReducedRowEchelonFormRef_SolvesLinearSystem()
        {
            var matrix = new Matrix4x4(
                1f, 1f, 1f, 0f,
                0f, 1f, 1f, 1f,
                1f, 0f, 1f, 1f,
                1f, 1f, 0f, 1f);
            var augment = new Vector4(3f, 3f, 3f, 3f);
            Matrix4x4.ReducedRowEchelonForm(ref matrix, ref augment, out var result, out var augmentResult);

            Assert.IsTrue(result.IsIdentity);

            // The solution should be [1, 1, 1, 1] (or similar)
            Assert.AreEqual(1f, augmentResult.X, ZeroTolerance);
            Assert.AreEqual(1f, augmentResult.Y, ZeroTolerance);
            Assert.AreEqual(1f, augmentResult.Z, ZeroTolerance);
            Assert.AreEqual(1f, augmentResult.W, ZeroTolerance);
        }

        [TestMethod]
        public void ReducedRowEchelonForm_SolvesLinearSystem()
        {
            var matrix = new Matrix4x4(
                1f, 1f, 1f, 0f,
                0f, 1f, 1f, 1f,
                1f, 0f, 1f, 1f,
                1f, 1f, 0f, 1f);
            var augment = new Vector4(3f, 3f, 3f, 3f);
            Matrix4x4.ReducedRowEchelonForm(matrix, augment, out var result, out var augmentResult);

            Assert.IsTrue(result.IsIdentity);

            // The solution should be [1, 1, 1, 1] (or similar)
            Assert.AreEqual(1f, augmentResult.X, ZeroTolerance);
            Assert.AreEqual(1f, augmentResult.Y, ZeroTolerance);
            Assert.AreEqual(1f, augmentResult.Z, ZeroTolerance);
            Assert.AreEqual(1f, augmentResult.W, ZeroTolerance);
        }

        #endregion

        #region Billboard and LookAt

        [TestMethod]
        public void BillboardLHRef_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            Matrix4x4.BillboardLH(ref objPos, ref camPos, ref camUp, ref camForward, out var result);

            // Right axis (X) = (1, 0, 0)
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);

            // Up axis (Y) = (0, 1, 0)
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(1f, result.M22, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);

            // Forward axis (Z) = (0, 0, 1) — points to camera
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            // Translation = object position
            Assert.AreEqual(0f, result.M41, ZeroTolerance);
            Assert.AreEqual(0f, result.M42, ZeroTolerance);
            Assert.AreEqual(0f, result.M43, ZeroTolerance);

            // Projection column = 0, homogeneity = 1
            Assert.AreEqual(0f, result.M14, ZeroTolerance);
            Assert.AreEqual(0f, result.M24, ZeroTolerance);
            Assert.AreEqual(0f, result.M34, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            // Case 2: displaced object, camera on relative +Z
            objPos = new Vector3(5f, 3f, 2f);
            camPos = new Vector3(5f, 3f, 12f);
            camUp = new Vector3(0f, 1f, 0f);
            camForward = new Vector3(0f, 0f, -1f);
            Matrix4x4.BillboardLH(ref objPos, ref camPos, ref camUp, ref camForward, out result);

            // Identical base axis (same relative direction camera→object)
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(1f, result.M22, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            // Translation = object position
            Assert.AreEqual(5f, result.M41, ZeroTolerance);
            Assert.AreEqual(3f, result.M42, ZeroTolerance);
            Assert.AreEqual(2f, result.M43, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void BillboardLH_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            var result = Matrix4x4.BillboardLH(objPos, camPos, camUp, camForward);

            // Right axis (X) = (1, 0, 0)
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);

            // Up axis (Y) = (0, 1, 0)
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(1f, result.M22, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);

            // Forward axis (Z) = (0, 0, 1) — points to camera
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            // Translation = object position
            Assert.AreEqual(0f, result.M41, ZeroTolerance);
            Assert.AreEqual(0f, result.M42, ZeroTolerance);
            Assert.AreEqual(0f, result.M43, ZeroTolerance);

            // Projection column = 0, homogeneity = 1
            Assert.AreEqual(0f, result.M14, ZeroTolerance);
            Assert.AreEqual(0f, result.M24, ZeroTolerance);
            Assert.AreEqual(0f, result.M34, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            // Case 2: displaced object, camera on relative +Z
            objPos = new Vector3(5f, 3f, 2f);
            camPos = new Vector3(5f, 3f, 12f);
            camUp = new Vector3(0f, 1f, 0f);
            camForward = new Vector3(0f, 0f, -1f);
            result = Matrix4x4.BillboardLH(objPos, camPos, camUp, camForward);

            // Identical base axis (same relative direction camera→object)
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(1f, result.M22, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            // Translation = object position
            Assert.AreEqual(5f, result.M41, ZeroTolerance);
            Assert.AreEqual(3f, result.M42, ZeroTolerance);
            Assert.AreEqual(2f, result.M43, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void BillboardRHRef_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            Matrix4x4.BillboardRH(ref objPos, ref camPos, ref camUp, ref camForward, out var result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void BillboardRH_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            var result = Matrix4x4.BillboardRH(objPos, camPos, camUp, camForward);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void LookAtLHRef_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            Matrix4x4.LookAtLH(ref eye, ref target, ref up, out var result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void LookAtLH_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            var result = Matrix4x4.LookAtLH(eye, target, up);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void LookAtRHRef_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            Matrix4x4.LookAtRH(ref eye, ref target, ref up, out var result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void LookAtRH_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            var result = Matrix4x4.LookAtRH(eye, target, up);
            Assert.IsFalse(result.IsIdentity);
        }

        #endregion

        #region Orthographic and Perspective projections

        [TestMethod]
        public void OrthoLH_CreatesOrthographicMatrix()
        {
            var result = Matrix4x4.OrthoLH(800f, 600f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.OrthoLH(800f, 600f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void OrthoRH_CreatesOrthographicMatrix()
        {
            var result = Matrix4x4.OrthoRH(800f, 600f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.OrthoRH(800f, 600f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void OrthoOffCenterLH_CreatesCustomOrthographicMatrix()
        {
            var result = Matrix4x4.OrthoOffCenterLH(-400f, 400f, -300f, 300f, 0.1f, 100f);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.OrthoOffCenterLH(-400f, 400f, -300f, 300f, 0.1f, 100f, out result);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void OrthoOffCenterRH_CreatesCustomOrthographicMatrix()
        {
            var result = Matrix4x4.OrthoOffCenterRH(-400f, 400f, -300f, 300f, 0.1f, 100f);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.OrthoOffCenterRH(-400f, 400f, -300f, 300f, 0.1f, 100f, out result);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void PerspectiveLH_CreatesPerspectiveMatrix()
        {
            var result = Matrix4x4.PerspectiveLH(800f, 600f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);

            Matrix4x4.PerspectiveLH(800f, 600f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void PerspectiveRH_CreatesPerspectiveMatrix()
        {
            var result = Matrix4x4.PerspectiveRH(800f, 600f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);

            Matrix4x4.PerspectiveRH(800f, 600f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void PerspectiveFovLH_CreatesPerspectiveMatrix()
        {
            var result = Matrix4x4.PerspectiveFovLH(MathF.PI / 4f, 4f / 3f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M34, ZeroTolerance);

            Matrix4x4.PerspectiveFovLH(MathF.PI / 4f, 4f / 3f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M34, ZeroTolerance);
        }

        [TestMethod]
        public void PerspectiveFovRH_CreatesPerspectiveMatrix()
        {
            var result = Matrix4x4.PerspectiveFovRH(MathF.PI / 4f, 4f / 3f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(-1f, result.M34, ZeroTolerance);

            Matrix4x4.PerspectiveFovRH(MathF.PI / 4f, 4f / 3f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(-1f, result.M34, ZeroTolerance);
        }

        [TestMethod]
        public void PerspectiveOffCenterLH_CreatesPerspectiveMatrix()
        {
            var result = Matrix4x4.PerspectiveOffCenterLH(-400f, 400f, -300f, 300f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);

            Matrix4x4.PerspectiveOffCenterLH(-400f, 400f, -300f, 300f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void PerspectiveOffCenterRH_CreatesPerspectiveMatrix()
        {
            var result = Matrix4x4.PerspectiveOffCenterRH(-400f, 400f, -300f, 300f, 0.1f, 100f);
            Assert.IsFalse(result.IsIdentity);

            Matrix4x4.PerspectiveOffCenterRH(-400f, 400f, -300f, 300f, 0.1f, 100f, out result);
            Assert.IsFalse(result.IsIdentity);
        }

        #endregion

        #region Scaling and Rotation factories

        [TestMethod]
        public void Scaling_Vector_ScalesCorrectly()
        {
            var scaling = new Vector3(2f, 3f, 4f);
            var result = Matrix4x4.Scaling(scaling);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.Scaling(ref scaling, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void Scaling_Components_ScalesCorrectly()
        {
            var scaling = new Vector3(2f, 3f, 4f);
            var result = Matrix4x4.Scaling(scaling);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);

            Matrix4x4.Scaling(ref scaling, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Scaling_3Components_ScalesCorrectly()
        {
            var result = Matrix4x4.Scaling(2f, 3f, 4f);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);

            Matrix4x4.Scaling(2f, 3f, 4f, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Scaling_Uniform_ScalesAllDiagonal()
        {
            var result = Matrix4x4.Scaling(5f);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);
            Assert.AreEqual(5f, result.M22, ZeroTolerance);
            Assert.AreEqual(5f, result.M33, ZeroTolerance);

            Matrix4x4.Scaling(5f, out result);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);
            Assert.AreEqual(5f, result.M22, ZeroTolerance);
            Assert.AreEqual(5f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void RotationX_CorrectMatrix()
        {
            var result = Matrix4x4.RotationX(0f);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
            Matrix4x4.RotationX(0f, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
           
            var cos = MathF.Cos(MathF.PI / 2);
            var sin = MathF.Sin(MathF.PI / 2);

            var half = Matrix4x4.RotationX(MathF.PI / 2);
            Assert.AreEqual(cos, half.M22, ZeroTolerance);
            Assert.AreEqual(sin, half.M23, ZeroTolerance);
            Assert.AreEqual(-sin, half.M32, ZeroTolerance);
            Assert.AreEqual(cos, half.M33, ZeroTolerance);

            Matrix4x4.RotationX(MathF.PI / 2, out half);
            Assert.AreEqual(cos, half.M22, ZeroTolerance);
            Assert.AreEqual(sin, half.M23, ZeroTolerance);
            Assert.AreEqual(-sin, half.M32, ZeroTolerance);
            Assert.AreEqual(cos, half.M33, ZeroTolerance);
        }

        [TestMethod]
        public void RotationY_CorrectMatrix()
        {
            var result = Matrix4x4.RotationY(0f);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
            Matrix4x4.RotationY(0f, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));

            var half = Matrix4x4.RotationY(MathF.PI / 2);
            Assert.AreEqual(0f, half.M11, ZeroTolerance);
            Matrix4x4.RotationY(MathF.PI / 2, out half);
            Assert.AreEqual(0f, half.M11, ZeroTolerance);
        }

        [TestMethod]
        public void RotationZ_CorrectMatrix()
        {
            var result = Matrix4x4.RotationZ(0f);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
            Matrix4x4.RotationZ(0f, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));

            var half = Matrix4x4.RotationZ(MathF.PI / 2);
            Assert.AreEqual(0f, half.M11, ZeroTolerance);
            Matrix4x4.RotationZ(MathF.PI / 2, out half);
            Assert.AreEqual(0f, half.M11, ZeroTolerance);
        }

        [TestMethod]
        public void RotationAxis_RotatesAroundGivenAxis()
        {
            var axis = new Vector3(0f, 1f, 0f);
            var result = Matrix4x4.RotationAxis(axis, MathF.PI / 2);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.RotationAxis(ref axis, MathF.PI / 2, out result);
            Assert.IsFalse(result.IsIdentity);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void RotationQuaternion_FromIdentityQuaternion_ReturnsIdentity()
        {
            var q = Quaternion.Identity;
            var result = Matrix4x4.RotationQuaternion(q);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));

            Matrix4x4.RotationQuaternion(ref q, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
        }

        [TestMethod]
        public void RotationYawPitchRoll_CreatesRotationMatrix()
        {
            var result = Matrix4x4.RotationYawPitchRoll(0f, 0f, 0f);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));

            Matrix4x4.RotationYawPitchRoll(0f, 0f, 0f, out result);
            Assert.IsTrue(Matrix4x4.NearEqual(result, Matrix4x4.Identity));
        }

        #endregion

        #region Translation and World

        [TestMethod]
        public void Translation_CreatesTranslationMatrix()
        {
            var result = Matrix4x4.Translation(1f, 2f, 3f);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.Translation(1f, 2f, 3f, out result);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void Translation_Vector_CreatesTranslationMatrix()
        {
            var translation = new Vector3(1f, 2f, 3f);

            var result = Matrix4x4.Translation(translation);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);

            Matrix4x4.Translation(ref translation, out result);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);
        }

        [TestMethod]
        public void CreateWorld_CreatesWorldMatrix()
        {
            var position = new Vector3(1f, 2f, 3f);
            var forward = new Vector3(0f, 0f, -1f);
            var up = new Vector3(0f, 1f, 0f);

            var result = Matrix4x4.CreateWorld(position, forward, up);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
            Assert.AreEqual(1f, result.TranslationVector.X, ZeroTolerance);
            Assert.AreEqual(2f, result.TranslationVector.Y, ZeroTolerance);
            Assert.AreEqual(3f, result.TranslationVector.Z, ZeroTolerance);

            Matrix4x4.CreateWorld(ref position, ref forward, ref up, out result);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
            Assert.AreEqual(1f, result.TranslationVector.X, ZeroTolerance);
            Assert.AreEqual(2f, result.TranslationVector.Y, ZeroTolerance);
            Assert.AreEqual(3f, result.TranslationVector.Z, ZeroTolerance);
        }

        #endregion

        #region Affine and Transformation

        [TestMethod]
        public void AffineTransformation_CreatesTransformationMatrix()
        {
            var q = Quaternion.Identity;
            var trans = new Vector3(1f, 2f, 3f);

            var result = Matrix4x4.AffineTransformation(1f, q, trans);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.AffineTransformation(1f, ref q, ref trans, out result);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void AffineTransformation_WithRotationCenter_CreatesTransformationMatrix()
        {
            var center = new Vector3(0f, 0f, 0f);
            var rot = Quaternion.Identity;
            var trans = new Vector3(1f, 2f, 3f);

            var result = Matrix4x4.AffineTransformation(1f, center, rot, trans);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);

            Matrix4x4.AffineTransformation(1f, ref center, ref rot, ref trans, out result);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(2f, result.M42, ZeroTolerance);
            Assert.AreEqual(3f, result.M43, ZeroTolerance);
        }

        [TestMethod]
        public void AffineTransformation2D_Creates2DTransformationMatrix()
        {
            var trans = new Vector2(1f, 2f);

            var result = Matrix4x4.AffineTransformation2D(1f, 0f, trans);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            Matrix4x4.AffineTransformation2D(1f, 0f, ref trans, out result);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void AffineTransformation2D_WithRotationCenter_Creates2DTransformationMatrix()
        {
            var center = new Vector2(0f, 0f);
            var trans = new Vector2(1f, 2f);

            var result = Matrix4x4.AffineTransformation2D(1f, center, 0f, trans);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);

            Matrix4x4.AffineTransformation2D(1f, ref center, 0f, ref trans, out result);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
        }

        [TestMethod]
        public void Transformation_CreatesTransformationMatrix()
        {
            var trans = new Vector3(1f, 2f, 3f);
            var rot = Quaternion.Identity;
            var scaling = new Vector3(1f, 1f, 1f);

            var result = Matrix4x4.Transformation(trans, rot, scaling);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
            Assert.AreEqual(1f, result.M22, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            Matrix4x4.Transformation(ref trans, ref rot, ref scaling, out result);
            Assert.AreEqual(1f, result.M41, ZeroTolerance);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
            Assert.AreEqual(1f, result.M22, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            var scalingCenter = new Vector3(0f, 0f, 0f);
            var rotCencer = new Vector3(0f, 0f, 0f);

            var result2 = Matrix4x4.Transformation(scalingCenter, rot, scaling, rotCencer, rot, trans);
            Assert.IsTrue(Matrix4x4.NearEqual(result, result2));

            Matrix4x4.Transformation(ref scalingCenter, ref rot, ref scaling, ref rotCencer, ref rot, ref trans, out result2);
            Assert.IsTrue(Matrix4x4.NearEqual(result, result2));
        }

        [TestMethod]
        public void Transformation2D_Creates2DTransformationMatrix()
        {
            var transCenter = new Vector2(0f, 0f);
            var scaling = new Vector2(1f, 1f);
            var rotCencer = new Vector2(0f, 0f);
            var trans = new Vector2(1f, 2f);

            var result = Matrix4x4.Transformation2D(transCenter, 0f, scaling, rotCencer, 0f, trans);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);

            Matrix4x4.Transformation2D(ref transCenter, 0f, ref scaling, ref rotCencer, 0f, ref trans, out result);
            Assert.AreEqual(1f, result.M44, ZeroTolerance);
            Assert.AreEqual(1f, result.M33, ZeroTolerance);
        }

        #endregion

        #region Skew

        [TestMethod]
        public void Skew_CreatesSkewMatrix()
        {
            var rot = new Vector3(1f, 0f, 0f);
            var tans = new Vector3(0f, 1f, 0f);

            var result = Matrix4x4.Skew(0.1f, rot, tans);
            Assert.IsFalse(result.IsIdentity);

            Matrix4x4.Skew(0.1f, ref rot, ref tans, out result);
            Assert.IsFalse(result.IsIdentity);
        }

        #endregion

        #region Operators

        [TestMethod]
        public void Operators_Addition_Subtraction_Multiplication_Division_Work()
        {
            var left = new Matrix4x4(2f);
            var right = new Matrix4x4(1f);

            Assert.AreEqual(new Matrix4x4(3f), left + right);
            Assert.AreEqual(new Matrix4x4(1f), left - right);
            Assert.AreEqual(new Matrix4x4(8f), left * right);
            Assert.AreEqual(new Matrix4x4(2f), left / right);
            Assert.AreEqual(new Matrix4x4(6f), left * 3f);
            Assert.AreEqual(new Matrix4x4(6f), 3f * left);
            Assert.AreEqual(new Matrix4x4(2f / 3f), left / 3f);
        }

        [TestMethod]
        public void Operators_UnaryPlusAndNegation_Work()
        {
            var matrix = new Matrix4x4(5f);
            Assert.AreEqual(new Matrix4x4(5f), +matrix);
            Assert.AreEqual(new Matrix4x4(-5f), -matrix);
        }

        [TestMethod]
        public void Operators_EqualityAndInequality_Work()
        {
            var a = new Matrix4x4(1f);
            var b = new Matrix4x4(1f);
            var c = new Matrix4x4(2f);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);
        }

        #endregion

        #region Casts

        [TestMethod]
        public void Cast_Matrix3x3ToMatrix4x4_SetsCorrectValues()
        {
            Matrix3x3 m3 = new(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            Matrix4x4 m4 = (Matrix4x4)m3;
            Assert.AreEqual(1f, m4.M11);
            Assert.AreEqual(5f, m4.M22);
            Assert.AreEqual(9f, m4.M33);
            Assert.AreEqual(0f, m4.M14);
            Assert.AreEqual(0f, m4.M41);
            Assert.AreEqual(1f, m4.M44);
        }

        [TestMethod]
        public void Cast_Matrix4x4ToMatrix3x3_Extracts3x3()
        {
            Matrix4x4 m4 = new(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            Matrix3x3 m3 = (Matrix3x3)m4;
            Assert.AreEqual(1f, m3.M11);
            Assert.AreEqual(6f, m3.M22);
            Assert.AreEqual(11f, m3.M33);
        }

        [TestMethod]
        public void Cast_SystemNumericsMatrix4x4_ToAndFrom()
        {
            var systemMatrix = new System.Numerics.Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var engineMatrix = (Matrix4x4)systemMatrix;
            Assert.AreEqual(1f, engineMatrix.M11);
            Assert.AreEqual(6f, engineMatrix.M22);
            Assert.AreEqual(11f, engineMatrix.M33);
            Assert.AreEqual(16f, engineMatrix.M44);

            var backToSystem = (System.Numerics.Matrix4x4)engineMatrix;
            Assert.AreEqual(1f, backToSystem.M11);
            Assert.AreEqual(6f, backToSystem.M22);
            Assert.AreEqual(11f, backToSystem.M33);
            Assert.AreEqual(16f, backToSystem.M44);
        }

        #endregion

        #region ToString

        [TestMethod]
        public void ToString_Default_ReturnsFormattedString()
        {
            var matrix = new Matrix4x4(
                1f, 2f, 3f, 4f,
                5f, 6f, 7f, 8f,
                9f, 10f, 11f, 12f,
                13f, 14f, 15f, 16f);
            var str = matrix.ToString();

            Assert.Contains($"M11:{01f}", str);
            Assert.Contains($"M12:{02f}", str);
            Assert.Contains($"M13:{03f}", str);
            Assert.Contains($"M14:{04f}", str);
            Assert.Contains($"M21:{05f}", str);
            Assert.Contains($"M22:{06f}", str);
            Assert.Contains($"M23:{07f}", str);
            Assert.Contains($"M24:{08f}", str);
            Assert.Contains($"M31:{09f}", str);
            Assert.Contains($"M32:{10f}", str);
            Assert.Contains($"M33:{11f}", str);
            Assert.Contains($"M34:{12f}", str);
            Assert.Contains($"M41:{13f}", str);
            Assert.Contains($"M42:{14f}", str);
            Assert.Contains($"M43:{15f}", str);
            Assert.Contains($"M44:{16f}", str);
        }

        [TestMethod]
        public void ToString_WithFormat_ReturnsFormattedString()
        {
            var matrix = new Matrix4x4(
                01.5f, 02.5f, 03.5f, 04.5f,
                05.5f, 06.5f, 07.5f, 08.5f,
                09.5f, 10.5f, 11.5f, 12.5f,
                13.5f, 14.5f, 15.5f, 16.5f);
            var str = matrix.ToString("F1");

            Assert.Contains($"M11:{01.5f:F1}", str);
            Assert.Contains($"M12:{02.5f:F1}", str);
            Assert.Contains($"M13:{03.5f:F1}", str);
            Assert.Contains($"M14:{04.5f:F1}", str);
            Assert.Contains($"M21:{05.5f:F1}", str);
            Assert.Contains($"M22:{06.5f:F1}", str);
            Assert.Contains($"M23:{07.5f:F1}", str);
            Assert.Contains($"M24:{08.5f:F1}", str);
            Assert.Contains($"M31:{09.5f:F1}", str);
            Assert.Contains($"M32:{10.5f:F1}", str);
            Assert.Contains($"M33:{11.5f:F1}", str);
            Assert.Contains($"M34:{12.5f:F1}", str);
            Assert.Contains($"M41:{13.5f:F1}", str);
            Assert.Contains($"M42:{14.5f:F1}", str);
            Assert.Contains($"M43:{15.5f:F1}", str);
            Assert.Contains($"M44:{16.5f:F1}", str);
        }

        [TestMethod]
        public void ToString_WithFormatProvider_ReturnsFormattedString()
        {
            var matrix = new Matrix4x4(1.5f);
            var str = matrix.ToString(System.Globalization.CultureInfo.CurrentCulture);

            Assert.Contains($"M11:{1.5f}", str);
            Assert.Contains($"M12:{1.5f}", str);
            Assert.Contains($"M13:{1.5f}", str);
            Assert.Contains($"M14:{1.5f}", str);
            Assert.Contains($"M21:{1.5f}", str);
            Assert.Contains($"M22:{1.5f}", str);
            Assert.Contains($"M23:{1.5f}", str);
            Assert.Contains($"M24:{1.5f}", str);
            Assert.Contains($"M31:{1.5f}", str);
            Assert.Contains($"M32:{1.5f}", str);
            Assert.Contains($"M33:{1.5f}", str);
            Assert.Contains($"M34:{1.5f}", str);
            Assert.Contains($"M41:{1.5f}", str);
            Assert.Contains($"M42:{1.5f}", str);
            Assert.Contains($"M43:{1.5f}", str);
            Assert.Contains($"M44:{1.5f}", str);
        }

        [TestMethod]
        public void ToString_FormatAndProvider_ReturnsFormattedString()
        {
            var matrix = new Matrix4x4(1.5f);
            var str = matrix.ToString("F1", System.Globalization.CultureInfo.CurrentCulture);

            Assert.Contains($"M11:{1.5f:F1}", str);
            Assert.Contains($"M12:{1.5f:F1}", str);
            Assert.Contains($"M13:{1.5f:F1}", str);
            Assert.Contains($"M14:{1.5f:F1}", str);
            Assert.Contains($"M21:{1.5f:F1}", str);
            Assert.Contains($"M22:{1.5f:F1}", str);
            Assert.Contains($"M23:{1.5f:F1}", str);
            Assert.Contains($"M24:{1.5f:F1}", str);
            Assert.Contains($"M31:{1.5f:F1}", str);
            Assert.Contains($"M32:{1.5f:F1}", str);
            Assert.Contains($"M33:{1.5f:F1}", str);
            Assert.Contains($"M34:{1.5f:F1}", str);
            Assert.Contains($"M41:{1.5f:F1}", str);
            Assert.Contains($"M42:{1.5f:F1}", str);
            Assert.Contains($"M43:{1.5f:F1}", str);
            Assert.Contains($"M44:{1.5f:F1}", str);
        }

        #endregion

        #region GetHashCode, Equals and NearEqual

        [TestMethod]
        public void GetHashCode_ReturnsConsistentHash()
        {
            var matrix = new Matrix4x4(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f, 13f, 14f, 15f, 16f);
            var hash1 = matrix.GetHashCode();
            var hash2 = matrix.GetHashCode();
            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void Equals_EqualMatrices_ReturnsTrue()
        {
            var a = new Matrix4x4(1f);
            var b = new Matrix4x4(1f);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals(ref b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [TestMethod]
        public void Equals_DifferentMatrices_ReturnsFalse()
        {
            var a = new Matrix4x4(1f);
            var b = new Matrix4x4(2f);
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod]
        public void Equals_NonMatrixObject_ReturnsFalse()
        {
            var matrix = new Matrix4x4();
            Assert.IsFalse(matrix.Equals("not a matrix"));
            Assert.IsFalse(matrix.Equals(42));
            Assert.IsFalse(matrix.Equals(null));
        }

        [TestMethod]
        public void NearEqual_NearMatrices_ReturnsTrue()
        {
            var a = new Matrix4x4(
                01f, 02f, 03f, 04f,
                05f, 06f, 07f, 08f,
                09f, 10f, 11f, 12f,
                13f, 14f, 15f, 16f);

            var b = new Matrix4x4(
                01.00001f, 02.00001f, 03.00001f, 04.00001f,
                05.00001f, 06.00001f, 07.00001f, 08.00001f,
                09.00001f, 10.00001f, 11.00001f, 12.00001f,
                13.00001f, 14.00001f, 15.00001f, 16.00001f);

            Assert.IsTrue(Matrix4x4.NearEqual(a, b, ZeroTolerance));
        }

        [TestMethod]
        public void NearEqual_FarMatrices_ReturnsFalse()
        {
            var a = new Matrix4x4(1f);
            var b = new Matrix4x4(2f);
            Assert.IsFalse(Matrix4x4.NearEqual(a, b, ZeroTolerance));
        }

        #endregion

        #region GetDescription

        [TestMethod]
        public void GetDescription_ReturnsDescriptionForValidMatrix()
        {
            var matrix = Matrix4x4.Scaling(2f, 3f, 4f) * Matrix4x4.Translation(1f, 2f, 3f);
            var desc = matrix.GetDescription();
            Assert.IsFalse(string.IsNullOrEmpty(desc));
            Assert.DoesNotContain("Bad matrix", desc);
        }

        [TestMethod]
        public void GetDescription_ReturnsBadMatrixForZeroMatrix()
        {
            var desc = Matrix4x4.Zero.GetDescription();
            Assert.AreEqual("Bad matrix", desc);
        }

        #endregion

        #region Helper

        private static void AssertAreSequenceEqual(float[] actual, float[] expected)
        {
            Assert.HasCount(expected.Length, actual);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], ZeroTolerance);
            }
        }

        #endregion
    }
}
