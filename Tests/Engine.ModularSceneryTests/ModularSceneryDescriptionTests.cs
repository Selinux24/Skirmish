using Engine.Content.FmtCollada.Fmt;
using Engine.Content.FmtObj.Fmt;
using Engine.Modular;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.ModularSceneryTests
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class ModularSceneryDescriptionTests
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            GameResourceManager.RegisterLoader<LoaderCollada>();
            GameResourceManager.RegisterLoader<LoaderObj>();
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
        }

        [TestMethod()]
        public async Task LoadFromFolderTest()
        {
            var desc = ModularSceneryDescription.FromFolder("resources", "assets.json", "assetsmap.json", "levels.json");

            Assert.IsNotNull(desc);

            var assetMap = desc.GetAssetMap();
            Assert.IsNotNull(assetMap);

            var levelMap = desc.GetLevelMap();
            Assert.IsNotNull(levelMap);

            var level = levelMap.Levels.First();
            var contentLibrary = await desc.ReadContentLibrary();

            var particles = desc.GetLevelParticleSystems();
            Assert.IsNotNull(particles);

            var levelAssets = desc.GetLevelAssets(level, contentLibrary);
            Assert.IsNotNull(levelAssets);

            var objectAssets = desc.GetLevelObjects(level, contentLibrary);
            Assert.IsNotNull(objectAssets);
        }
    }
}
