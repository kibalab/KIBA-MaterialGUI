using KIBA_.KIBAMaterialGUI.Editor.UI.Property;
using NUnit.Framework;

namespace KIBA_.KIBAMaterialGUI.Tests.Editor
{
    public sealed class ShaderAttributeArgumentParserTests
    {
        [Test]
        public void Split_TrimsWhitespaceAndQuotes()
        {
            var tokens = ShaderAttributeArgumentParser.Split(" Root , \"Child\" , 2 ");

            Assert.That(tokens, Is.EqualTo(new[] { "Root", "Child", "2" }));
        }
    }
}


