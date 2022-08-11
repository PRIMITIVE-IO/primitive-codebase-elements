using FluentAssertions;
using PrimitiveCodebaseElements.Primitive;
using Xunit;

namespace PrimitiveCodebaseElements.Tests;

public class CodebaseElementNameTest
{
    [Fact]
    public void ShouldCreateClassNameFromFqnAndViceVersa()
    {
        ClassName.FromFqn("MyPackage.OuterClass$InnerClass", new FileName("some/path"))
            .FullyQualifiedName.Should().Be("MyPackage.OuterClass$InnerClass");

        ClassName.FromFqn("OuterClass$InnerClass", new FileName("some/path"))
            .FullyQualifiedName.Should().Be("OuterClass$InnerClass");

        ClassName.FromFqn("MyPackage.OuterClass$InnerClass$InnerInnerClass", new FileName("some/path"))
            .FullyQualifiedName.Should().Be("MyPackage.OuterClass$InnerClass$InnerInnerClass");

        ClassName.FromFqn("MyPackage.OuterClass", new FileName("some/path"))
            .FullyQualifiedName.Should().Be("MyPackage.OuterClass");
    }
    
    [Fact]
    public void ParentPackageShouldBeNotNull()
    {
        ClassName className = ClassName.FromFqn("OuterClass", new FileName("some/path"));
        className.ContainmentPackage.PackageNameString.Should().Be("");
        
        className
            .FullyQualifiedName.Should().Be("OuterClass");
    }

    [Fact]
    public void MethodFQN()
    {
        var className =  ClassName.FromFqn("MyPackage.OuterClass$InnerClass", new FileName("some/path"));
        var methodName = new MethodName(className, "f", "void", new[] { new Argument("x", TypeName.For("int")) });
        methodName.FullyQualifiedName.Should().Be("MyPackage.OuterClass$InnerClass.f(int)");
    }
}