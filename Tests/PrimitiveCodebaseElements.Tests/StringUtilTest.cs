using FluentAssertions;
using PrimitiveCodebaseElements.Primitive;
using PrimitiveCodebaseElements.Primitive.dto;
using Xunit;
using CodeRange = PrimitiveCodebaseElements.Primitive.dto.CodeRange;

namespace PrimitiveCodebaseElements.Tests
{
    public class StringUtilTest
    {
        [Fact]
        public static void LocationOf()
        {
            "12345".LocationOf('3').Should().Be(new CodeLocation(1, 3));

            @"12345
              67890
              12345".TrimIndent().LocationOf('8').Should().Be(new CodeLocation(2, 3));
        }

        [Fact]
        public static void LocationIn()
        {
            "12345".LocationIn(CodeRange.Of(1, 1, 1, 5), '3').Should().Be(new CodeLocation(1, 3));

            @"12345
              67890
              12345".TrimIndent().LocationIn(CodeRange.Of(1, 1, 2, 5), '8').Should().Be(new CodeLocation(2, 3));
        }

        [Fact]
        public static void SubstringAfterTest()
        {
            "a".SubstringAfter("b").Should().Be("a");
            "struct Books".SubstringAfter("struct").Should().Be(" Books");
        }
        
        [Fact]
        public static void SubstringBeforeLastTest()
        {
            "abc".SubstringBeforeLast("a").Should().Be("");
            "abc".SubstringBeforeLast("b").Should().Be("a");
            "abc".SubstringBeforeLast("c").Should().Be("ab");
            "abc".SubstringBeforeLast("d").Should().Be("abc");
            "abac".SubstringBeforeLast("a").Should().Be("ab");
        }
        
        [Fact]
        public static void SubstringBeforeTest()
        {
            "abc".SubstringBefore("a").Should().Be("");
            "abc".SubstringBefore("b").Should().Be("a");
            "abc".SubstringBefore("c").Should().Be("ab");
            "abc".SubstringBefore("d").Should().Be("abc");
            "abac".SubstringBefore("a").Should().Be("");
        }

    }
}