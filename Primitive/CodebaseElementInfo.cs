using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PrimitiveCodebaseElements.Primitive
{
    #region CODEBASE ELEMENT TOP LEVEL

    /// <summary>
    /// <para>A general set of access flags as defined throughout the Java class file format specification. Some flags
    /// apply to only certain types of structures, such as <c>ACC_STRICT</c> only applying to methods. However, whenever
    /// two flags apply to multiple structures, the underlying value is the same in each case.</para>
    ///
    /// <para>Note that only the flags that are used have corresponding properties, so this is not comprehensive by any
    /// means. For other languages, the native access flags have been mapped to match those defined for Java.</para>
    /// </summary>
    [Flags]
    [PublicAPI]
    public enum AccessFlags : uint
    {
        None = 0x0000,
        AccPublic = 0x0001,
        AccPrivate = 0x0002,
        AccProtected = 0x0004,
        AccStatic = 0x0008,
        AccFinal = 0x0010,
        AccSynchronized = 0x0020,
        AccVolatile = 0x0040,
        AccVarargs = 0x0080,
        AccInterface = 0x0200,
        AccAbstract = 0x0400,
        AccStrict = 0x0800,
        AccEnum = 0x4000
    }

    [PublicAPI]
    public static class AccessFlagsExtensions
    {
        /// <summary>
        /// Return the given access flags, with the specified ones unset,
        /// regardless of their original values.
        /// </summary>
        /// <param name="original">The original flags.</param>
        /// <param name="toRemove">The flags to unset in the original.</param>
        /// <returns></returns>
        public static AccessFlags Without(
            this AccessFlags original,
            AccessFlags toRemove)
        {
            return original & ~toRemove;
        }
    }

    [PublicAPI]
    public static class AccessFlagExtensions
    {
        static bool HasFlagSet(
            this AccessFlags allFlags,
            AccessFlags mask) => (allFlags & mask) != 0;

        public static bool IsPublic(this AccessFlags flags) =>
            flags.HasFlagSet(AccessFlags.AccPublic);

        public static bool IsAbstract(this AccessFlags flags) =>
            flags.HasFlagSet(AccessFlags.AccAbstract);
    }

    /// <summary>
    /// Information about classes/methods/fields within the user's program.
    /// </summary>
    public interface ICodebaseElementInfo
    {
        [JsonProperty] CodebaseElementName Name { get; }

        /// <summary>
        /// All the children of this codebase element. This property imposes a canonical order in the set of children,
        /// and the children are expected to be shown in that order in the world.
        /// </summary>
        [JsonProperty]
        List<ICodebaseElementInfo> Children { get; }

        [JsonProperty] List<CodeReferenceEndpoint> ReferencesToThis { get; }
        [JsonProperty] List<CodeReferenceEndpoint> ReferencesFromThis { get; }
        [JsonProperty] SourceCodeSnippet SourceCode { get; }

        [JsonIgnore] string Serialized { get; }

        void DiffAgainst(ICodebaseElementInfo branchInfo);
    }

    [PublicAPI]
    public static class CodebaseElementInfoExtensions
    {
        public static bool HasReferences(this ICodebaseElementInfo codebaseElementInfo)
        {
            return codebaseElementInfo.ReferencesToThis.Any() ||
                   codebaseElementInfo.ReferencesFromThis.Any();
        }

        /// <summary>
        /// A computed value representing how "long" the source code of the element whose code is present in this
        /// collection is. The reason for not using the sum of the individual lengths is there is often redundancy
        /// between multiple definitions of the same element, such as a partial method declaration vs. its
        /// implementation in C#. In these cases, it's better to report the length of the implementation as the length
        /// of the method's source code.
        /// </summary>
        public static int RepresentativeLength(
            this List<SourceCodeSnippet> sourceCodeSnippets)
        {
            return sourceCodeSnippets.Any()
                ? sourceCodeSnippets
                    .Select(snippet => snippet.Text.Length)
                    .Max()
                : 0;
        }
    }

    #endregion

    #region ICodebaseElementInfo IMPLEMENTATIONS

    [PublicAPI]
    public class PackageVector2
    {
        public float x;
        public float y;

        public PackageVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [PublicAPI]
    public class PackageInfo : ICodebaseElementInfo
    {
        public CodebaseElementName Name => PackageName;
        public PackageName PackageName { get; }

        /// <summary>
        /// <para>For packages, it's important to distinguish between children and sub-packages. The children are the
        /// elements directly inside this package, such as classes contained in the package. Meanwhile, sub-packages are
        /// other packages that are logically under this package, and don't count as children.</para>
        ///
        /// <para>When thinking about the visualization, children appear in the stack under a package, whereas
        /// sub-packages are laid out on the ground with connecting lines.</para>
        /// </summary>
        public List<ICodebaseElementInfo> Children { get; set; }

        // We can get away without modeling the parent and sub-packages currently. However, these relationships
        // ultimately belong here, so if needed, they should be added to this structure.

        // These are unused
        public List<CodeReferenceEndpoint> ReferencesToThis { get; }
        public List<CodeReferenceEndpoint> ReferencesFromThis { get; }
        public SourceCodeSnippet SourceCode { get; set; }

        public readonly bool IsTestPackage = false;
        public PackageVector2 InitialPosition = new PackageVector2(0, 0);

        public PackageInfo(
            PackageName name,
            List<ICodebaseElementInfo> children)
        {
            PackageName = name;
            Children = children;

            IsTestPackage = children.Any() && children
                .TrueForAll(child =>
                    child is ClassInfo classInfo &&
                    classInfo.IsTestClass);

            // Packages don't have references
            ReferencesToThis = new List<CodeReferenceEndpoint>();
            ReferencesFromThis = new List<CodeReferenceEndpoint>();
        }

        public void DiffAgainst(ICodebaseElementInfo branchInfo) { }

        [JsonIgnore]
        public string Serialized
        {
            get
            {
                if (string.IsNullOrEmpty(serialized))
                {
                    serialized = JsonConvert.SerializeObject(this, DefaultSerializerSettings.Default);
                }

                return serialized;
            }
        }

        [JsonIgnore] string serialized;
    }

    [PublicAPI]
    public class MethodInfo : ICodebaseElementInfo
    {
        public readonly MethodName MethodName;
        public CodebaseElementName Name => MethodName;
        public List<ICodebaseElementInfo> Children => new List<ICodebaseElementInfo>();

        public readonly AccessFlags accessFlags;
        public bool IsPublic => accessFlags.IsPublic();
        public bool IsPrivate => !accessFlags.IsPublic();
        public bool IsAbstract => accessFlags.IsAbstract();

        /// <summary>
        /// The most concrete class owning this method
        /// </summary>
        public readonly ClassName ParentClass;

        public readonly Argument[] Arguments;
        public readonly TypeName ReturnType;

        public FieldInfo Field { get; set; }
        public List<CodeReferenceEndpoint> ReferencesToThis { get; }
        public List<CodeReferenceEndpoint> ReferencesFromThis { get; }

        public SourceCodeSnippet SourceCode { get; set; }

        public int NumberOfRuntimeCalls;
        public int RuntimeExecutionMaxTime;
        public int SolvedCount = 0;
        public int InvocationCount = 0;

        public MethodInfo(
            MethodName methodName,
            AccessFlags accessFlags,
            ClassName parentClass,
            IEnumerable<Argument> arguments,
            TypeName returnType,
            SourceCodeSnippet sourceCode)
        {
            MethodName = methodName;
            this.accessFlags = accessFlags;
            ParentClass = parentClass;

            Arguments = arguments.ToArray();
            ReturnType = returnType;

            Field = null;
            ReferencesToThis = new List<CodeReferenceEndpoint>();
            ReferencesFromThis = new List<CodeReferenceEndpoint>();

            SourceCode = sourceCode;
        }

        bool Equals(MethodInfo other) => Equals(MethodName, other.MethodName);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MethodInfo) obj);
        }

        public override int GetHashCode() => MethodName?.GetHashCode() ?? 0;

        public void DiffAgainst(ICodebaseElementInfo branchInfo)
        {
            SourceCode.SetBranchText(branchInfo.SourceCode.Text);
        }

        [JsonIgnore]
        public string Serialized
        {
            get
            {
                if (string.IsNullOrEmpty(serialized))
                {
                    serialized = JsonConvert.SerializeObject(this, DefaultSerializerSettings.Default);
                }

                return serialized;
            }
        }

        [JsonIgnore] string serialized;
    }

    [PublicAPI]
    public class Argument
    {
        public readonly string Name;
        public readonly TypeName Type;

        public Argument(string name, TypeName type)
        {
            Name = name;
            Type = type;
        }
    }

    [PublicAPI]
    public class FieldInfo : ICodebaseElementInfo
    {
        //the most concrete class owning this field
        public readonly ClassName ParentClass;

        public readonly TypeName FieldType;
        public readonly FieldName FieldName;
        public CodebaseElementName Name => FieldName;
        public List<ICodebaseElementInfo> Children => new List<ICodebaseElementInfo>();

        public readonly AccessFlags AccessFlags;

        public readonly List<MethodInfo> Methods;
        public List<CodeReferenceEndpoint> ReferencesToThis { get; }
        public List<CodeReferenceEndpoint> ReferencesFromThis { get; }

        // Total up both getter and setter
        public int NumberOfRuntimeCalls =>
            Methods.Sum(methodInfo => methodInfo.NumberOfRuntimeCalls);

        public SourceCodeSnippet SourceCode { get; set; }

        [JsonIgnore]
        public string Serialized
        {
            get
            {
                if (string.IsNullOrEmpty(serialized))
                {
                    serialized = JsonConvert.SerializeObject(this, DefaultSerializerSettings.Default);
                }

                return serialized;
            }
        }

        [JsonIgnore] string serialized;


        public FieldInfo(
            FieldName fieldName,
            ClassName parentClass,
            AccessFlags accessFlags,
            SourceCodeSnippet sourceCode)
        {
            ParentClass = parentClass;
            FieldName = fieldName;
            FieldType = TypeName.For(FieldName.FieldType);
            AccessFlags = accessFlags;
            Methods = new List<MethodInfo>();
            ReferencesToThis = new List<CodeReferenceEndpoint>();
            ReferencesFromThis = new List<CodeReferenceEndpoint>();
            SourceCode = sourceCode;
        }

        bool Equals(FieldInfo other) => Equals(FieldName, other.FieldName);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FieldInfo) obj);
        }

        public override int GetHashCode() => FieldName?.GetHashCode() ?? 0;

        public void DiffAgainst(ICodebaseElementInfo branchInfo)
        {
            SourceCode.SetBranchText(branchInfo.SourceCode.Text);
        }
    }

    [PublicAPI]
    public class FileInfo : ICodebaseElementInfo
    {
        public readonly FileName FileName;
        public CodebaseElementName Name => FileName;
        public List<ICodebaseElementInfo> Children { get; }
        public List<CodeReferenceEndpoint> ReferencesToThis { get; }
        public List<CodeReferenceEndpoint> ReferencesFromThis { get; }
        public SourceCodeSnippet SourceCode { get; set; }

        public string SourceUrl { get; }
        public string LocalUrl { get; set; }
        public string BranchUrl { get; set; }

        public long Size => SourceCode?.Text.Length ?? 0;
        public readonly string FileExtension;

        public bool IsParsed = false;

        /// <summary>
        /// A fully readable file
        /// </summary>
        public FileInfo(FileName name, SourceCodeSnippet sourceText)
        {
            FileName = name;
            ReferencesToThis = new List<CodeReferenceEndpoint>();
            ReferencesFromThis = new List<CodeReferenceEndpoint>();
            SourceCode = sourceText;
            Children = new List<ICodebaseElementInfo>();
        }

        /// <summary>
        /// A file that has yet to be downloaded
        /// </summary>
        public FileInfo(FileName name, string ext, string sourceUrl = "", string localUrl = "")
        {
            FileName = name;
            ReferencesToThis = new List<CodeReferenceEndpoint>();
            ReferencesFromThis = new List<CodeReferenceEndpoint>();
            SourceUrl = sourceUrl;
            LocalUrl = localUrl;
            FileExtension = ext;
            Children = new List<ICodebaseElementInfo>();
        }

        public void DiffAgainst(ICodebaseElementInfo branchInfo)
        {
            SourceCode.SetBranchText(branchInfo.SourceCode.Text);
        }

        public void TruncateSource()
        {
            switch (FileExtension)
            {
                case ".java":
                case ".cs":
                case ".kt":
                    // empty source code for files in languages that have a 1:1 representation with their classes
                    int ind = SourceCode.Text.IndexOf("class", StringComparison.Ordinal);
                    if (ind < 0)
                    {
                        ind = SourceCode.Text.IndexOf("interface", StringComparison.Ordinal);
                    }

                    if (ind < 0)
                    {
                        ind = SourceCode.Text.IndexOf("enum", StringComparison.Ordinal);
                    }

                    if (ind >= 0)
                    {
                        string rest = SourceCode.Text.Substring(ind);
                        int newlineInd = rest.IndexOf('\n');

                        // substring header to first newline
                        SourceCode = new SourceCodeSnippet(
                            SourceCode.Text.Substring(0, ind + newlineInd),
                            FileExtension);
                    }

                    break;
                case ".c":
                case ".cpp":
                case ".cc":
                case ".m":
                case ".h":
                case ".hpp":
                case ".hxx":
                case ".py":
                case ".py3":
                case ".js":
                case ".jsx":
                    // replace the now parsed code with the file short name
                    SourceCode = new SourceCodeSnippet(
                        FileName.ShortName,
                        SourceCodeLanguage.PlainText);
                    break;
            }
        }

        [JsonIgnore]
        public string Serialized
        {
            get
            {
                if (string.IsNullOrEmpty(serialized))
                {
                    serialized = JsonConvert.SerializeObject(this, DefaultSerializerSettings.Default);
                }

                return serialized;
            }
        }

        [JsonIgnore] string serialized;
    }

    [Serializable]
    [PublicAPI]
    public class ClassInfo : ICodebaseElementInfo
    {
        public readonly ClassName className;
        public CodebaseElementName Name => className;
        public readonly AccessFlags accessFlags;

        public IEnumerable<ClassInfo> InnerClasses =>
            Children.OfType<ClassInfo>();

        public readonly bool IsTestClass;

        public IEnumerable<MethodInfo> Methods =>
            Children.OfType<MethodInfo>();

        public IEnumerable<FieldInfo> Fields =>
            Children.OfType<FieldInfo>();

        public List<ICodebaseElementInfo> Children { get; }

        public List<CodeReferenceEndpoint> ReferencesToThis { get; }
        public List<CodeReferenceEndpoint> ReferencesFromThis { get; }
        public SourceCodeSnippet SourceCode { get; set; }

        public ClassInfo(
            ClassName className,
            IEnumerable<MethodInfo> methods,
            IEnumerable<FieldInfo> fields,
            AccessFlags accessFlags,
            IEnumerable<ClassInfo> innerClasses,
            SourceCodeSnippet headerSource,
            bool isTestClass)
        {
            this.className = className;
            this.accessFlags = accessFlags;

            Children = new List<ICodebaseElementInfo>();
            Children.AddRange(fields);
            Children.AddRange(methods);
            Children.AddRange(innerClasses);

            ReferencesToThis = new List<CodeReferenceEndpoint>();
            ReferencesFromThis = new List<CodeReferenceEndpoint>();
            SourceCode = headerSource;
            IsTestClass = isTestClass;
        }

        public void DiffAgainst(ICodebaseElementInfo branchInfo)
        {
            SourceCode.SetBranchText(branchInfo.SourceCode.Text);

            foreach (ICodebaseElementInfo branchChild in branchInfo.Children)
            {
                foreach (ICodebaseElementInfo originalChild in Methods)
                {
                    if (originalChild.Name == branchChild.Name)
                    {
                        originalChild.DiffAgainst(branchChild);
                        break;
                    }
                }
            }
        }

        [JsonIgnore]
        public string Serialized
        {
            get
            {
                if (string.IsNullOrEmpty(serialized))
                {
                    serialized = JsonConvert.SerializeObject(this, DefaultSerializerSettings.Default);
                }

                return serialized;
            }
        }

        [JsonIgnore] string serialized;
    }

    #endregion

    #region CODE REFERENCE INFO

    /// <summary>
    /// One end of a code reference edge. This allows the flexibility of representing the full edge using two endpoints.
    /// </summary>
    [PublicAPI]
    public class CodeReferenceEndpoint
    {
        public readonly CodeReferenceType Type;
        public readonly CodebaseElementName Endpoint;

        /// <summary>
        /// Only outbound references have a CodeRange. All outbound references should have a code range even if the
        /// range is unknown (-1), but inbound references have no range at all (null).
        /// </summary>
        public CodeReferenceEndpoint(
            CodeReferenceType type,
            CodebaseElementName endpoint)
        {
            Type = type;
            Endpoint = endpoint;
        }
    }

    [PublicAPI]
    public enum CodeReferenceType
    {
        Undefined = -1,
        MethodCall = 0,
        ClassInheritance = 1,
        InterfaceImplementation = 2,
        MethodOverride = 3
    }

    [PublicAPI]
    public enum CodeReferenceDirection
    {
        Undefined = -1,
        Outbound = 0,
        Inbound = 1
    }

    /// <summary>
    /// Represents a classification of directed edges in the code reference graph. Consists of a type and direction.
    /// Does not capture the actual endpoints, as the edge classification is usually attached to one of the endpoints.
    /// </summary>
    [PublicAPI]
    public struct CodeReferenceEdgeType
    {
        public readonly CodeReferenceType Type;
        public readonly CodeReferenceDirection Direction;

        public CodeReferenceEdgeType(
            CodeReferenceType type,
            CodeReferenceDirection direction)
        {
            Type = type;
            Direction = direction;
        }

        #region Equality members

        bool Equals(CodeReferenceEdgeType other) =>
            Type == other.Type && Direction == other.Direction;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CodeReferenceEdgeType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // 397 is an arbitrary prime number always chosen by ReSharper for GetHashCode functions
                return ((int) Type * 397) ^ (int) Direction;
            }
        }

        public static bool operator ==(
            CodeReferenceEdgeType left,
            CodeReferenceEdgeType right) => Equals(left, right);

        public static bool operator !=(
            CodeReferenceEdgeType left,
            CodeReferenceEdgeType right) => !Equals(left, right);

        #endregion
    }

    #endregion
}