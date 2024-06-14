using Engine.Common;
using Engine.PathFinding.RecastNavigation;
using Engine.PathFinding.RecastNavigation.Detour.Crowds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Engine.PathFinding.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class CrowdsTests
    {
        static TestContext _testContext;

        const float frame = 1f / 60f;

        static readonly float hZero = 0;
        static readonly float hOne = 5;
        static readonly float rDefault = 0.5f;
        static readonly int maxAgents = 3;

        static IEnumerable<Triangle> zeroPlaneTris;
        static Graph graph;
        static GraphAgentType agentDefault;

        static readonly Vector3 p1Start = new(-8, 0, -8);
        static readonly Vector3 p2Start = new(-6, 0, -8);
        static readonly Vector3 p3Start = new(-8, 0, -6);
        static readonly Vector3 target = new(8, 0, 8);

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");

            var pZero = GeometryUtil.CreateXZPlane(20, 20, hZero);
            zeroPlaneTris = Triangle.ComputeTriangleList(pZero.Vertices, pZero.Indices);

            agentDefault = new GraphAgentType() { Radius = rDefault, Height = hOne * 0.5f };

            var settings = BuildSettings.Default;
            var input = new InputGeometry(() => { return zeroPlaneTris; });

            var pfDesc = new PathFinderDescription(settings, input, [agentDefault]);
            graph = pfDesc.BuildAsync().GetAwaiter().GetResult() as Graph;
        }

        [TestMethod()]
        public void CrowdAddAgents()
        {
            GroupManager<CrowdAgentSettings> crowdManager = new();

            var crowd = new Crowd(graph, new(agentDefault, maxAgents));
            crowdManager.Add(crowd);

            Assert.AreEqual(1, crowd.AddAgent(Vector3.Zero));
            Assert.AreEqual(1, crowd.Count());

            Assert.AreEqual(2, crowd.AddAgent(Vector3.Zero));
            Assert.AreEqual(2, crowd.Count());

            Assert.AreEqual(3, crowd.AddAgent(Vector3.Zero));
            Assert.AreEqual(3, crowd.Count());

            Assert.AreEqual(-1, crowd.AddAgent(Vector3.Zero));
            Assert.AreEqual(3, crowd.Count());
        }
        [TestMethod()]
        public void CrowdRemoveAgents()
        {
            GroupManager<CrowdAgentSettings> crowdManager = new();

            var crowd = new Crowd(graph, new(agentDefault, maxAgents));
            crowdManager.Add(crowd);

            Assert.AreEqual(1, crowd.AddAgent(Vector3.Zero));
            Assert.AreEqual(2, crowd.AddAgent(Vector3.Zero));
            Assert.AreEqual(3, crowd.AddAgent(Vector3.Zero));

            crowd.RemoveAgent(1);
            Assert.AreEqual(2, crowd.Count());
            Assert.AreEqual(2, crowd.GetPositions().Length);

            crowd.RemoveAgent(1);
            Assert.AreEqual(2, crowd.Count());
            Assert.AreEqual(2, crowd.GetPositions().Length);

            crowd.RemoveAgent(2);
            Assert.AreEqual(1, crowd.Count());
            Assert.AreEqual(1, crowd.GetPositions().Length);

            crowd.RemoveAgent(3);
            Assert.AreEqual(0, crowd.Count());
            Assert.AreEqual(0, crowd.GetPositions().Length);
        }

        [TestMethod()]
        public void CrowdGetPositions()
        {
            GroupManager<CrowdAgentSettings> crowdManager = new();

            var crowd = new Crowd(graph, new(agentDefault, maxAgents));
            crowdManager.Add(crowd);

            Assert.AreEqual(1, crowd.AddAgent(p1Start));
            Assert.AreEqual(1, crowd.GetPositions().Length);

            Assert.AreEqual(2, crowd.AddAgent(p2Start));
            Assert.AreEqual(2, crowd.GetPositions().Length);

            Assert.AreEqual(3, crowd.AddAgent(p3Start));
            Assert.AreEqual(3, crowd.GetPositions().Length);

            Assert.AreEqual(-1, crowd.AddAgent(p1Start));
            Assert.AreEqual(3, crowd.GetPositions().Length);

            Assert.AreEqual(p1Start.XZ(), crowd.GetPosition(1).XZ());
            Assert.AreEqual(p2Start.XZ(), crowd.GetPosition(2).XZ());
            Assert.AreEqual(p3Start.XZ(), crowd.GetPosition(3).XZ());
        }

        [TestMethod()]
        public void CrowdRequestMoveCrowd()
        {
            GroupManager<CrowdAgentSettings> crowdManager = new();

            var crowd = new Crowd(graph, new(agentDefault, maxAgents));
            crowdManager.Add(crowd);

            Assert.AreEqual(1, crowd.AddAgent(p1Start));
            Assert.AreEqual(2, crowd.AddAgent(p2Start));
            Assert.AreEqual(3, crowd.AddAgent(p3Start));

            var positions = crowd.GetPositions();
            Assert.AreEqual(3, positions.Length);
            Assert.AreEqual(p1Start.XZ(), positions[0].Position.XZ());
            Assert.AreEqual(p2Start.XZ(), positions[1].Position.XZ());
            Assert.AreEqual(p3Start.XZ(), positions[2].Position.XZ());

            crowd.RequestMove(target);
            Assert.AreEqual(p1Start.XZ(), positions[0].Position.XZ());
            Assert.AreEqual(p2Start.XZ(), positions[1].Position.XZ());
            Assert.AreEqual(p3Start.XZ(), positions[2].Position.XZ());

            var mockGameTime = new Mock<IGameTime>();
            mockGameTime.SetupGet(m => m.ElapsedSeconds).Returns(frame);

            crowd.Update(mockGameTime.Object);
            Assert.AreEqual(p1Start.XZ(), positions[0].Position.XZ());
            Assert.AreEqual(p2Start.XZ(), positions[1].Position.XZ());
            Assert.AreEqual(p3Start.XZ(), positions[2].Position.XZ());
        }
        [TestMethod()]
        public void CrowdRequestMoveAgent()
        {
            GroupManager<CrowdAgentSettings> crowdManager = new();

            var crowd = new Crowd(graph, new(agentDefault, maxAgents));
            crowdManager.Add(crowd);

            Assert.AreEqual(1, crowd.AddAgent(p1Start));
            Assert.AreEqual(2, crowd.AddAgent(p2Start));
            Assert.AreEqual(3, crowd.AddAgent(p3Start));

            var positions = crowd.GetPositions();
            Assert.AreEqual(3, positions.Length);
            Assert.AreEqual(p1Start.XZ(), positions[0].Position.XZ());
            Assert.AreEqual(p2Start.XZ(), positions[1].Position.XZ());
            Assert.AreEqual(p3Start.XZ(), positions[2].Position.XZ());

            crowd.RequestMove(1, target);
            Assert.AreEqual(p1Start.XZ(), positions[0].Position.XZ());
            Assert.AreEqual(p2Start.XZ(), positions[1].Position.XZ());
            Assert.AreEqual(p3Start.XZ(), positions[2].Position.XZ());

            var mockGameTime = new Mock<IGameTime>();
            mockGameTime.SetupGet(m => m.ElapsedSeconds).Returns(frame);

            crowd.Update(mockGameTime.Object);
            Assert.AreEqual(p1Start.XZ(), positions[0].Position.XZ());
            Assert.AreEqual(p2Start.XZ(), positions[1].Position.XZ());
            Assert.AreEqual(p3Start.XZ(), positions[2].Position.XZ());
        }
    }
}