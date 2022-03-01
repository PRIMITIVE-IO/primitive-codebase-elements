using System.Collections.Generic;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive.dto
{
    [PublicAPI]
    public class FileDto
    {
        public readonly string Text;
        public readonly string Path;
        public readonly bool IsTest;
        public readonly List<ClassDto> Classes;
        public readonly SourceCodeLanguage Language;

        public FileDto(string text, string path, bool isTest, List<ClassDto> classes, SourceCodeLanguage language)
        {
            Text = text;
            Path = path;
            IsTest = isTest;
            Classes = classes;
            Language = language;
        }

        public FileDto With(List<ClassDto> classes)
        {
            return new FileDto(
                text: Text,
                path: Path,
                isTest: IsTest,
                classes: classes,
                language: Language
            );
        }
    }
}