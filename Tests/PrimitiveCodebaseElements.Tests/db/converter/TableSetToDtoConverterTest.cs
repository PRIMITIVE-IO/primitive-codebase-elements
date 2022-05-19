using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PrimitiveCodebaseElements.Primitive.db;
using PrimitiveCodebaseElements.Primitive.db.converter;
using PrimitiveCodebaseElements.Primitive.dto;
using Xunit;

namespace PrimitiveCodebaseElements.Tests.db.converter;

public class TableSetToDtoConverterTest
{
    [Fact]
    public void SmokeTest()
    {
        List<FileDto> fileDtos = TableSetToDtoConverter.ToFileDto(new TableSet(
            arguments: new List<DbArgument>
            {
                new DbArgument(132, 432, 0, "x", 543),
                new DbArgument(133, 433, 0, "y", 544),
            },
            methods: new List<DbMethod>
            {
                new DbMethod(432, 1, 423, "f", 544, 0, 1, 10),
                new DbMethod(433, 1, 423, "g", 545, 0, 1, 10)
            },
            classes: new List<DbClass> { new DbClass(423, 1, 525, "some.Cls", 0, 1, 0) },
            files: new List<DbFile> { new DbFile(525, 283, "Cls.sc", "some/path", "", 1) },
            directories: new List<DbDirectory> { new DbDirectory(283, "some", 1, 1) },
            types: new List<DbType>
            {
                new DbType(543, "int"),
                new DbType(544, "string"),
                new DbType(545, "void")
            },
            methodReferences: new List<DbMethodReference>
            {
                new DbMethodReference(121, 1, 432, 433, 1, 1, 1, 1, 1, 1, 1, 1),
                new DbMethodReference(122, 1, 433, 432, 1, 1, 1, 1, 1, 1, 1, 1),
            },
            sourceIndices: new List<DbSourceIndex>
            {
                new DbSourceIndex(432, 525, "METHOD", 1, 1, 1, 1, 1, 1),
                new DbSourceIndex(433, 525, "METHOD", 2, 2, 2, 2, 2, 2),
                new DbSourceIndex(423, 525, "CLASS", 2, 2, 2, 2, 2, 2),
            }
        ));

        fileDtos[0].Classes[0].Methods.Should().HaveCount(2);
        
        MethodDto fMethod = fileDtos[0].Classes[0].Methods.Single(x => x.Name == "f");
        fMethod.Arguments.Should().HaveCount(1);
        fMethod.Arguments[0].Name.Should().Be("x");
        fMethod.Arguments[0].Type.Should().Be("int");
        fMethod.Signature.Should().Be("some.Cls.f(int)");
        fMethod.ReturnType.Should().Be("string");
        
        List<MethodReferenceDto> fRefs = fMethod.MethodReferences;
        fRefs.Should().HaveCount(1);
        fRefs[0].FromMethodSignature.Should().Be("some.Cls.f(int)");
        fRefs[0].ToMethodSignature.Should().Be("some.Cls.g(string)");

        MethodDto gMethod = fileDtos[0].Classes[0].Methods.Single(x => x.Name == "g");
        gMethod.Arguments.Should().HaveCount(1);
        gMethod.Arguments[0].Name.Should().Be("y");
        gMethod.Arguments[0].Type.Should().Be("string");
        gMethod.Signature.Should().Be("some.Cls.g(string)");
        gMethod.ReturnType.Should().Be("void");
        
        List<MethodReferenceDto> gRefs = gMethod.MethodReferences;
        gRefs.Should().HaveCount(1);
        gRefs[0].FromMethodSignature.Should().Be("some.Cls.g(string)");
        gRefs[0].ToMethodSignature.Should().Be("some.Cls.f(int)");
    }
}