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
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Console.WriteLine($"TestContext.TestName='{TestContext.TestName}'");
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
            Assert.IsEmpty(result.Parts);
            Assert.IsEmpty(result.Colors);
            Assert.IsEmpty(result.ShadowColors);
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
            Assert.HasCount(7, result.Parts);
            Assert.HasCount(7, result.Colors);
            Assert.HasCount(7, result.ShadowColors);
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
            Assert.HasCount(1, result.Parts);

            Assert.HasCount(1, result.Colors);
            Assert.HasCount(5, result.Colors[0]);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Red, Color.White, Color.White, Color.White, Color.White }, result.Colors[0]);
            Assert.HasCount(1, result.ShadowColors);
            Assert.HasCount(5, result.ShadowColors[0]);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent }, result.ShadowColors[0]);

            var part = result.GetPart(0);
            Assert.AreEqual("Hello", part.Text);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Red, Color.White, Color.White, Color.White, Color.White }, part.Colors);
            CollectionAssert.AreEquivalent(new Color4[] { Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent }, part.ShadowColors);
        }
    }
}
