using Engine.Common;
using Engine.PathFinding.RecastNavigation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Engine.PathFinding.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class PathFinderDescriptionTests
    {
        static TestContext _testContext;

        static IEnumerable<Triangle> zeroPlaneTris;
        static IEnumerable<Triangle> hOnePlaneTris;
        static IEnumerable<Triangle> hTwoPlaneTris;
        static IEnumerable<Triangle> sceneryTris;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            var pZero = GeometryUtil.CreateXZPlane(10, 0);
            zeroPlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pZero.Vertices, pZero.Indices);

            var pOne = GeometryUtil.CreateXZPlane(10, 5);
            hOnePlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pOne.Vertices, pOne.Indices);

            var pTwo = GeometryUtil.CreateXZPlane(10, 10);
            hTwoPlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pTwo.Vertices, pTwo.Indices);

            sceneryTris = (new[] { zeroPlaneTris, hOnePlaneTris, hTwoPlaneTris }).SelectMany(t => t);
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void PathFinderDescriptionTest()
        {
            Mock<PathFinderSettings> mockSettings = new Mock<PathFinderSettings>();

            Func<IEnumerable<Triangle>> fnc = () => { return zeroPlaneTris; };
            Mock<PathFinderInput> mockInput = new Mock<PathFinderInput>(new object[] { fnc });

            var pfDesc = new PathFinderDescription(mockSettings.Object, mockInput.Object);

            Assert.IsNotNull(pfDesc);
            Assert.IsNotNull(pfDesc.Settings);
            Assert.IsNotNull(pfDesc.Input);
        }

        [TestMethod()]
        public void BuildTest()
        {
            Mock<PathFinderSettings> mockSettings = new Mock<PathFinderSettings>();

            Func<IEnumerable<Triangle>> fnc = () => { return zeroPlaneTris; };
            Mock<PathFinderInput> mockInput = new Mock<PathFinderInput>(new object[] { fnc });

            Mock<IGraph> mockGraph = new Mock<IGraph>();
            mockInput.Setup(i => i.CreateGraph(It.IsAny<PathFinderSettings>())).ReturnsAsync(mockGraph.Object);

            var pfDesc = new PathFinderDescription(mockSettings.Object, mockInput.Object);
            var res = pfDesc.Build().GetAwaiter().GetResult();

            Assert.IsNotNull(res);
        }

        [TestMethod()]
        public void BuildNavmeshTest()
        {
            BuildSettings settings = BuildSettings.Default;
            Func<IEnumerable<Triangle>> fnc = () => { return zeroPlaneTris; };
            InputGeometry input = new InputGeometry(fnc);

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(new Agent(), Vector3.Zero);
            var walkable2 = graph.IsWalkable(new Agent(), Vector3.Up * 5f);
            var walkable3 = graph.IsWalkable(new Agent(), Vector3.Up * 10f);

            Assert.IsNotNull(graph);
            Assert.IsTrue(walkable1);
            Assert.IsFalse(walkable2);
            Assert.IsFalse(walkable3);
        }

        [TestMethod()]
        public void BuildNavmeshOneTest()
        {
            BuildSettings settings = BuildSettings.Default;
            Func<IEnumerable<Triangle>> fnc = () => { return hOnePlaneTris; };
            InputGeometry input = new InputGeometry(fnc);

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(new Agent(), Vector3.Up * -200f);
            var walkable2 = graph.IsWalkable(new Agent(), Vector3.Up * 5f);
            var walkable3 = graph.IsWalkable(new Agent(), Vector3.Up * 20f);

            Assert.IsNotNull(graph);
            Assert.IsFalse(walkable1);
            Assert.IsTrue(walkable2);
            Assert.IsFalse(walkable3);
        }

        [TestMethod()]
        public void BuildNavmeshTwoTest()
        {
            BuildSettings settings = BuildSettings.Default;
            Func<IEnumerable<Triangle>> fnc = () => { return hTwoPlaneTris; };
            InputGeometry input = new InputGeometry(fnc);

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(new Agent(), Vector3.Zero);
            var walkable2 = graph.IsWalkable(new Agent(), Vector3.Up * 5f);
            var walkable3 = graph.IsWalkable(new Agent(), Vector3.Up * 10f);

            Assert.IsNotNull(graph);
            Assert.IsFalse(walkable1);
            Assert.IsFalse(walkable2);
            Assert.IsTrue(walkable3);
        }

        [TestMethod()]
        public void BuildNavmeshSceneryTest()
        {
            BuildSettings settings = BuildSettings.Default;
            Func<IEnumerable<Triangle>> fnc = () => { return sceneryTris; };
            InputGeometry input = new InputGeometry(fnc);

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(new Agent(), Vector3.Zero);
            var walkable2 = graph.IsWalkable(new Agent(), Vector3.Up * 5f);
            var walkable3 = graph.IsWalkable(new Agent(), Vector3.Up * 10f);

            Assert.IsNotNull(graph);
            Assert.IsTrue(walkable1);
            Assert.IsTrue(walkable2);
            Assert.IsTrue(walkable3);
        }
    }
}