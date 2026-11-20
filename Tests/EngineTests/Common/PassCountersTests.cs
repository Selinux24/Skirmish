using Engine.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.Common
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class PassCountersTests
    {
        [TestMethod]
        public void ToStringFormatsRegularPassCounters()
        {
            var counters = new PassCounters("Main", 0)
            {
                DrawCallsPerFrame = 1,
                InstancesPerFrame = 12,
                PrimitivesPerFrame = 34,
                RasterizerStateChanges = 5,
                DepthStencilClears = 6,
                DepthStencilStateChanges = 7,
                OMBlendStateChanges = 8,
            };

            Assert.AreEqual(
                "DrawCalls/Instances/Triangles => 001/0012/000000034 Rasterizer/DepthStencil/Blend => 05/13/08 ### Main",
                counters.ToString());
        }

        [TestMethod]
        public void ToStringFormatsCommandListCounters()
        {
            var counters = new PassCounters("CommandLists", -1)
            {
                ExecuteCommandLists = 3,
                ViewportsSets = 4,
                RenderTargetSets = 2,
                RenderTargetClears = 4,
                DepthStencilClears = 5,
                DepthStencilStateChanges = 4,
            };

            Assert.AreEqual(
                "ExecuteCommandLists => 03 Viewport/RenderTarget/DepthStencil => 04/06/09 ### CommandLists",
                counters.ToString());
        }
    }
}
