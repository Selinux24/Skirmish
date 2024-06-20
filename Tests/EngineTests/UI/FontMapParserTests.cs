using Engine.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EngineTests.UI
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FontMapParserTests
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

        [TestMethod]
        public void ParseSentenceEmptyTest()
        {
            // Arrange
            string text = string.Empty;
            Color4 defaultForeColor = new(1, 1, 1, 1); // Blanco
            Color4 defaultShadowColor = new(0, 0, 0, 1); // Negro

            // Act
            var result = FontMapParser.ParseSentence(text, defaultForeColor, defaultShadowColor);

            // Assert
            Assert.AreEqual(string.Empty, result.Text);
            Assert.AreEqual(0, result.Parts.Length);
            Assert.AreEqual(0, result.Colors.Length);
            Assert.AreEqual(0, result.ShadowColors.Length);
        }
        [TestMethod]
        public void ParseSentenceTest()
        {
            // Arrange
            string text = "Hello I'm your father";
            Color4 defaultForeColor = new(1, 1, 1, 1); // Blanco
            Color4 defaultShadowColor = new(0, 0, 0, 1); // Negro

            // Act
            var result = FontMapParser.ParseSentence(text, defaultForeColor, defaultShadowColor);

            // Assert
            Assert.AreEqual(text, result.Text);
            Assert.AreEqual(7, result.Parts.Length);
            Assert.AreEqual(7, result.Colors.Length);
            Assert.AreEqual(7, result.ShadowColors.Length);
            Assert.AreEqual("Hello", result.GetPart(0).Text);
            Assert.AreEqual(" ", result.GetPart(1).Text);
            Assert.AreEqual("I'm", result.GetPart(2).Text);
            Assert.AreEqual(" ", result.GetPart(3).Text);
            Assert.AreEqual("your", result.GetPart(4).Text);
            Assert.AreEqual(" ", result.GetPart(5).Text);
            Assert.AreEqual("father", result.GetPart(6).Text);
        }
        [TestMethod]
        public void ParseSentenceWithColorsTest()
        {
            // Arrange
            string text = $"{Color.Red}H{Color.White}ello";
            Color4 defaultForeColor = Color.White;
            Color4 defaultShadowColor = Color.Transparent;

            // Act
            var result = FontMapParser.ParseSentence(text, defaultForeColor, defaultShadowColor);

            // Assert
            Assert.AreEqual("Hello", result.Text);
            Assert.AreEqual(1, result.Parts.Length);

            Assert.AreEqual(1, result.Colors.Length);
            Assert.AreEqual(5, result.Colors[0].Length);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Red, Color.White, Color.White, Color.White, Color.White }, result.Colors[0]);
            Assert.AreEqual(1, result.ShadowColors.Length);
            Assert.AreEqual(5, result.ShadowColors[0].Length);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent }, result.ShadowColors[0]);

            var part = result.GetPart(0);
            Assert.AreEqual("Hello", part.Text);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Red, Color.White, Color.White, Color.White, Color.White }, part.Colors);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent }, part.ShadowColors);
        }
    }
}
