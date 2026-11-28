using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Engine.MathematicsTests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class Matrix3x3Tests
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
            Assert.AreEqual(36, Matrix3x3.SizeInBytes);
            Assert.AreEqual(0f, Matrix3x3.Zero.M11);
            Assert.AreEqual(1f, Matrix3x3.Identity.M11);
            Assert.AreEqual(1f, Matrix3x3.Identity.M22);
            Assert.AreEqual(1f, Matrix3x3.Identity.M33);
            Assert.IsTrue(Matrix3x3.Identity.IsIdentity);
            Assert.IsFalse(Matrix3x3.Zero.IsIdentity);
        }

        #endregion

        #region Constructors

        [TestMethod]
        public void Constructors_ScalarValue_SetsAllComponents()
        {
            var matrix = new Matrix3x3(5f);
            Assert.AreEqual(5f, matrix.M11);
            Assert.AreEqual(5f, matrix.M22);
            Assert.AreEqual(5f, matrix.M33);
        }

        [TestMethod]
        public void Constructors_ComponentValues_SetsAllComponents()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(2f, matrix.M12);
            Assert.AreEqual(3f, matrix.M13);
            Assert.AreEqual(4f, matrix.M21);
            Assert.AreEqual(5f, matrix.M22);
            Assert.AreEqual(6f, matrix.M23);
            Assert.AreEqual(7f, matrix.M31);
            Assert.AreEqual(8f, matrix.M32);
            Assert.AreEqual(9f, matrix.M33);
        }

        [TestMethod]
        public void Constructors_Default_SetsAllToZero()
        {
            var matrix = new Matrix3x3();
            Assert.AreEqual(0f, matrix.M11);
            Assert.AreEqual(0f, matrix.M33);
        }

        [TestMethod]
        public void Constructors_Array_SetsAllComponents()
        {
            var matrix = new Matrix3x3([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(5f, matrix.M22);
            Assert.AreEqual(9f, matrix.M33);
        }

        [TestMethod]
        public void Constructor_Array_Null_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new Matrix3x3(null));
        }

        [TestMethod]
        public void Constructor_Array_WrongLength_ThrowsArgumentOutOfRangeException()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Matrix3x3([1f, 2f]));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Matrix3x3([1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f]));
        }

        #endregion

        #region Properties

        [TestMethod]
        public void Properties_Rows_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix3x3(1f);
            Assert.AreEqual(new Vector3(1f, 1f, 1f), matrix.Row1);
            Assert.AreEqual(new Vector3(1f, 1f, 1f), matrix.Row2);
            Assert.AreEqual(new Vector3(1f, 1f, 1f), matrix.Row3);

            matrix.Row1 = new Vector3(10f, 20f, 30f);
            Assert.AreEqual(10f, matrix.M11);
            Assert.AreEqual(20f, matrix.M12);
            Assert.AreEqual(30f, matrix.M13);

            matrix.Row2 = new Vector3(40f, 50f, 60f);
            Assert.AreEqual(40f, matrix.M21);
            Assert.AreEqual(50f, matrix.M22);
            Assert.AreEqual(60f, matrix.M23);

            matrix.Row3 = new Vector3(70f, 80f, 90f);
            Assert.AreEqual(70f, matrix.M31);
            Assert.AreEqual(80f, matrix.M32);
            Assert.AreEqual(90f, matrix.M33);
        }

        [TestMethod]
        public void Properties_Columns_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix3x3
            {
                Column1 = new Vector3(1f, 2f, 3f)
            };
            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(2f, matrix.M21);
            Assert.AreEqual(3f, matrix.M31);

            matrix.Column2 = new Vector3(4f, 5f, 6f);
            Assert.AreEqual(4f, matrix.M12);
            Assert.AreEqual(5f, matrix.M22);
            Assert.AreEqual(6f, matrix.M32);

            matrix.Column3 = new Vector3(7f, 8f, 9f);
            Assert.AreEqual(7f, matrix.M13);
            Assert.AreEqual(8f, matrix.M23);
            Assert.AreEqual(9f, matrix.M33);
        }

        [TestMethod]
        public void Properties_ScaleVector_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix3x3(2f, 3f, 4f, 0f, 5f, 0f, 0f, 0f, 6f);
            Assert.AreEqual(new Vector3(2f, 5f, 6f), matrix.ScaleVector);

            matrix.ScaleVector = new Vector3(10f, 20f, 30f);
            Assert.AreEqual(10f, matrix.M11);
            Assert.AreEqual(20f, matrix.M22);
            Assert.AreEqual(30f, matrix.M33);
        }

        #endregion

        #region Indexers

        [TestMethod]
        public void Indexer_SingleIndex_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix3x3(1f);
            Assert.AreEqual(1f, matrix[0]);
            Assert.AreEqual(1f, matrix[4]);
            Assert.AreEqual(1f, matrix[8]);

            matrix[0] = 10f;
            Assert.AreEqual(10f, matrix.M11);

            matrix[8] = 30f;
            Assert.AreEqual(30f, matrix.M33);
        }

        [TestMethod]
        public void Indexer_SingleIndex_OutOfRange_Throws()
        {
            var matrix = new Matrix3x3();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[-1]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[9]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { matrix[-1] = 1f; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { matrix[9] = 1f; });
        }

        [TestMethod]
        public void Indexer_RowColumn_GetAndSet_WorkAsExpected()
        {
            var matrix = new Matrix3x3(1f);
            Assert.AreEqual(1f, matrix[0, 0]);
            Assert.AreEqual(1f, matrix[1, 1]);
            Assert.AreEqual(1f, matrix[2, 2]);

            matrix[0, 1] = 10f;
            Assert.AreEqual(10f, matrix.M12);

            matrix[2, 2] = 30f;
            Assert.AreEqual(30f, matrix.M33);
        }

        [TestMethod]
        public void Indexer_RowColumn_OutOfRange_Throws()
        {
            var matrix = new Matrix3x3();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[-1, 0]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[0, -1]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[3, 0]; });
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => { _ = matrix[0, 3]; });
        }

        #endregion

        #region ToArray and Determinant

        [TestMethod]
        public void ToArray_ReturnsNineElementsInOrder()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            var array = matrix.ToArray();
            AssertAreSequenceEqual(array, [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f]);
        }

        [TestMethod]
        public void Determinant_CorrectForKnownMatrices()
        {
            Assert.AreEqual(1f, Matrix3x3.Identity.Determinant(), ZeroTolerance);
            Assert.AreEqual(0f, Matrix3x3.Zero.Determinant(), ZeroTolerance);

            // Determinant of [1,2,3; 4,5,6; 7,8,9] = 0
            var singular = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            Assert.AreEqual(0f, singular.Determinant(), ZeroTolerance);

            // Determinant of [1,0,0; 0,2,0; 0,0,3] = 6
            var diag = Matrix3x3.Scaling(1f, 2f, 3f);
            Assert.AreEqual(6f, diag.Determinant(), ZeroTolerance);
        }

        #endregion

        #region Invert and Transpose (instance)

        [TestMethod]
        public void Instance_Invert_InvertsMatrix()
        {
            var matrix = Matrix3x3.Scaling(2f, 3f, 4f);
            matrix.Invert();
            var expected = Matrix3x3.Scaling(0.5f, 1f / 3f, 0.25f);
            Assert.IsTrue(Matrix3x3.NearEqual(matrix, expected));
        }

        [TestMethod]
        public void Instance_Transpose_InvertsOffDiagonalElements()
        {
            var matrix = new Matrix3x3(
                1f, 2f, 3f,
                4f, 5f, 6f,
                7f, 8f, 9f);
            matrix.Transpose();

            Assert.AreEqual(1f, matrix.M11);
            Assert.AreEqual(5f, matrix.M22);
            Assert.AreEqual(9f, matrix.M33);

            Assert.AreEqual(4f, matrix.M12);
            Assert.AreEqual(7f, matrix.M13);

            Assert.AreEqual(2f, matrix.M21);
            Assert.AreEqual(8f, matrix.M23);

            Assert.AreEqual(3f, matrix.M31);
            Assert.AreEqual(6f, matrix.M32);
        }

        [TestMethod]
        public void Static_Invert_InvertsIdentity()
        {
            var identity = Matrix3x3.Identity;
            Matrix3x3.Invert(ref identity, out var result);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));
        }

        [TestMethod]
        public void Static_Invert_SingularMatrix_ReturnsZero()
        {
            var singular = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            Matrix3x3.Invert(ref singular, out var result);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Zero));
        }

        [TestMethod]
        public void Static_Invert_ReturnsMatrix()
        {
            var identity = Matrix3x3.Identity;
            var result = Matrix3x3.Invert(identity);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));
        }

        [TestMethod]
        public void Static_TransposeRef_TransposesMatrix()
        {
            var matrix = new Matrix3x3(
                1f, 2f, 3f,
                4f, 5f, 6f,
                7f, 8f, 9f);
            Matrix3x3.Transpose(ref matrix, out var result);

            Assert.AreEqual(matrix.M11, result.M11);
            Assert.AreEqual(matrix.M22, result.M22);
            Assert.AreEqual(matrix.M33, result.M33);

            Assert.AreEqual(matrix.M12, result.M21);
            Assert.AreEqual(matrix.M13, result.M31);

            Assert.AreEqual(matrix.M21, result.M12);
            Assert.AreEqual(matrix.M23, result.M32);

            Assert.AreEqual(matrix.M31, result.M13);
            Assert.AreEqual(matrix.M32, result.M23);
        }

        [TestMethod]
        public void Static_Transpose_TransposesMatrix()
        {
            var matrix = new Matrix3x3(
                1f, 2f, 3f,
                4f, 5f, 6f,
                7f, 8f, 9f);
            var result = Matrix3x3.Transpose(matrix);

            Assert.AreEqual(matrix.M11, result.M11);
            Assert.AreEqual(matrix.M22, result.M22);
            Assert.AreEqual(matrix.M33, result.M33);

            Assert.AreEqual(matrix.M12, result.M21);
            Assert.AreEqual(matrix.M13, result.M31);

            Assert.AreEqual(matrix.M21, result.M12);
            Assert.AreEqual(matrix.M23, result.M32);

            Assert.AreEqual(matrix.M31, result.M13);
            Assert.AreEqual(matrix.M32, result.M23);
        }

        #endregion

        #region Orthogonalize and Orthonormalize

        [TestMethod]
        public void Instance_Orthogonalize_MakesRowsOrthogonal()
        {
            var matrix = new Matrix3x3(1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f);
            matrix.Orthogonalize();
            var dot = Vector3.Dot(matrix.Row1, matrix.Row2);
            Assert.AreEqual(0f, dot, ZeroTolerance);
        }

        [TestMethod]
        public void OrthogonalizeRef_MakesRowsOrthogonal()
        {
            var matrix = new Matrix3x3(1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f);
            var result = Matrix3x3.Orthogonalize(matrix);
            // Row1 and Row2 should be orthogonal
            var dot = Vector3.Dot(result.Row1, result.Row2);
            Assert.AreEqual(0f, dot, ZeroTolerance);
        }

        [TestMethod]
        public void Orthogonalize_MakesRowsOrthogonal()
        {
            var matrix = new Matrix3x3(1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f);
            Matrix3x3.Orthogonalize(ref matrix, out var result);
            // Row1 and Row2 should be orthogonal
            var dot = Vector3.Dot(result.Row1, result.Row2);
            Assert.AreEqual(0f, dot, ZeroTolerance);
        }

        [TestMethod]
        public void Instance_Orthonormalize_MakesRowsOrthonormal()
        {
            var matrix = new Matrix3x3(3f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 5f);
            matrix.Orthonormalize();
            Assert.AreEqual(1f, matrix.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, matrix.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, matrix.Row3.Length(), ZeroTolerance);
        }

        [TestMethod]
        public void OrthonormalizeRef_MakesRowsOrthonormal()
        {
            var matrix = new Matrix3x3(3f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 5f);
            Matrix3x3.Orthonormalize(ref matrix, out var result);
            Assert.AreEqual(1f, result.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row3.Length(), ZeroTolerance);
        }

        [TestMethod]
        public void Orthonormalize_MakesRowsOrthonormal()
        {
            var matrix = new Matrix3x3(3f, 0f, 0f, 0f, 4f, 0f, 0f, 0f, 5f);
            var result = Matrix3x3.Orthonormalize(matrix);
            Assert.AreEqual(1f, result.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, result.Row3.Length(), ZeroTolerance);
        }

        #endregion

        #region DecomposeQR and DecomposeLQ

        [TestMethod]
        public void DecomposeQR_DecomposesCorrectly()
        {
            var matrix = new Matrix3x3(1f, 1f, 0f, 0f, 1f, 1f, 0f, 0f, 1f);
            matrix.DecomposeQR(out var Q, out var R);
            // Q should be orthonormal
            Assert.AreEqual(1f, Q.Row1.Length(), ZeroTolerance);
            Assert.AreEqual(1f, Q.Row2.Length(), ZeroTolerance);
            Assert.AreEqual(1f, Q.Row3.Length(), ZeroTolerance);
            // R should be upper triangular
            Assert.AreEqual(0f, R.M21, ZeroTolerance);
            Assert.AreEqual(0f, R.M31, ZeroTolerance);
            Assert.AreEqual(0f, R.M32, ZeroTolerance);
        }

        [TestMethod]
        public void DecomposeLQ_DecomposesCorrectly()
        {
            var matrix = new Matrix3x3(1f, 0f, 0f, 1f, 1f, 0f, 1f, 1f, 1f);
            matrix.DecomposeLQ(out var L, out var Q);
            // L should be lower triangular
            Assert.AreEqual(0f, L.M12, ZeroTolerance);
            Assert.AreEqual(0f, L.M13, ZeroTolerance);
            Assert.AreEqual(0f, L.M23, ZeroTolerance);
            // Q should be orthonormal
            Assert.AreEqual(1f, Q.Row1.Length(), ZeroTolerance);
        }

        #endregion

        #region Decompose and DecomposeUniformScale

        [TestMethod]
        public void Decompose_ScaleRotation_ReturnsTrue()
        {
            var matrix = Matrix3x3.Scaling(2f, 3f, 4f);
            var result = matrix.Decompose(out var scale, out _);
            Assert.IsTrue(result);
            Assert.AreEqual(2f, scale.X, ZeroTolerance);
            Assert.AreEqual(3f, scale.Y, ZeroTolerance);
            Assert.AreEqual(4f, scale.Z, ZeroTolerance);
        }

        [TestMethod]
        public void Decompose_ZeroScale_ReturnsFalse()
        {
            var matrix = new Matrix3x3();
            var result = matrix.Decompose(out _, out var rotation);
            Assert.IsFalse(result);
            Assert.AreEqual(Quaternion.Identity, rotation);
        }

        [TestMethod]
        public void DecomposeUniformScale_ReturnsCorrectScale()
        {
            var matrix = Matrix3x3.Scaling(5f);
            var result = matrix.DecomposeUniformScale(out var scale, out _);
            Assert.IsTrue(result);
            Assert.AreEqual(5f, scale, ZeroTolerance);
        }

        [TestMethod]
        public void DecomposeUniformScale_ZeroScale_ReturnsFalse()
        {
            var matrix = new Matrix3x3();
            var result = matrix.DecomposeUniformScale(out _, out _);
            Assert.IsFalse(result);
        }

        #endregion

        #region ExchangeRows and ExchangeColumns

        [TestMethod]
        public void ExchangeRows_SwapsRowsCorrectly()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            matrix.ExchangeRows(0, 1);
            Assert.AreEqual(4f, matrix.M11);
            Assert.AreEqual(1f, matrix.M21);
        }

        [TestMethod]
        public void ExchangeRows_SameRow_NoChange()
        {
            var matrix = new Matrix3x3(1f);
            matrix.ExchangeRows(0, 0);
            Assert.AreEqual(1f, matrix.M11);
        }

        [TestMethod]
        public void ExchangeColumns_SwapsColumnsCorrectly()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            matrix.ExchangeColumns(0, 1);
            Assert.AreEqual(2f, matrix.M11);
            Assert.AreEqual(1f, matrix.M12);
        }

        [TestMethod]
        public void ExchangeRows_InvalidIndices_Throws()
        {
            var matrix = new Matrix3x3();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeRows(-1, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeRows(0, 3));
        }

        [TestMethod]
        public void ExchangeColumns_InvalidIndices_Throws()
        {
            var matrix = new Matrix3x3();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeColumns(-1, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => matrix.ExchangeColumns(0, 3));
        }

        #endregion

        #region Transform and TransformTranspose

        [TestMethod]
        public void Transform_TransformsVector()
        {
            var matrix = Matrix3x3.Scaling(2f, 3f, 4f);
            var trn = new Vector3(1f, 1f, 1f);

            var result = matrix.Transform(trn);
            Assert.AreEqual(2f, result.X, ZeroTolerance);
            Assert.AreEqual(3f, result.Y, ZeroTolerance);
            Assert.AreEqual(4f, result.Z, ZeroTolerance);

            matrix.Transform(ref trn, out result);
            Assert.AreEqual(2f, result.X, ZeroTolerance);
            Assert.AreEqual(3f, result.Y, ZeroTolerance);
            Assert.AreEqual(4f, result.Z, ZeroTolerance);
        }

        [TestMethod]
        public void TransformTranspose_TransposesVector()
        {
            var matrix = Matrix3x3.Scaling(2f, 3f, 4f);
            var trn = new Vector3(1f, 1f, 1f);

            var result = matrix.TransformTranspose(trn);
            Assert.AreEqual(2f, result.X, ZeroTolerance);
            Assert.AreEqual(3f, result.Y, ZeroTolerance);
            Assert.AreEqual(4f, result.Z, ZeroTolerance);

            result = matrix.TransformTranspose(ref trn);
            Assert.AreEqual(2f, result.X, ZeroTolerance);
            Assert.AreEqual(3f, result.Y, ZeroTolerance);
            Assert.AreEqual(4f, result.Z, ZeroTolerance);

            var result2 = Matrix3x3.TransformTranspose(matrix, trn);
            Assert.IsTrue(Vector3.NearEqual(result, result2));

            Matrix3x3.TransformTranspose(ref matrix, ref trn, out result2);
            Assert.IsTrue(Vector3.NearEqual(result, result2));
        }

        [TestMethod]
        public void Static_Transform_TransformsWith4x4()
        {
            var m3 = Matrix3x3.Identity;

            var m4 = Matrix4x4.Scaling(2f, 3f, 4f);
            Assert.AreEqual(2f, m4.M11, ZeroTolerance);
            Assert.AreEqual(3f, m4.M22, ZeroTolerance);
            Assert.AreEqual(4f, m4.M33, ZeroTolerance);

            var result = Matrix3x3.Transform(m3, m4);
            Assert.AreEqual(4f, result.M11, ZeroTolerance);
            Assert.AreEqual(9f, result.M22, ZeroTolerance);
            Assert.AreEqual(16f, result.M33, ZeroTolerance);

            Matrix3x3.Transform(ref m3, ref m4, out result);
            Assert.AreEqual(4f, result.M11, ZeroTolerance);
            Assert.AreEqual(9f, result.M22, ZeroTolerance);
            Assert.AreEqual(16f, result.M33, ZeroTolerance);
        }

        #endregion

        #region Static math operations

        [TestMethod]
        public void AddRef_AddsMatrices()
        {
            var left = new Matrix3x3(1f);
            var right = new Matrix3x3(2f);
            Matrix3x3.Add(ref left, ref right, out var result);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void Add_AddsMatrices()
        {
            var left = new Matrix3x3(1f);
            var right = new Matrix3x3(2f);
            var result = Matrix3x3.Add(left, right);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void SubtractRef_SubtractsMatrices()
        {
            var left = new Matrix3x3(5f);
            var right = new Matrix3x3(2f);
            Matrix3x3.Subtract(ref left, ref right, out var result);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void Subtract_SubtractsMatrices()
        {
            var left = new Matrix3x3(5f);
            var right = new Matrix3x3(2f);
            var result = Matrix3x3.Subtract(left, right);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void MultiplyRef_Scalar_ScalesMatrix()
        {
            var matrix = new Matrix3x3(2f);
            Matrix3x3.Multiply(ref matrix, 3f, out var result);
            Assert.AreEqual(6f, result.M11);
        }

        [TestMethod]
        public void Multiply_Scalar_ScalesMatrix()
        {
            var matrix = new Matrix3x3(2f);
            var result = Matrix3x3.Multiply(matrix, 3f);
            Assert.AreEqual(6f, result.M11);
        }

        [TestMethod]
        public void MultiplyRef_Matrix_MultipliesCorrectly()
        {
            var a = Matrix3x3.Identity;
            var b = Matrix3x3.Scaling(2f, 3f, 4f);
            Matrix3x3.Multiply(ref a, ref b, out var result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Multiply_Matrix_MultipliesCorrectly()
        {
            var a = Matrix3x3.Identity;
            var b = Matrix3x3.Scaling(2f, 3f, 4f);
            var result = Matrix3x3.Multiply(a, b);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void DivideRef_Scalar_DividesMatrix()
        {
            var matrix = new Matrix3x3(6f);
            Matrix3x3.Divide(ref matrix, 2f, out var result);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void Divide_Scalar_DividesMatrix()
        {
            var matrix = new Matrix3x3(6f);
            var result = Matrix3x3.Divide(matrix, 2f);
            Assert.AreEqual(3f, result.M11);
        }

        [TestMethod]
        public void DivideRef_Matrix_DividesComponentWise()
        {
            var left = new Matrix3x3(6f, 6f, 6f, 6f, 6f, 6f, 6f, 6f, 6f);
            var right = new Matrix3x3(2f, 3f, 1f, 2f, 3f, 1f, 2f, 3f, 1f);
            Matrix3x3.Divide(ref left, ref right, out var result);
            Assert.AreEqual(3f, result.M11);
            Assert.AreEqual(2f, result.M12);
            Assert.AreEqual(6f, result.M13);
        }

        [TestMethod]
        public void Divide_Matrix_DividesComponentWise()
        {
            var left = new Matrix3x3(6f, 6f, 6f, 6f, 6f, 6f, 6f, 6f, 6f);
            var right = new Matrix3x3(2f, 3f, 1f, 2f, 3f, 1f, 2f, 3f, 1f);
            var result = Matrix3x3.Divide(left, right);
            Assert.AreEqual(3f, result.M11);
            Assert.AreEqual(2f, result.M12);
            Assert.AreEqual(6f, result.M13);
        }

        [TestMethod]
        public void NegateRef_NegatesAllComponents()
        {
            var matrix = new Matrix3x3(5f);
            Matrix3x3.Negate(ref matrix, out var result);
            Assert.AreEqual(-5f, result.M11);
        }

        [TestMethod]
        public void Negate_NegatesAllComponents()
        {
            var matrix = new Matrix3x3(5f);
            var result = Matrix3x3.Negate(matrix);
            Assert.AreEqual(-5f, result.M11);
        }

        #endregion

        #region Exponent

        [TestMethod]
        public void Exponent_Zero_ReturnsIdentity()
        {
            var matrix = new Matrix3x3(5f);

            var result = Matrix3x3.Exponent(matrix, 0);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));

            Matrix3x3.Exponent(ref matrix, 0, out result);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));
        }

        [TestMethod]
        public void Exponent_One_ReturnsSameMatrix()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);

            var result = Matrix3x3.Exponent(matrix, 1);
            Assert.IsTrue(Matrix3x3.NearEqual(result, matrix));

            Matrix3x3.Exponent(ref matrix, 1, out result);
            Assert.IsTrue(Matrix3x3.NearEqual(result, matrix));
        }

        [TestMethod]
        public void Exponent_Two_SquaresMatrix()
        {
            var matrix = Matrix3x3.Scaling(3f);

            var result = Matrix3x3.Exponent(matrix, 2);
            Assert.AreEqual(9f, result.M11, ZeroTolerance);
            Assert.AreEqual(9f, result.M22, ZeroTolerance);
            Assert.AreEqual(9f, result.M33, ZeroTolerance);

            Matrix3x3.Exponent(ref matrix, 2, out result);
            Assert.AreEqual(9f, result.M11, ZeroTolerance);
            Assert.AreEqual(9f, result.M22, ZeroTolerance);
            Assert.AreEqual(9f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Exponent_Negative_ThrowsArgumentOutOfRangeException()
        {
            var matrix = new Matrix3x3();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Matrix3x3.Exponent(matrix, -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Matrix3x3.Exponent(ref matrix, -1, out _));
        }

        #endregion

        #region Lerp and SmoothStep

        [TestMethod]
        public void Lerp_InterpolatesCorrectly()
        {
            var start = new Matrix3x3(0f);
            var end = new Matrix3x3(10f);

            var result = Matrix3x3.Lerp(start, end, 0.5f);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);

            Matrix3x3.Lerp(ref start, ref end, 0.5f, out result);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void Lerp_AmountZero_ReturnsStart()
        {
            var start = new Matrix3x3(1f);
            var end = new Matrix3x3(2f);

            var result = Matrix3x3.Lerp(start, end, 0f);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);

            Matrix3x3.Lerp(ref start, ref end, 0f, out result);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void Lerp_AmountOne_ReturnsEnd()
        {
            var start = new Matrix3x3(1f);
            var end = new Matrix3x3(2f);

            var result = Matrix3x3.Lerp(start, end, 1f);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);

            Matrix3x3.Lerp(ref start, ref end, 1f, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
        }

        [TestMethod]
        public void SmoothStep_InterpolatesWithCubicEase()
        {
            var start = new Matrix3x3(0f);
            var end = new Matrix3x3(1f);

            var result = Matrix3x3.SmoothStep(start, end, 0.5f);
            Assert.AreEqual(0.5f, result.M11, ZeroTolerance);

            Matrix3x3.SmoothStep(ref start, ref end, 0.5f, out result);
            Assert.AreEqual(0.5f, result.M11, ZeroTolerance);
        }

        #endregion

        #region Triangular and Echelon Forms

        [TestMethod]
        public void UpperTriangularForm_ZerosBelowDiagonal()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);

            var result = Matrix3x3.UpperTriangularForm(matrix);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);

            Matrix3x3.UpperTriangularForm(ref matrix, out result);
            Assert.AreEqual(0f, result.M21, ZeroTolerance);
            Assert.AreEqual(0f, result.M31, ZeroTolerance);
            Assert.AreEqual(0f, result.M32, ZeroTolerance);
        }

        [TestMethod]
        public void LowerTriangularForm_ZerosAboveDiagonal()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);

            var result = Matrix3x3.LowerTriangularForm(matrix);
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);

            Matrix3x3.LowerTriangularForm(ref matrix, out result);
            Assert.AreEqual(0f, result.M12, ZeroTolerance);
            Assert.AreEqual(0f, result.M13, ZeroTolerance);
            Assert.AreEqual(0f, result.M23, ZeroTolerance);
        }

        [TestMethod]
        public void RowEchelonForm_ProducesEchelonStructure()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);

            var result = Matrix3x3.RowEchelonForm(matrix);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);

            Matrix3x3.RowEchelonForm(ref matrix, out result);
            Assert.AreEqual(1f, result.M11, ZeroTolerance);
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
            Matrix3x3.BillboardLH(ref objPos, ref camPos, ref camUp, ref camForward, out var result);

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

            // Case 2: displaced object, camera on relative +Z
            objPos = new Vector3(5f, 3f, 2f);
            camPos = new Vector3(5f, 3f, 12f);
            camUp = new Vector3(0f, 1f, 0f);
            camForward = new Vector3(0f, 0f, -1f);
            Matrix3x3.BillboardLH(ref objPos, ref camPos, ref camUp, ref camForward, out result);

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
        }

        [TestMethod]
        public void BillboardLH_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            var result = Matrix3x3.BillboardLH(objPos, camPos, camUp, camForward);

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

            // Case 2: displaced object, camera on relative +Z
            objPos = new Vector3(5f, 3f, 2f);
            camPos = new Vector3(5f, 3f, 12f);
            camUp = new Vector3(0f, 1f, 0f);
            camForward = new Vector3(0f, 0f, -1f);
            result = Matrix3x3.BillboardLH(objPos, camPos, camUp, camForward);

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
        }

        [TestMethod]
        public void BillboardRHRef_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            Matrix3x3.BillboardRH(ref objPos, ref camPos, ref camUp, ref camForward, out var result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void BillboardRH_CreatesBillboardMatrix()
        {
            var objPos = new Vector3(0f, 0f, 0f);
            var camPos = new Vector3(0f, 0f, 10f);
            var camUp = new Vector3(0f, 1f, 0f);
            var camForward = new Vector3(0f, 0f, -1f);
            var result = Matrix3x3.BillboardRH(objPos, camPos, camUp, camForward);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void LookAtLHRef_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            Matrix3x3.LookAtLH(ref eye, ref target, ref up, out var result);
            Assert.AreEqual(-1f, result.M11);
            Assert.AreEqual(1f, result.M22);
            Assert.AreEqual(-1f, result.M33);
        }

        [TestMethod]
        public void LookAtLH_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            var result = Matrix3x3.LookAtLH(eye, target, up);
            Assert.AreEqual(-1f, result.M11);
            Assert.AreEqual(1f, result.M22);
            Assert.AreEqual(-1f, result.M33);
        }

        [TestMethod]
        public void LookAtRHRef_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            Matrix3x3.LookAtRH(ref eye, ref target, ref up, out var result);
            Assert.AreEqual(1f, result.M11);
            Assert.AreEqual(1f, result.M22);
            Assert.AreEqual(1f, result.M33);
        }

        [TestMethod]
        public void LookAtRH_CreatesLookAtMatrix()
        {
            var eye = new Vector3(0f, 0f, 5f);
            var target = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 1f, 0f);
            var result = Matrix3x3.LookAtRH(eye, target, up);
            Assert.AreEqual(1f, result.M11);
            Assert.AreEqual(1f, result.M22);
            Assert.AreEqual(1f, result.M33);
        }

        #endregion

        #region Scaling and Rotation factories

        [TestMethod]
        public void Scaling_Vector_ScalesCorrectly()
        {
            var scaling = new Vector3(2f, 3f, 4f);

            var result = Matrix3x3.Scaling(scaling);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);

            Matrix3x3.Scaling(ref scaling, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Scaling_Components_ScalesCorrectly()
        {
            var result = Matrix3x3.Scaling(2f, 3f, 4f);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);

            Matrix3x3.Scaling(2f, 3f, 4f, out result);
            Assert.AreEqual(2f, result.M11, ZeroTolerance);
            Assert.AreEqual(3f, result.M22, ZeroTolerance);
            Assert.AreEqual(4f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void Scaling_Uniform_ScalesAllDiagonal()
        {
            var result = Matrix3x3.Scaling(5f);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);
            Assert.AreEqual(5f, result.M22, ZeroTolerance);
            Assert.AreEqual(5f, result.M33, ZeroTolerance);

            Matrix3x3.Scaling(5f, out result);
            Assert.AreEqual(5f, result.M11, ZeroTolerance);
            Assert.AreEqual(5f, result.M22, ZeroTolerance);
            Assert.AreEqual(5f, result.M33, ZeroTolerance);
        }

        [TestMethod]
        public void RotationX_CorrectMatrix()
        {
            var result = Matrix3x3.RotationX(0f);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));

            var cos = MathF.Cos(MathF.PI / 2);
            var sin = MathF.Sin(MathF.PI / 2);

            var half = Matrix3x3.RotationX(MathF.PI / 2);
            Assert.AreEqual(cos, half.M22, ZeroTolerance);
            Assert.AreEqual(sin, half.M23, ZeroTolerance);
            Assert.AreEqual(-sin, half.M32, ZeroTolerance);
            Assert.AreEqual(cos, half.M33, ZeroTolerance);

            Matrix3x3.RotationX(MathF.PI / 2, out half);
            Assert.AreEqual(cos, half.M22, ZeroTolerance);
            Assert.AreEqual(sin, half.M23, ZeroTolerance);
            Assert.AreEqual(-sin, half.M32, ZeroTolerance);
            Assert.AreEqual(cos, half.M33, ZeroTolerance);
        }

        [TestMethod]
        public void RotationY_CorrectMatrix()
        {
            var result = Matrix3x3.RotationY(0f);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));

            var half = Matrix3x3.RotationY(MathF.PI / 2);
            Assert.AreEqual(0f, half.M11, ZeroTolerance); // cos(PI/2) ≈ 0

            Matrix3x3.RotationY(MathF.PI / 2, out half);
            Assert.AreEqual(0f, half.M11, ZeroTolerance); // cos(PI/2) ≈ 0
        }

        [TestMethod]
        public void RotationZ_CorrectMatrix()
        {
            var result = Matrix3x3.RotationZ(0f);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));

            var half = Matrix3x3.RotationZ(MathF.PI / 2);
            Assert.AreEqual(0f, half.M11, ZeroTolerance);

            Matrix3x3.RotationZ(MathF.PI / 2, out half);
            Assert.AreEqual(0f, half.M11, ZeroTolerance);
        }

        [TestMethod]
        public void RotationAxis_RotatesAroundGivenAxis()
        {
            var axis = new Vector3(0f, 1f, 0f);

            var result = Matrix3x3.RotationAxis(axis, MathF.PI / 2);
            Assert.IsFalse(result.IsIdentity);

            Matrix3x3.RotationAxis(ref axis, MathF.PI / 2, out result);
            Assert.IsFalse(result.IsIdentity);
        }

        [TestMethod]
        public void RotationQuaternion_FromIdentityQuaternion_ReturnsIdentity()
        {
            var rot = Quaternion.Identity;

            var result = Matrix3x3.RotationQuaternion(rot);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));

            Matrix3x3.RotationQuaternion(ref rot, out result);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));
        }

        [TestMethod]
        public void RotationYawPitchRoll_CreatesRotationMatrix()
        {
            var result = Matrix3x3.RotationYawPitchRoll(0f, 0f, 0f);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));

            Matrix3x3.RotationYawPitchRoll(0f, 0f, 0f, out result);
            Assert.IsTrue(Matrix3x3.NearEqual(result, Matrix3x3.Identity));
        }

        #endregion

        #region SkewSymmetric

        [TestMethod]
        public void SkewSymmetric_CreatesCorrectMatrix()
        {
            var vector = new Vector3(1f, 2f, 3f);

            var result = Matrix3x3.SkewSymmetric(vector);
            Assert.AreEqual(0f, result.M11, ZeroTolerance);
            Assert.AreEqual(0f, result.M22, ZeroTolerance);
            Assert.AreEqual(0f, result.M33, ZeroTolerance);
            Assert.AreEqual(-3f, result.M12, ZeroTolerance);
            Assert.AreEqual(2f, result.M13, ZeroTolerance);
            Assert.AreEqual(3f, result.M21, ZeroTolerance);
            Assert.AreEqual(-1f, result.M23, ZeroTolerance);
            Assert.AreEqual(-2f, result.M31, ZeroTolerance);
            Assert.AreEqual(1f, result.M32, ZeroTolerance);

            Matrix3x3.SkewSymmetric(ref vector, out result);
            Assert.AreEqual(0f, result.M11, ZeroTolerance);
            Assert.AreEqual(0f, result.M22, ZeroTolerance);
            Assert.AreEqual(0f, result.M33, ZeroTolerance);
            Assert.AreEqual(-3f, result.M12, ZeroTolerance);
            Assert.AreEqual(2f, result.M13, ZeroTolerance);
            Assert.AreEqual(3f, result.M21, ZeroTolerance);
            Assert.AreEqual(-1f, result.M23, ZeroTolerance);
            Assert.AreEqual(-2f, result.M31, ZeroTolerance);
            Assert.AreEqual(1f, result.M32, ZeroTolerance);
        }

        #endregion

        #region Operators

        [TestMethod]
        public void Operators_Addition_Subtraction_Multiplication_Division_Work()
        {
            var left = new Matrix3x3(2f);
            var right = new Matrix3x3(1f);

            Assert.AreEqual(new Matrix3x3(3f), left + right);
            Assert.AreEqual(new Matrix3x3(1f), left - right);
            Assert.AreEqual(new Matrix3x3(6f), left * right);
            Assert.AreEqual(new Matrix3x3(2f), left / right);
            Assert.AreEqual(new Matrix3x3(6f), left * 3f);
            Assert.AreEqual(new Matrix3x3(6f), 3f * left);
            Assert.AreEqual(new Matrix3x3(2f / 3f), left / 3f);
        }

        [TestMethod]
        public void Operators_UnaryPlusAndNegation_Work()
        {
            var matrix = new Matrix3x3(5f);
            Assert.AreEqual(new Matrix3x3(5f), +matrix);
            Assert.AreEqual(new Matrix3x3(-5f), -matrix);
        }

        [TestMethod]
        public void Operators_EqualityAndInequality_Work()
        {
            var a = new Matrix3x3(1f);
            var b = new Matrix3x3(1f);
            var c = new Matrix3x3(2f);
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

        #endregion

        #region ToString

        [TestMethod]
        public void ToString_Default_ReturnsFormattedString()
        {
            var matrix = new Matrix3x3(
                1f, 2f, 3f,
                4f, 5f, 6f,
                7f, 8f, 9f);
            var str = matrix.ToString();

            Assert.Contains($"M11:{1f}", str);
            Assert.Contains($"M12:{2f}", str);
            Assert.Contains($"M13:{3f}", str);
            Assert.Contains($"M21:{4f}", str);
            Assert.Contains($"M22:{5f}", str);
            Assert.Contains($"M23:{6f}", str);
            Assert.Contains($"M31:{7f}", str);
            Assert.Contains($"M32:{8f}", str);
            Assert.Contains($"M33:{9f}", str);
        }

        [TestMethod]
        public void ToString_WithFormat_ReturnsFormattedString()
        {
            var matrix = new Matrix3x3(
                1.5f, 2.5f, 3.5f,
                4.5f, 5.5f, 6.5f,
                7.5f, 8.5f, 9.5f);
            var str = matrix.ToString("F1");

            Assert.Contains($"M11:{1.5f}", str);
            Assert.Contains($"M12:{2.5f}", str);
            Assert.Contains($"M13:{3.5f}", str);
            Assert.Contains($"M21:{4.5f}", str);
            Assert.Contains($"M22:{5.5f}", str);
            Assert.Contains($"M23:{6.5f}", str);
            Assert.Contains($"M31:{7.5f}", str);
            Assert.Contains($"M32:{8.5f}", str);
            Assert.Contains($"M33:{9.5f}", str);
        }

        [TestMethod]
        public void ToString_WithFormatProvider_ReturnsFormattedString()
        {
            var matrix = new Matrix3x3(1.5f);
            var str = matrix.ToString(System.Globalization.CultureInfo.CurrentCulture);

            Assert.Contains($"M11:{1.5f}", str);
            Assert.Contains($"M12:{1.5f}", str);
            Assert.Contains($"M13:{1.5f}", str);
            Assert.Contains($"M21:{1.5f}", str);
            Assert.Contains($"M22:{1.5f}", str);
            Assert.Contains($"M23:{1.5f}", str);
            Assert.Contains($"M31:{1.5f}", str);
            Assert.Contains($"M32:{1.5f}", str);
            Assert.Contains($"M33:{1.5f}", str);
        }

        [TestMethod]
        public void ToString_FormatAndProvider_ReturnsFormattedString()
        {
            var matrix = new Matrix3x3(1.5f);
            var str = matrix.ToString("F1", System.Globalization.CultureInfo.CurrentCulture);

            Assert.Contains($"M11:{1.5f:F1}", str);
            Assert.Contains($"M12:{1.5f:F1}", str);
            Assert.Contains($"M13:{1.5f:F1}", str);
            Assert.Contains($"M21:{1.5f:F1}", str);
            Assert.Contains($"M22:{1.5f:F1}", str);
            Assert.Contains($"M23:{1.5f:F1}", str);
            Assert.Contains($"M31:{1.5f:F1}", str);
            Assert.Contains($"M32:{1.5f:F1}", str);
            Assert.Contains($"M33:{1.5f:F1}", str);
        }

        #endregion

        #region GetHashCode, Equals and NearEqual

        [TestMethod]
        public void GetHashCode_ReturnsConsistentHash()
        {
            var matrix = new Matrix3x3(1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f);
            var hash1 = matrix.GetHashCode();
            var hash2 = matrix.GetHashCode();
            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void Equals_EqualMatrices_ReturnsTrue()
        {
            var a = new Matrix3x3(1f);
            var b = new Matrix3x3(1f);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals(ref b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [TestMethod]
        public void Equals_DifferentMatrices_ReturnsFalse()
        {
            var a = new Matrix3x3(1f);
            var b = new Matrix3x3(2f);
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod]
        public void Equals_NonMatrixObject_ReturnsFalse()
        {
            var matrix = new Matrix3x3();
            Assert.IsFalse(matrix.Equals("not a matrix"));
            Assert.IsFalse(matrix.Equals(42));
            Assert.IsFalse(matrix.Equals(null));
        }

        [TestMethod]
        public void NearEqual_NearMatrices_ReturnsTrue()
        {
            var a = new Matrix3x3(
                1f, 2f, 3f,
                4f, 5f, 6f,
                7f, 8f, 9f);

            var b = new Matrix3x3(
                1.00001f, 2.00001f, 3.00001f,
                4.00001f, 5.00001f, 6.00001f,
                7.00001f, 8.00001f, 9.00001f);

            Assert.IsTrue(Matrix3x3.NearEqual(a, b, ZeroTolerance));
        }

        [TestMethod]
        public void NearEqual_FarMatrices_ReturnsFalse()
        {
            var a = new Matrix3x3(1f);
            var b = new Matrix3x3(2f);

            Assert.IsFalse(Matrix3x3.NearEqual(a, b, ZeroTolerance));
        }

        #endregion

        #region GetDescription

        [TestMethod]
        public void GetDescription_ReturnsDescriptionForValidMatrix()
        {
            var matrix = Matrix3x3.Scaling(2f, 3f, 4f);
            var desc = matrix.GetDescription();
            Assert.IsFalse(string.IsNullOrEmpty(desc));
            Assert.DoesNotContain("Bad matrix", desc);
        }

        [TestMethod]
        public void GetDescription_ReturnsBadMatrixForZeroMatrix()
        {
            var desc = Matrix3x3.Zero.GetDescription();
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
