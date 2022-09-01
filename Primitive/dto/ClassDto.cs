using System.Collections.Generic;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class ClassDto
    {
        public readonly string Path;
        public readonly string? PackageName;
        public readonly string Name;
        public readonly string FullyQualifiedName;
        public readonly List<MethodDto> Methods;
        public readonly List<FieldDto> Fields;
        public readonly AccessFlags Modifier;
        public readonly List<ClassReferenceDto> ReferencesFromThis;
        // line/column coordinates in file
        public readonly CodeRange CodeRange;
        // Is used for restore parent-child relationship, avoiding parsing class separators ("$") in class FQN.
        // The problem with parsing FQNs is in fake file-classes: their FQN does not contain a namespace (unlike their
        // child classes), so it is hard to find fake-class FQN based on "nested" class FQN. 
        public readonly string? ParentClassFqn;

        public ClassDto(
            string path,
            string? packageName,
            string name,
            string fullyQualifiedName,
            List<MethodDto> methods,
            List<FieldDto> fields,
            AccessFlags modifier,
            CodeRange codeRange, 
            List<ClassReferenceDto> referencesFromThis,
            string? parentClassFqn)
        {
            Path = path;
            PackageName = packageName;
            Name = name;
            FullyQualifiedName = fullyQualifiedName;
            Methods = methods;
            Fields = fields;
            Modifier = modifier;
            CodeRange = codeRange;
            ReferencesFromThis = referencesFromThis;
            ParentClassFqn = parentClassFqn;
        }

        public ClassDto With(
            List<MethodDto>? methods = null, 
            List<ClassReferenceDto>? referencesFromThis = null,
            string? parentClassFqn = null)
        {
            return new ClassDto(
                path: Path,
                packageName: PackageName,
                name: Name,
                fullyQualifiedName: FullyQualifiedName,
                methods: methods ?? Methods,
                fields: Fields,
                modifier: Modifier,
                codeRange: CodeRange,
                referencesFromThis: referencesFromThis ?? ReferencesFromThis,
                parentClassFqn: parentClassFqn ?? ParentClassFqn
            );
        }
    }
}