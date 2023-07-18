using System.Collections.Generic;


namespace PrimitiveCodebaseElements.Primitive.dto
{
    
    public class FileDto
    {
        public readonly string Text;
        public readonly string Path;
        public readonly bool IsTest;
        public readonly List<ClassDto> Classes;
        public readonly List<MethodDto> Functions;
        public readonly List<FieldDto> Fields;
        public readonly SourceCodeLanguage Language;

        public FileDto(string text, string path, bool isTest, List<ClassDto> classes, SourceCodeLanguage language,
            List<MethodDto> functions, List<FieldDto> fields)
        {
            Text = text;
            Path = path;
            IsTest = isTest;
            Classes = classes;
            Language = language;
            Functions = functions;
            Fields = fields;
        }

        public FileDto With(List<ClassDto> classes)
        {
            return new FileDto(
                text: Text,
                path: Path,
                isTest: IsTest,
                classes: classes,
                language: Language,
                functions: Functions,
                fields: Fields
            );
        }
    }
}