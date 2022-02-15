using FluentAssertions;
using PrimitiveCodebaseElements.Primitive;
using Xunit;
using CodeRange = PrimitiveCodebaseElements.Primitive.dto.CodeRange;

namespace PrimitiveCodebaseElements.Tests.dto
{
    public class CodeRangeTest
    {
        [Fact]
        public static void SubstringCodeRange()
        {
            CodeRange.Of(1, 1, 1, 1).Of("12345").Should().Be("1");
            CodeRange.Of(1, 1, 1, 2).Of("12345").Should().Be("12");
            CodeRange.Of(1, 1, 1, 5).Of("12345").Should().Be("12345");
            CodeRange.Of(1, 2, 1, 4).Of("12345").Should().Be("234");
            
            var s = @"12345
                           67890
                           12345".TrimIndent();
            
            CodeRange.Of(1, 5, 3, 1).Of(s).Should().Be("5\n67890\n1");
            
            CodeRange.Of(3, 1, 3, 5).Of(s).Should().Be("12345");
            CodeRange.Of(3, 5, 3, 5).Of(s).Should().Be("5");

            CodeRange.Of(1, 1, 1, 2).Of("\r\n").Should().Be("\r\n");
        }
    }
}