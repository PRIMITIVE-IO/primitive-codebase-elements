using System.Collections.Generic;


namespace PrimitiveCodebaseElements.Primitive
{
    
    public class SourceCodeLanguageHelper
    {
        static readonly Dictionary<string, SourceCodeLanguage> ExtensionToLanguage =
            new()
            {
                {".java", SourceCodeLanguage.Java},
                {".cs", SourceCodeLanguage.CSharp},
                {".js", SourceCodeLanguage.JavaScript},
                {".jsx", SourceCodeLanguage.JavaScript},
                {".h", SourceCodeLanguage.Cpp},
                {".cpp", SourceCodeLanguage.Cpp},
                {".kt", SourceCodeLanguage.Kotlin},
                {".xml", SourceCodeLanguage.XML},
                {".sql", SourceCodeLanguage.SQL},
                {".md", SourceCodeLanguage.Markdown},
                {".html", SourceCodeLanguage.HTML},
                {".py", SourceCodeLanguage.Python},
                {".sc", SourceCodeLanguage.Scala},
                {".c", SourceCodeLanguage.C},
                {".cc", SourceCodeLanguage.Cpp},
                {".m", SourceCodeLanguage.Cpp},
                {".rs", SourceCodeLanguage.Rust},
                {".ts", SourceCodeLanguage.TypeScript},
                {".go", SourceCodeLanguage.Go},
                {".sol", SourceCodeLanguage.Solidity}
            };
        
        public static SourceCodeLanguage FromExtension(string ext)
        {
            return ExtensionToLanguage.GetValueOrDefault(ext, SourceCodeLanguage.PlainText);
        }
    }
    
    
    public enum SourceCodeLanguage
    {
        // These enum int values must match in:
        // - IntelliJ Plugin -> SQLiteOutput#dbValueForSourceCodeLanguage
        // - C# analyzer -> CsStructure#SourceCodeSnippetLanguage
        // - JS Analyzer -> sqliteOutput#JAVASCRIPT_LANGUAGE_TYPE
        PlainText = -1,

        // Core, widely-used languages
        Java = 0,
        CSharp = 1,
        JavaScript = 2,
        Cpp = 3,
        Kotlin = 4,
        XML = 5,
        SQL = 6,
        Markdown = 7,
        HTML = 8,
        Python = 9,
        Scala = 10,
        C = 11,
        CWithClasses = 12,
        ObjC = 13,
        Rust = 14,
        TypeScript = 15,
        Go = 16,

        // Non-core, specialized languages
        Solidity = 100,
    }
}