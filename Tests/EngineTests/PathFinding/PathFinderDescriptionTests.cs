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
        static Agent agentInclined;

        static IEnumerable<Triangle> zeroPlaneTris;
        static IEnumerable<Triangle> hOnePlaneTris;
        static IEnumerable<Triangle> hTwoPlaneTris;
        static IEnumerable<Triangle> sceneryTris;
        static IEnumerable<Triangle> inclinedPlaneTris;

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
            agentInclined = new Agent() { Radius = rDefault, Height = hOne * 0.5f, MaxSlope = 50f };

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

            var pInclined = GeometryUtil.CreatePlane(10, 0, Vector3.Normalize(new Vector3(1, 1, 0)));
            inclinedPlaneTris = Triangle.ComputeTriangleList(Topology.TriangleList, pInclined.Vertices, pInclined.Indices);
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void PathFinderDescriptionTest()
        {
            var mockSettings = new Mock<PathFinderSettings>();

            var fnc = () => { return zeroPlaneTris; };
            var mockInput = new Mock<PathFinderInput>(new object[] { fnc });

            var pfDesc = new PathFinderDescription(mockSettings.Object, mockInput.Object);

            Assert.IsNotNull(pfDesc);
            Assert.IsNotNull(pfDesc.Settings);
            Assert.IsNotNull(pfDesc.Input);
        }

        [TestMethod()]
        public void BuildAsyncTest()
        {
            var mockSettings = new Mock<PathFinderSettings>();

            var fnc = () => { return zeroPlaneTris; };
            var mockInput = new Mock<PathFinderInput>(new object[] { fnc });

            var mockGraph = new Mock<IGraph>();
            mockInput.Setup(i => i.CreateGraphAsync(It.IsAny<PathFinderSettings>(), null)).ReturnsAsync(mockGraph.Object);

            var pfDesc = new PathFinderDescription(mockSettings.Object, mockInput.Object);
            var res = pfDesc.BuildAsync().GetAwaiter().GetResult();

            Assert.IsNotNull(res);
        }
        [TestMethod()]
        public void BuildTest()
        {
            var mockSettings = new Mock<PathFinderSettings>();

            var fnc = () => { return zeroPlaneTris; };
            var mockInput = new Mock<PathFinderInput>(new object[] { fnc });

            var mockGraph = new Mock<IGraph>();
            mockInput.Setup(i => i.CreateGraph(It.IsAny<PathFinderSettings>(), null)).Returns(mockGraph.Object);

            var pfDesc = new PathFinderDescription(mockSettings.Object, mockInput.Object);
            var res = pfDesc.Build();

            Assert.IsNotNull(res);
        }

        [TestMethod()]
        public void BuildNavmeshZeroTest()
        {
            var settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            var input = new InputGeometry(() => { return zeroPlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.BuildAsync().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsTrue(walkable1);
            Assert.AreEqual(pointZero.XZ(), n1.Value.XZ());
            Assert.IsFalse(walkable2);
            Assert.IsNull(n2);
            Assert.IsFalse(walkable3);
            Assert.IsNull(n3);
        }

        [TestMethod()]
        public void BuildNavmeshOneTest()
        {
            var settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            var input = new InputGeometry(() => { return hOnePlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.BuildAsync().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsFalse(walkable1);
            Assert.AreEqual(pointOne.XZ(), n1.Value.XZ());
            Assert.IsTrue(walkable2);
            Assert.AreEqual(pointOne.XZ(), n2.Value.XZ());
            Assert.IsFalse(walkable3);
            Assert.IsNull(n3);
        }

        [TestMethod()]
        public void BuildNavmeshTwoTest()
        {
            var settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            var input = new InputGeometry(() => { return hTwoPlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.BuildAsync().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsFalse(walkable1);
            Assert.AreEqual(pointTwo.XZ(), n1.Value.XZ());
            Assert.IsFalse(walkable2);
            Assert.AreEqual(pointTwo.XZ(), n2.Value.XZ());
            Assert.IsTrue(walkable3);
            Assert.AreEqual(pointTwo.XZ(), n3.Value.XZ());
        }

        [TestMethod()]
        public void BuildNavmeshSceneryTest()
        {
            var settings = BuildSettings.Default;
            settings.Agents = new[] { agentDefault };
            var input = new InputGeometry(() => { return sceneryTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.BuildAsync().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentDefault, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentDefault, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentDefault, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph);
            Assert.IsTrue(walkable1);
            Assert.AreEqual(pointZero.XZ(), n1.Value.XZ());
            Assert.IsTrue(walkable2);
            Assert.AreEqual(pointOne.XZ(), n2.Value.XZ());
            Assert.IsTrue(walkable3);
            Assert.AreEqual(pointTwo.XZ(), n3.Value.XZ());
        }

        [TestMethod()]
        public void BuildNavmeshInclinedTest()
        {
            var settings = BuildSettings.Default;
            settings.Agents = new[] { agentInclined };
            var input = new InputGeometry(() => { return inclinedPlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input);
            var graph = pfDesc.BuildAsync().GetAwaiter().GetResult();

            var walkable1 = graph.IsWalkable(agentInclined, pointZero, 0.5f, out var n1);
            var walkable2 = graph.IsWalkable(agentInclined, pointOne, 0.5f, out var n2);
            var walkable3 = graph.IsWalkable(agentInclined, pointTwo, 0.5f, out var n3);

            Assert.IsNotNull(graph, "Graph is null");
            Assert.IsTrue(walkable1, "Point zero spected to be walkable.");
            Assert.AreEqual(pointZero.XZ(), n1.Value.XZ(), "Point zero spected to be the nearest point.");
            Assert.IsFalse(walkable2, "Point one spected to be not walkable.");
            Assert.AreEqual(pointZero.XZ(), n2.Value.XZ(), "Point zero spected to be the nearest point.");
            Assert.IsFalse(walkable3, "Point two spected to be not walkable.");
            Assert.IsNull(n3, "No nearest point spected for point two");
        }
    }
}