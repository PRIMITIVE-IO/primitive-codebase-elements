using FluentAssertions;
using PrimitiveCodebaseElements.Primitive;
using Xunit;

namespace PrimitiveCodebaseElements.Tests
{
    public class SourceCodeSnippetTests
    {
        SourceCodeSnippet _snippet; 

        public SourceCodeSnippetTests()
        {
            _snippet = new SourceCodeSnippet("test text", SourceCodeLanguage.C);
        }
        [Fact]
        public void Test1()
        {
            string output = "test";

            //Ver
            output.Should().Be("test");
        }
    }
}
