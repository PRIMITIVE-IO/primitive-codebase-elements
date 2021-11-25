using System;
using System.Collections.Generic;
using JetBrains.Annotations;

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
        //char index in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int StartIdx;
        //char index in file
        [Obsolete("CodeRange should be used instead")]
        public readonly int EndIdx;
        [Obsolete("Header should be extracted from file (FileDto.Text) using CodeRange")] 
        public readonly string Header;
        public readonly List<ClassReferenceDto> ReferencesFromThis;
        // line/column coordinates in file
        // nullable for backward compatibility. Should be Non-null after removing all Idx
        [CanBeNull] public readonly CodeRange CodeRange;

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
            CodeRange codeRange = default, 
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
            CodeRange = codeRange;
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
                codeRange: CodeRange,
                referencesFromThis: referencesFromThis ?? ReferencesFromThis
            );
        }
    }
}