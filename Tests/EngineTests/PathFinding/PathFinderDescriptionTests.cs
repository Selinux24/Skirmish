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

        static Agent agentDefault;
        static Agent agentSmall;
        static Agent agentTall;

        static IEnumerable<Triangle> zeroPlaneTris;
        static IEnumerable<Triangle> hOnePlaneTris;
        static IEnumerable<Triangle> hTwoPlaneTris;
        static IEnumerable<Triangle> sceneryTris;

        static Vector3 pointZero;
        static Vector3 pointOne;
        static Vector3 pointTwo;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;

            float hZero = 0;
            float hOne = 5;
            float hTwo = 10;

            float rDefault = 0.5f;

            agentDefault = new Agent() { Radius = rDefault, Height = hOne * 0.5f };
            agentSmall = new Agent() { Radius = rDefault, Height = hOne * 0.1f };
            agentTall = new Agent() { Radius = rDefault, Height = hOne * 1.5f };

            var pZero = GeometryUtil.CreateXZPlane(10, hZero);
            zeroPlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pZero.Vertices, pZero.Indices);
            pointZero = Vector3.Up * hZero;

            var pOne = GeometryUtil.CreateXZPlane(10, hOne);
            hOnePlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pOne.Vertices, pOne.Indices);
            pointOne = Vector3.Up * hOne;

            var pTwo = GeometryUtil.CreateXZPlane(10, hTwo);
            hTwoPlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pTwo.Vertices, pTwo.Indices);
            pointTwo = Vector3.Up * hTwo;

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
        public void BuildNavmeshZeroTest()
        {
            BuildSettings settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            InputGeometry input = new InputGeometry(() => { return zeroPlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsTrue(walkable1);
            Assert.AreEqual(pointZero, n1);
            Assert.IsFalse(walkable2);
            Assert.IsNull(n2);
            Assert.IsFalse(walkable3);
            Assert.IsNull(n3);
        }

        [TestMethod()]
        public void BuildNavmeshOneTest()
        {
            BuildSettings settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            InputGeometry input = new InputGeometry(() => { return hOnePlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsFalse(walkable1);
            Assert.AreEqual(pointOne, n1);
            Assert.IsTrue(walkable2);
            Assert.AreEqual(pointOne, n2);
            Assert.IsFalse(walkable3);
            Assert.IsNull(n3);
        }

        [TestMethod()]
        public void BuildNavmeshTwoTest()
        {
            BuildSettings settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            InputGeometry input = new InputGeometry(() => { return hTwoPlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsFalse(walkable1);
            Assert.AreEqual(pointTwo, n1);
            Assert.IsFalse(walkable2);
            Assert.AreEqual(pointTwo, n2);
            Assert.IsTrue(walkable3);
            Assert.AreEqual(pointTwo, n3);
        }

        [TestMethod()]
        public void BuildNavmeshSceneryTest()
        {
            BuildSettings settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            InputGeometry input = new InputGeometry(() => { return sceneryTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.Build().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsTrue(walkable1);
            Assert.AreEqual(pointZero, n1);
            Assert.IsTrue(walkable2);
            Assert.AreEqual(pointOne, n2);
            Assert.IsTrue(walkable3);
            Assert.AreEqual(pointTwo, n3);
        }
    }
}