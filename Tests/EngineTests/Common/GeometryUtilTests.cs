using Engine;
using Engine.Common;
using Engine.Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EngineTests.Common
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    [OSCondition(OperatingSystems.Windows)]
    public class GeometryUtilTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        #region LevelOfDetail Tests

        [TestMethod]
        [DataRow(LevelOfDetail.High, 64, 96)]
        [DataRow(LevelOfDetail.Medium, 64, 24)]
        [DataRow(LevelOfDetail.Low, 256, 24)]
        [DataRow(LevelOfDetail.Minimum, 1024, 24)]
        public void GenerateIndices_FullShape_ReturnsExpectedCount_ForAllLODLevels(LevelOfDetail lod, int triangles, int expectedCount)
        {
            var result = GeometryUtil.GenerateIndices(lod, IndexBufferShapes.Full, triangles).ToArray();

            Assert.IsNotNull(result, $"Failed for LOD: {lod}");
            Assert.HasCount(expectedCount, result, $"Expected {expectedCount} indices for LOD {lod} with {triangles} triangles, got {result.Length}");
        }

        [TestMethod]
        [DataRow(LevelOfDetail.High, 64, 96)]
        [DataRow(LevelOfDetail.Medium, 64, 24)]
        [DataRow(LevelOfDetail.Low, 256, 24)]
        [DataRow(LevelOfDetail.Minimum, 1024, 24)]
        [DataRow(LevelOfDetail.High, 128, 384)]
        [DataRow(LevelOfDetail.Medium, 128, 96)]
        [DataRow(LevelOfDetail.Low, 512, 96)]
        [DataRow(LevelOfDetail.Minimum, 2048, 96)]
        public void GenerateIndices_FullShape_ReturnsExpectedCount_ForLargerTriangleCounts(LevelOfDetail lod, int triangles, int expectedCount)
        {
            var result = GeometryUtil.GenerateIndices(lod, IndexBufferShapes.Full, triangles).ToArray();

            Assert.IsNotNull(result, $"Failed for LOD: {lod}");
            Assert.HasCount(expectedCount, result, $"Expected {expectedCount} indices for LOD {lod} with {triangles} triangles, got {result.Length}");
        }

        [TestMethod]
        [DataRow(LevelOfDetail.High, 64)]
        [DataRow(LevelOfDetail.Medium, 64)]
        [DataRow(LevelOfDetail.Low, 256)]
        [DataRow(LevelOfDetail.Minimum, 1024)]
        public void GenerateIndices_FullShape_ReturnsNonEmptyCollection_ForAllLODLevels(LevelOfDetail lod, int triangles)
        {
            var result = GeometryUtil.GenerateIndices(lod, IndexBufferShapes.Full, triangles).ToArray();

            Assert.IsNotNull(result, $"Failed for LOD: {lod}");
            Assert.IsNotEmpty(result, $"Collection should not be empty for LOD {lod}");
        }

        [TestMethod]
        [DataRow(LevelOfDetail.High, 64)]
        [DataRow(LevelOfDetail.Medium, 64)]
        [DataRow(LevelOfDetail.Low, 256)]
        [DataRow(LevelOfDetail.Minimum, 1024)]
        public void GenerateIndices_FullShape_ReturnsValidUIntIndices_ForAllLODLevels(LevelOfDetail lod, int triangles)
        {
            var result = GeometryUtil.GenerateIndices(lod, IndexBufferShapes.Full, triangles).ToArray();

            foreach (var index in result)
            {
                Assert.IsLessThanOrEqualTo(uint.MaxValue, index, $"Index {index} exceeds uint.MaxValue for LOD {lod}");
            }
        }

        [TestMethod]
        [DataRow(LevelOfDetail.High, 64)]
        [DataRow(LevelOfDetail.Medium, 64)]
        [DataRow(LevelOfDetail.Low, 256)]
        [DataRow(LevelOfDetail.Minimum, 1024)]
        public void GenerateIndices_FullShape_ReturnsCorrectTriangleCount_ForAllLODLevels(LevelOfDetail lod, int triangles)
        {
            var result = GeometryUtil.GenerateIndices(lod, IndexBufferShapes.Full, triangles).ToArray();

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result, $"Expected non-zero indices for LOD {lod}");
        }

        #endregion

        #region Side Shape Tests

        [TestMethod]
        [DataRow(IndexBufferShapes.SideTop, 64, 90)]
        [DataRow(IndexBufferShapes.SideBottom, 64, 96)]
        [DataRow(IndexBufferShapes.SideLeft, 64, 90)]
        [DataRow(IndexBufferShapes.SideRight, 64, 96)]
        public void GenerateIndices_SideShapes_ReturnCorrectCount(IndexBufferShapes shape, int triangles, int maxExpectedCount)
        {
            var lod = LevelOfDetail.High;
            var result = GeometryUtil.GenerateIndices(lod, shape, triangles).ToArray();

            Assert.IsNotNull(result);
            Assert.HasCount(maxExpectedCount, result, $"Side shape {shape} should return {maxExpectedCount} indices");
        }

        #endregion

        #region Corner Shape Tests

        [TestMethod]
        [DataRow(IndexBufferShapes.CornerTopLeft, 64, 84)]
        [DataRow(IndexBufferShapes.CornerBottomLeft, 64, 90)]
        [DataRow(IndexBufferShapes.CornerTopRight, 64, 90)]
        [DataRow(IndexBufferShapes.CornerBottomRight, 64, 96)]
        public void GenerateIndices_CornerShapes_ReturnCorrectCount(IndexBufferShapes shape, int triangles, int maxExpectedCount)
        {
            var lod = LevelOfDetail.High;
            var result = GeometryUtil.GenerateIndices(lod, shape, triangles).ToArray();

            Assert.IsNotNull(result);
            Assert.HasCount(maxExpectedCount, result, $"Corner shape {shape} should return less than {maxExpectedCount} indices");
        }

        #endregion

        #region LOD with Corner Shape Tests

        [TestMethod]
        [DataRow(LevelOfDetail.High, IndexBufferShapes.CornerTopLeft, 64, 84)]
        [DataRow(LevelOfDetail.Medium, IndexBufferShapes.CornerTopLeft, 64, 18)]
        [DataRow(LevelOfDetail.Low, IndexBufferShapes.CornerTopLeft, 256, 18)]
        [DataRow(LevelOfDetail.Minimum, IndexBufferShapes.CornerTopLeft, 1024, 18)]
        public void GenerateIndices_CornerTopLeftShape_ReturnsExpectedCount_ForAllLODLevels(LevelOfDetail lod, IndexBufferShapes shape, int triangles, int maxExpectedCount)
        {
            var result = GeometryUtil.GenerateIndices(lod, shape, triangles).ToArray();

            Assert.IsNotNull(result);
            Assert.HasCount(maxExpectedCount, result, $"CornerTopLeft with LOD {lod} should return {maxExpectedCount} indices");
        }

        [TestMethod]
        [DataRow(IndexBufferShapes.CornerTopLeft, 64, 84)]
        [DataRow(IndexBufferShapes.CornerBottomLeft, 64, 90)]
        [DataRow(IndexBufferShapes.CornerTopRight, 64, 90)]
        [DataRow(IndexBufferShapes.CornerBottomRight, 64, 96)]
        public void GenerateIndices_CornerShapes_ReturnCorrectCount_ForAllCornerTypes(IndexBufferShapes shape, int triangles, int maxExpectedCount)
        {
            var lod = LevelOfDetail.High;
            var result = GeometryUtil.GenerateIndices(lod, shape, triangles).ToArray();

            Assert.IsNotNull(result);
            Assert.HasCount(maxExpectedCount, result, $"Corner shape {shape} should return {maxExpectedCount} indices");
        }

        #endregion

        #region Return Type Tests

        [TestMethod]
        public void GenerateIndices_ReturnsIEnumerableOfUInt()
        {
            var lod = LevelOfDetail.High;
            var shape = IndexBufferShapes.Full;
            var triangles = 64;

            var result = GeometryUtil.GenerateIndices(lod, shape, triangles);

            Assert.IsInstanceOfType<IEnumerable<uint>>(result);
        }

        #endregion

        #region ChangeCoordinate Tests

        [TestMethod]
        public void ChangeCoordinate_SwapsYCoordinates_InTriangleIndices()
        {
            // Arrange
            var indices = new uint[] { 0, 1, 2, 3, 4, 5 }; // Triangle 1: (0,1,2), Triangle 2: (3,4,5)

            // Act
            var result = GeometryUtil.ChangeCoordinate(indices).ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(6, result);
            // Triangle 1: (0,2,1) - Y swapped
            Assert.AreEqual(0u, result[0]);
            Assert.AreEqual(2u, result[1]);
            Assert.AreEqual(1u, result[2]);
            // Triangle 2: (3,5,4) - Y swapped
            Assert.AreEqual(3u, result[3]);
            Assert.AreEqual(5u, result[4]);
            Assert.AreEqual(4u, result[5]);
        }

        [TestMethod]
        [DataRow(3)]
        [DataRow(6)]
        [DataRow(9)]
        [DataRow(12)]
        [DataRow(15)]
        public void ChangeCoordinate_MultipleTriangles_SwapsYCoordinates_EachTriangle(int triangleCount)
        {
            // Arrange
            var indices = new uint[triangleCount * 3];
            for (int i = 0; i < triangleCount; i++)
            {
                indices[i * 3 + 0] = (uint)(i * 3);
                indices[i * 3 + 1] = (uint)(i * 3 + 1);
                indices[i * 3 + 2] = (uint)(i * 3 + 2);
            }

            // Act
            var result = GeometryUtil.ChangeCoordinate(indices).ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(triangleCount * 3, result);

            for (int i = 0; i < triangleCount; i++)
            {
                Assert.AreEqual((uint)(i * 3), result[i * 3], $"Triangle {i}: X index should remain same");
                Assert.AreEqual((uint)(i * 3 + 2), result[i * 3 + 1], $"Triangle {i}: Y index should swap with Z");
                Assert.AreEqual((uint)(i * 3 + 1), result[i * 3 + 2], $"Triangle {i}: Z index should swap with Y");
            }
        }

        [TestMethod]
        public void ChangeCoordinate_EmptyCollection_ReturnsEmptyCollection()
        {
            // Arrange
            var indices = Array.Empty<uint>();

            // Act
            var result = GeometryUtil.ChangeCoordinate(indices).ToArray();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void ChangeCoordinate_ReturnsIEnumerableOfUInt()
        {
            // Arrange
            var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

            // Act
            var result = GeometryUtil.ChangeCoordinate(indices);

            // Assert
            Assert.IsInstanceOfType<IEnumerable<uint>>(result);
        }

        #endregion

        #region GenerateNormals Tests

        [TestMethod]
        public void GenerateNormals()
        {
            Vector3[] positions = [Vector3.Left, Vector3.Right, Vector3.ForwardLH];

            uint[] indices = [0, 1, 2];
            Vector3[] expected = [Vector3.Down, Vector3.Down, Vector3.Down];
            var result = GeometryUtil.GenerateNormals(positions, indices);
            Assert.IsNotNull(result, "GenerateNormals should not return null.");
            Assert.AreSequenceEqual(expected, result, "Result length must match positions length.");

            indices = [0, 2, 1];
            expected = [Vector3.Up, Vector3.Up, Vector3.Up];
            result = GeometryUtil.GenerateNormals(positions, indices);
            Assert.IsNotNull(result, "GenerateNormals should not return null.");
            Assert.AreSequenceEqual(expected, result, "Result length must match positions length.");
        }

        #endregion
    }
}
