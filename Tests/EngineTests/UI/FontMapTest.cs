using Engine;
using Engine.BuiltIn.Format;
using Engine.UI;
using Engine.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.UI
{
    [ExcludeFromCodeCoverage]
    [TestClass()]
    public class FontMapTest
    {
        static TestContext _testContext;

        const string p1 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Suspendisse sed elementum massa, ac porta nibh. Nullam bibendum id orci ut sollicitudin. Maecenas at augue venenatis, sollicitudin dui ac, pulvinar lacus. Fusce quis metus vitae lorem feugiat porttitor. Aliquam finibus nunc vel facilisis placerat. Duis molestie dignissim condimentum. Integer suscipit finibus dolor ac volutpat. Donec iaculis arcu ac erat tincidunt, non venenatis ligula pulvinar.";
        const string p2 = "Phasellus vulputate, ligula vel congue feugiat, risus mi maximus elit, nec elementum ligula est ac felis. Sed bibendum vel sem sed lobortis. Etiam magna tortor, sollicitudin non nisi sollicitudin, condimentum gravida ligula. Sed et massa ac libero gravida porta vitae a risus. Nulla facilisi. Phasellus nec bibendum odio, pulvinar consectetur est. Curabitur in dolor vitae risus laoreet mattis. Phasellus malesuada eget nulla sit amet maximus. Fusce egestas nulla nunc, ut volutpat quam auctor varius. Nullam ultrices ultrices feugiat.";
        const string p3 = "Sed laoreet nulla vel lobortis pharetra. Aliquam erat volutpat. Quisque mollis dui id metus viverra pharetra. In ac dignissim lectus, et pharetra est. Maecenas dignissim ut elit sit amet pretium. Cras elementum tortor id justo tristique scelerisque. Curabitur non tellus lectus. Pellentesque sem nulla, lacinia quis semper nec, bibendum sit amet metus.";
        const string p4 = "Vestibulum aliquam lectus eros, non sodales nisi efficitur ac. Curabitur lacinia arcu in tortor laoreet bibendum. Donec gravida in est sit amet placerat. Vestibulum vitae volutpat magna, id malesuada mi. Maecenas nisi tellus, viverra at nulla quis, rhoncus porttitor mauris. Nunc molestie imperdiet libero in fringilla. Phasellus lacinia imperdiet lectus sit amet fermentum. Nulla gravida odio risus, id luctus est volutpat sit amet. Donec a dignissim lectus. Integer suscipit sem eu justo cursus semper. Nulla turpis elit, suscipit sit amet lectus in, dignissim maximus elit. Cras imperdiet, elit vel consequat dictum, nibh diam interdum diam, ut malesuada sem justo at augue.";
        const string p5 = "Morbi at elit quis quam auctor congue eu a eros. Praesent aliquam erat mi, ut suscipit metus sagittis id. Phasellus at ligula ante. Curabitur at turpis tincidunt, pellentesque nisl in, consectetur risus. Vestibulum a sollicitudin mi. Vivamus cursus vitae diam at suscipit. Integer congue justo quis tempus sodales. Nullam porttitor sapien ac ante sagittis scelerisque. Vivamus tristique neque eget fermentum tincidunt. Sed molestie at odio id lobortis.";

        static readonly int p1Size = p1.Replace(Environment.NewLine, "").Replace(" ", "").Length;
        static readonly int p2Size = p2.Replace(Environment.NewLine, "").Replace(" ", "").Length;
        static readonly int p3Size = p3.Replace(Environment.NewLine, "").Replace(" ", "").Length;
        static readonly int p4Size = p4.Replace(Environment.NewLine, "").Replace(" ", "").Length;
        static readonly int p5Size = p5.Replace(Environment.NewLine, "").Replace(" ", "").Length;

        static Game game;
        static FontMap<VertexFont> font;

        const string fontImgResource = "Font.png";
        const string fontMapResource = "Font.txt";
        static FontMap<VertexFont> fontMapped;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testContext = context;
        }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{_testContext.TestName}'");

            WindowsExtensions.Startup();

            game = new Game("Unit testing game");

            font = FontMap<VertexFont>.FromFamily(game, FontMapKeycodeGenerator.Default(), "Arial", 10, FontMapStyles.Regular);

            var fontMapping = new FontMapping()
            {
                ImageFile = fontImgResource,
                MapFile = fontMapResource,
            };

            fontMapped = FontMap<VertexFont>.FromMap(game, "resources", fontMapping);
        }

        [TestMethod()]
        public void MapSentenceSimpleTest()
        {
            const string sample = "Hello world!";

            int sampleSize = sample.Replace(Environment.NewLine, "").Replace(" ", "").Length;

            var sentence = FontMapParser.ParseSentence(sample, Color4.White, Color4.White);

            var desc = FontMapSentenceDescriptor<VertexFont>.Create(100);

            font.MapSentence(sentence, false, 1000, ref desc);

            Assert.AreEqual(sampleSize * 4, (int)desc.VertexCount);
            Assert.AreEqual(sampleSize * 6, (int)desc.IndexCount);
            Assert.AreEqual(Vector3.Zero, desc.Vertices[0].Position);
        }
        [TestMethod()]
        public void MapSentenceLinesTest()
        {
            const string sample = @"Hello world!
I'm the next line";

            int sampleSize = sample.Replace(Environment.NewLine, "").Replace(" ", "").Length;

            var sentence = FontMapParser.ParseSentence(sample, Color4.White, Color4.White);

            var desc = FontMapSentenceDescriptor<VertexFont>.Create(100);

            font.MapSentence(sentence, false, 1000, ref desc);

            Assert.AreEqual(sampleSize * 4, (int)desc.VertexCount);
            Assert.AreEqual(sampleSize * 6, (int)desc.IndexCount);
            Assert.AreEqual(Vector3.Zero, desc.Vertices[0].Position);
        }
        [TestMethod()]
        public void MapSentenceParagrafsTest()
        {
            const string sample = @$"{p1}

{p2}

{p3}

{p4}

{p5}";

            int sampleSize = sample.Replace(Environment.NewLine, "").Replace(" ", "").Length;

            int height = (int)font.GetSpaceSize().Y;

            var sentence = FontMapParser.ParseSentence(sample, Color4.White, Color4.White);
            var desc = FontMapSentenceDescriptor<VertexFont>.Create(5000);
            font.MapSentence(sentence, false, float.MaxValue, ref desc);

            Assert.AreEqual(sampleSize * 4, (int)desc.VertexCount);
            Assert.AreEqual(sampleSize * 6, (int)desc.IndexCount);

            int index = 0;
            Assert.AreEqual(new Vector3(0, 0, 0), desc.Vertices[index].Position);
            index += p1Size * 4;
            Assert.AreEqual(new Vector3(0, -height, 0), desc.Vertices[index].Position);
            index += p2Size * 4;
            Assert.AreEqual(new Vector3(0, -height * 2, 0), desc.Vertices[index].Position);
            index += p3Size * 4;
            Assert.AreEqual(new Vector3(0, -height * 3, 0), desc.Vertices[index].Position);
            index += p4Size * 4;
            Assert.AreEqual(new Vector3(0, -height * 4, 0), desc.Vertices[index].Position);
            index += p5Size * 4;
            Assert.AreEqual(new Vector3(0, 0, 0), desc.Vertices[index].Position);
        }
        [TestMethod()]
        public void MapSentenceParagrafsInRectTest()
        {
            const string sample = @$"{p1}

{p2}

{p3}

{p4}

{p5}";

            int sampleSize = sample.Replace(Environment.NewLine, "").Replace(" ", "").Length;

            var sentence = FontMapParser.ParseSentence(sample, Color4.White, Color4.White);
            var desc = FontMapSentenceDescriptor<VertexFont>.Create(5000);
            font.MapSentence(sentence, false, 500, ref desc);

            Assert.AreEqual(sampleSize * 4, (int)desc.VertexCount);
            Assert.AreEqual(sampleSize * 6, (int)desc.IndexCount);

            int index = 0;
            Assert.AreEqual(0, desc.Vertices[index].Position.X);
            index += p1Size * 4;
            Assert.AreEqual(0, desc.Vertices[index].Position.X);
            index += p2Size * 4;
            Assert.AreEqual(0, desc.Vertices[index].Position.X);
            index += p3Size * 4;
            Assert.AreEqual(0, desc.Vertices[index].Position.X);
            index += p4Size * 4;
            Assert.AreEqual(0, desc.Vertices[index].Position.X);
            index += p5Size * 4;
            Assert.AreEqual(0, desc.Vertices[index].Position.X);

            var size = desc.GetSize();
            Assert.IsTrue(size.X <= 500 + font.GetSpaceSize().X);
        }
        [TestMethod()]
        public void MapSentenceMappedInRectTest()
        {
            const string sample = @"Letters by Mara";
            float width = 1000;

            int sampleSize = sample.Replace(Environment.NewLine, "").Replace(" ", "").Length;

            var sentence = FontMapParser.ParseSentence(sample, Color4.White, Color4.White);
            var desc = FontMapSentenceDescriptor<VertexFont>.Create(15);
            fontMapped.MapSentence(sentence, false, width, ref desc);

            Assert.AreEqual(sampleSize * 4, (int)desc.VertexCount);
            Assert.AreEqual(sampleSize * 6, (int)desc.IndexCount);
        }
        [TestMethod()]
        public void MapSentenceSizeErrorTest()
        {
            const string sample = "Hello world!";

            var sentence = FontMapParser.ParseSentence(sample, Color4.White, Color4.White);

            var desc = FontMapSentenceDescriptor<VertexFont>.Create(5);

            font.MapSentence(sentence, false, 1000, ref desc);

            Assert.AreEqual(5 * 4, (int)desc.VertexCount);
            Assert.AreEqual(5 * 6, (int)desc.IndexCount);
            Assert.AreEqual(Vector3.Zero, desc.Vertices[0].Position);
        }
    }
}
