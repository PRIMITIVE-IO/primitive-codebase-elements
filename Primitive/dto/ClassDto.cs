using System.Collections.Generic;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    public class ClassDto
    {
        public readonly string Path;
        public readonly string PackageName;
        public readonly string Name;
        public readonly string FullyQualifiedName;
        public readonly List<MethodDto> Methods;
        public readonly List<FieldDto> Fields;
        public readonly AccessFlags Modifier;
        public readonly int StartIdx;
        public readonly int EndIdx;
        public readonly string Header;
        public readonly List<ClassReferenceDto> ReferencesFromThis;

        public ClassDto(
            string path,
            string packageName,
            string name,
            string fullyQualifiedName,
            List<MethodDto> methods,
            List<FieldDto> fields,
            AccessFlags modifier,
            int startIdx,
            int endIdx,
            string header,
            List<ClassReferenceDto> referencesFromThis = null)
        {
            Path = path;
            PackageName = packageName;
            Name = name;
            FullyQualifiedName = fullyQualifiedName;
            Methods = methods;
            Fields = fields;
            Modifier = modifier;
            StartIdx = startIdx;
            EndIdx = endIdx;
            Header = header;
            ReferencesFromThis = referencesFromThis ?? new List<ClassReferenceDto>();
        }

        public ClassDto With(List<MethodDto> methods = null, List<ClassReferenceDto> referencesFromThis = null)
        {
            return new ClassDto(
                path: Path,
                packageName: PackageName,
                name: Name,
                fullyQualifiedName: FullyQualifiedName,
                methods: methods ?? Methods,
                fields: Fields,
                modifier: Modifier,
                startIdx: StartIdx,
                endIdx: EndIdx,
                header: Header,
                referencesFromThis: referencesFromThis ?? ReferencesFromThis
            );
        }
    }
}