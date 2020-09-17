﻿using System.Collections.Generic;
using System.Linq;

namespace PrimitiveCodebaseElements.Primitive
{
    public class SourceCodeSnippet
    {
        readonly string text;
        readonly SourceCodeLanguage language;
        public readonly List<CodeRangeWithReference> OutboundUsageLinks =
            new List<CodeRangeWithReference>();

        bool hasLoadedColorizedText;
        string colorizedText;
        TextSpanWithReference[] selectableTokens;

        static readonly Dictionary<string, SourceCodeLanguage> ExtensionToLanguage =
            new Dictionary<string, SourceCodeLanguage>
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

                {".sol", SourceCodeLanguage.Solidity}
            };

        public string SyntaxColoredText
        {
            get
            {
                if (string.IsNullOrEmpty(text))
                {
                    return "<i><color=#404040>no source available</color></i>";
                }
                
                LoadColorSyntaxAndTokens();
                return colorizedText;
            }
        }

        public IEnumerable<TextSpanWithReference> SelectableTokens
        {
            get
            {
                LoadColorSyntaxAndTokens();
                // this is an additional check to protect against empty spans
                return selectableTokens.Where(span => span.End > span.Start); 
            }
        }

        public SourceCodeSnippet(
            string text,
            SourceCodeLanguage language)
        {
            this.text = text;
            this.language = language;
        }

        public static SourceCodeSnippet Synthesized(string text) =>
            new SourceCodeSnippet(text, SourceCodeLanguage.PlainText);

        public SourceCodeSnippet(
            string text,
            string extension)
        {
            this.text = text;
            bool success = ExtensionToLanguage.TryGetValue(extension, out language);
            if (!success)
            {
                language = SourceCodeLanguage.PlainText;
            }
        }

        void LoadColorSyntaxAndTokens()
        {
            if (hasLoadedColorizedText) return;

            GenerateSelectableTokens();
            ColorTextBySyntax();

            hasLoadedColorizedText = true;
        }

        void ColorTextBySyntax()
        {
            
        }

        void GenerateSelectableTokens()
        {
            int lastStart = int.MaxValue;
            int lastEnd = -1;
            selectableTokens = new TextSpanWithReference[OutboundUsageLinks.Count];
            for (int ii = OutboundUsageLinks.Count - 1; ii >= 0; ii--)
            {
                CodeRangeWithReference link = OutboundUsageLinks[ii];
                if (lastStart <= link.End)
                {
                    if (lastStart > link.Start + 1)
                    {
                        selectableTokens[ii] = new TextSpanWithReference(
                            link.Start,
                            lastStart - 1,
                            link.Reference);
                    }
                    else
                    {
                        // special case
                        selectableTokens[ii] = new TextSpanWithReference(
                            lastEnd + 1,
                            link.End,
                            link.Reference);
                    }
                }
                else
                {
                    selectableTokens[ii] = new TextSpanWithReference(
                        link.Start,
                        link.End,
                        link.Reference);
                }

                lastStart = link.Start;
                lastEnd = link.End;
            }
        }

        public string Text => string.IsNullOrEmpty(text)
            ? ""
            : text;

        public IEnumerable<TextSpanWithReference> SpanOfReference(CodebaseElementName target) => SelectableTokens
            .Where(span => span.Reference == target)
            .ToList();
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

        // Non-core, specialized languages
        Solidity = 100
    }
}