using System;
using FluentAssertions;
using PrimitiveCodebaseElements.Primitive;
using Xunit;
using CodeRange = PrimitiveCodebaseElements.Primitive.dto.CodeRange;

namespace PrimitiveCodebaseElements.Tests.dto
{
    public class CodeRangeTest
    {
        [Fact]
        public void CoderangeOfSingleLine()
        {
            var lines = @"
                12345
            ".TrimIndent2()
                .Split("\n");


            CodeRange.Of(1, 1, 1, 1).Of(lines).Should().Be("1");
            CodeRange.Of(1, 1, 1, 5).Of(lines).Should().Be("12345");
        }
        [Fact]
        public void CoderangeOfTwoLines(){
            var text = @"
                12345
                67890
            ".TrimIndent2();
            
            var lines = text
                .Split(Environment.NewLine);


            CodeRange.Of(1, 1, 2, 5).Of(lines).Should().Be(text);
            CodeRange.Of(1, 2, 2, 4).Of(lines).Should().Be(
                @"
                    2345
                    6789
                ".TrimIndent2());

            CodeRange.Of(2, 1, 2, 5).Of(lines).Should().Be("67890");
        }
        
        [Fact]
        public void CoderangeOfTreeLines(){
            var text = @"
                12345
                67890
                54321
            ".TrimIndent2();
            
            var lines = text
                .Split(Environment.NewLine);


            CodeRange.Of(1, 1, 3, 5).Of(lines).Should().Be(text);
            CodeRange.Of(1, 2, 3, 4).Of(lines).Should().Be(
                @"
                    2345
                    67890
                    5432
                ".TrimIndent2());

        }
        
        [Fact]
        public static void SubstringCodeRange()
        {
            CodeRange.Of(1, 1, 1, 1).Of("12345").Should().Be("1");
            CodeRange.Of(1, 1, 1, 2).Of("12345").Should().Be("12");
            CodeRange.Of(1, 1, 1, 5).Of("12345").Should().Be("12345");
            CodeRange.Of(1, 2, 1, 4).Of("12345").Should().Be("234");
            
            string s = @"
                12345
                67890
                12345
            ".TrimIndent2();
            
            CodeRange.Of(1, 5, 3, 1).Of(s).Should().Be("5\n67890\n1".PlatformSpecific());
            
            CodeRange.Of(3, 1, 3, 5).Of(s).Should().Be("12345");
            CodeRange.Of(3, 5, 3, 5).Of(s).Should().Be("5");

            CodeRange.Of(1, 1, 1, 2).Of("\r\n").Should().Be("\r\n");
        }

        [Fact]
        public static void CarrierReturn()
        {
            CodeRange.Of(1, 1, 1, 2).Of("12\r\n").Should().Be("12");
            CodeRange.Of(1, 1, 1, 3).Of("12\r\n").Should().Be("12\r");
            CodeRange.Of(1, 1, 1, 4).Of("12\r\n").Should().Be("12\r\n");
            CodeRange.Of(1, 1, 2, 2).Of("\r\n12\r\n").Should().Be("\r\n12");
            CodeRange.Of(1, 1, 2, 2).Of("12\r\n12\r\n").Should().Be("12\r\n12");
            CodeRange.Of(2, 1, 2, 2).Of("12\r\n22\r\n").Should().Be("22");
            CodeRange.Of(2, 2, 3, 3).Of("12\r\n22\r\n333").Should().Be("2\r\n333");
        }

        [Fact]
        public void ColumnCanBeOutOfBounds()
        {
            string txt = @"
                12345
                67890
            ".TrimIndent2();
            var lines = txt.Split(Environment.NewLine);

            CodeRange.Of(1, 1, 2, 6).Of(lines).Should().Be(txt);
            CodeRange.Of(1, 1, 1, 6).Of(lines).Should().Be("12345");
            
            

        }
    }
}
