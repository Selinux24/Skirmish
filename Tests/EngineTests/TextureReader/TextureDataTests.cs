using Engine.TextureReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Vortice.Mathematics;

namespace EngineTests.TextureReader
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [OSCondition(OperatingSystems.Windows)]
    public class TextureDataTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ReadTextureCropReturnsRequestedDimensionsAndData()
        {
            var path = Path.Combine(TestContext.DeploymentDirectory, "Resources", "Font.png");

            using var texture = TextureData.ReadTexture(path, new RectI(0, 0, 1, 1));

            Assert.IsNotNull(texture);
            Assert.AreEqual(1u, texture.Width);
            Assert.AreEqual(1u, texture.Height);
            var pixels = texture.GetDataOffset(0, 0, out _);
            Assert.HasCount(4, pixels);
        }
    }
}
