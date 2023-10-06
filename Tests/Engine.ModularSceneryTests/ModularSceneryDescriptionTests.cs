using Engine.Modular;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Engine.ModularSceneryTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ModularSceneryDescriptionTests
    {
        static TestContext _testContext;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");
        }

        [TestMethod()]
        public void LoadFromFolderTest()
        {
            var desc = ModularSceneryDescription.FromFolder("resources", "assets.json", "assetsmap.json", "levels.json");

            Assert.IsNotNull(desc);

            var assetMap = desc.GetAssetMap();
            Assert.IsNotNull(assetMap);

            var levelMap = desc.GetLevelMap();
            Assert.IsNotNull(levelMap);
        }
    }
}
