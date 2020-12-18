using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace PrimitiveCodebaseElements.Primitive
{
    #region BASE

    /// <summary>
    /// Everything in this file is essentially a wrapper for a string. There are various different naming schemes for
    /// all the different aspects of code elements that we need to use, and this is for the purpose of unifying them.
    /// </summary>
    [PublicAPI]
    public abstract class CodebaseElementName
    {
        /// <summary>
        /// The JSON payload containing all information about this name.
        /// </summary>
        public string Serialized;

        /// <summary>
        /// A human-readable representation of the name. Examples are unqualified class names and method names without
        /// their declaring class.
        /// </summary>
        public readonly string ShortName;

        /// <summary>
        /// The parent element in the containment hierarchy. In particular, packages don't "contain" one another, even
        /// though they are linked in their own hierarchy.
        /// </summary>
        /// <remarks>
        /// May be <c>null</c> if there is no containing element.
        /// </remarks>
        public abstract CodebaseElementName ContainmentParent { get; }

        /// <summary>
        /// The branch (i.e. version of the code) that this name belongs to.
        /// 
        /// This is re-assignable. 
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// An element is also considered to contain itself.
        /// </summary>
        public bool IsContainedIn(CodebaseElementName container)
        {
            if (this == container) return true;

            return ContainmentParent != null &&
                   ContainmentParent.IsContainedIn(container);
        }

        public abstract CodebaseElementType CodebaseElementType { get; }

        protected CodebaseElementName(string shortName)
        {
            ShortName = shortName;
        }

        protected void Serialize()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            Serialized = JsonConvert.SerializeObject(this, settings);
        }

        public virtual FileName ContainmentFile()
        {
            if (this is FileName) return this as FileName;
            if (ContainmentParent == null) return null;
            return ContainmentParent.ContainmentFile();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CodebaseElementName) obj);
        }

        protected bool Equals(CodebaseElementName other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(
            CodebaseElementName a,
            CodebaseElementName b) =>
            (a?.GetHashCode() == b?.GetHashCode());

        public static bool operator !=(
            CodebaseElementName a,
            CodebaseElementName b) => !(a == b);

        static readonly Regex RegexWhitespace = new Regex(@"\s+");

        protected static string ReplaceWhitespace(string typeName) =>
            RegexWhitespace.Replace(typeName, "").Replace(",", ", ");
    }

    #endregion

    #region MEMBERS

    [PublicAPI]
    public sealed class MethodName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent { get; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Method;

        readonly string returnType;
        readonly IEnumerable<Argument> arguments;

        public MethodName(
            CodebaseElementName parent,
            string methodName,
            string returnType,
            IEnumerable<Argument> argumentTypes) : base(methodName)
        {
            ContainmentParent = parent;
            this.returnType = returnType;

            arguments = argumentTypes;
            Serialize();
        }

        public override int GetHashCode()
        {
            return ContainmentParent.GetHashCode() + (ShortName + arguments + returnType).GetHashCode();
        }

        public string ToJavaFullyQualified()
        {
            if (ContainmentParent is ClassName parentJavaClass &&
                !string.IsNullOrEmpty(parentJavaClass.ToJavaFullyQualified()))
            {
                // TODO 
                return $"{parentJavaClass.ToJavaFullyQualified()}" +
                       $"{ShortName}:{arguments}){returnType}";
            }

            return "Not Java";
        }
    }

    [PublicAPI]
    public sealed class FieldName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent { get; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Field;

        public readonly string FieldType;

        public FieldName(ClassName containmentClass, string fieldName, string fieldType) : base(fieldName)
        {
            ContainmentParent = containmentClass;
            FieldType = fieldType;
            Serialize();
        }

        public override int GetHashCode()
        {
            return ContainmentParent.GetHashCode() + (ShortName + FieldType).GetHashCode();
        }

        // Suppose we have a field:
        //
        //   - Declared in "com.example.DeclaringClass"
        //   - Named "fieldName"
        //   - Has type of "java.lang.Object"
        //
        // The fully-qualified name would be:
        // com.example.DeclaringClass;fieldName:java.lang.Object
        public string ToJavaFullyQualified()
        {
            if (ContainmentParent is ClassName parentJavaClass &&
                !string.IsNullOrEmpty(parentJavaClass.ToJavaFullyQualified()))
            {
                // TODO 
                return $"{parentJavaClass.ToJavaFullyQualified()}" +
                       $"{ShortName}:{FieldType}";
            }

            return "Not Java";
        }
    }

    #endregion

    #region TYPES

    public abstract class TypeName : CodebaseElementName
    {
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Unknown;

        public override CodebaseElementName ContainmentParent => null;
        readonly string signature;

        public static TypeName For(string signature)
        {
            if (signature.Contains('['))
            {
                return new ArrayTypeName(signature);
            }

            PrimitiveTypeName primitiveTypeName =
                PrimitiveTypeName.ForPrimitiveTypeSignature(signature);
            if (primitiveTypeName != null)
            {
                return primitiveTypeName;
            }

            string packageAndClass = signature;

            string packageNameString = "";
            string classNameString = packageAndClass;
            if (packageAndClass.Contains('.'))
            {
                packageNameString = packageAndClass.Substring(0, packageAndClass.LastIndexOf('.'));
                classNameString = packageAndClass.Substring(packageAndClass.LastIndexOf('.') + 1);
            }

            PackageName packageName = new PackageName(packageNameString);

            return new ClassName(
                new FileName(""),
                packageName,
                classNameString);
        }

        protected TypeName(string shortName) : base(shortName)
        {
            signature = shortName;
            Serialize();
        }

        public override int GetHashCode()
        {
            return signature.GetHashCode();
        }
    }

    public sealed class ArrayTypeName : TypeName
    {
        readonly string signature;

        public ArrayTypeName(string signature) : base(GetShortName(signature))
        {
            this.signature = signature;
            Serialize();
        }

        static string GetShortName(string signature)
        {
            // Include a U+200A HAIR SPACE in order to ensure, no matter what font is used to render this name, the
            // braces don't join together visually.
            TypeName componentType = For(signature.Substring(0, signature.IndexOf('[')));
            return $"{componentType.ShortName}[\u200A]";
        }

        public override int GetHashCode()
        {
            return signature.GetHashCode();
        }
    }

    [PublicAPI]
    public sealed class PrimitiveTypeName : TypeName
    {
        // The "short" names of primitive types are actually longer than the fully-qualified names, but it follows the
        // general pattern: the "short" is the "human-friendly" representation of the name, whereas the fully-qualified
        // name is the compiler-friendly version.

        public static readonly PrimitiveTypeName Boolean = new PrimitiveTypeName("Z", "boolean");
        public static readonly PrimitiveTypeName Int = new PrimitiveTypeName("I", "int");
        public static readonly PrimitiveTypeName Float = new PrimitiveTypeName("F", "float");
        public static readonly PrimitiveTypeName Void = new PrimitiveTypeName("V", "void");
        public static readonly PrimitiveTypeName Byte = new PrimitiveTypeName("B", "byte");
        public static readonly PrimitiveTypeName Char = new PrimitiveTypeName("C", "char");
        public static readonly PrimitiveTypeName Short = new PrimitiveTypeName("S", "short");
        public static readonly PrimitiveTypeName Long = new PrimitiveTypeName("J", "long");
        public static readonly PrimitiveTypeName Double = new PrimitiveTypeName("D", "double");

        readonly string fullyQualified;

        PrimitiveTypeName(string fullyQualified, string shortName) : base(shortName)
        {
            this.fullyQualified = fullyQualified;
            Serialize();
        }

        internal static PrimitiveTypeName ForPrimitiveTypeSignature(string signature)
        {
            switch (signature)
            {
                case "Z":
                    return Boolean;
                case "B":
                    return Byte;
                case "C":
                    return Char;
                case "S":
                    return Short;
                case "I":
                    return Int;
                case "J":
                    return Long;
                case "F":
                    return Float;
                case "D":
                    return Double;
                case "V":
                    return Void;
                default:
                    return null;
            }
        }

        public override int GetHashCode()
        {
            return fullyQualified.GetHashCode();
        }
    }

    #endregion

    #region CONTAINERS

    [PublicAPI]
    public sealed class ClassName : TypeName
    {
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Class;

        public override CodebaseElementName ContainmentParent => IsOuterClass
            ? (CodebaseElementName) containmentFile
            : ParentClass;

        public override FileName ContainmentFile()
        {
            return containmentFile ?? base.ContainmentFile();
        }

        // only used if set by constructor
        readonly FileName containmentFile;
        public readonly PackageName ContainmentPackage;

        public readonly bool IsOuterClass;
        public readonly ClassName ParentClass;
        readonly string originalClassName;

        public ClassName(FileName containmentFile, PackageName containmentPackage, string className)
            : base(GetShortName(className))
        {
            this.containmentFile = containmentFile;
            ContainmentPackage = containmentPackage;
            originalClassName = className;

            if (className.Contains('$'))
            {
                IsOuterClass = false;
                ParentClass = new ClassName(
                    containmentFile,
                    containmentPackage,
                    className.Substring(0, className.LastIndexOf('$')));
            }
            else
            {
                IsOuterClass = true;
            }
            
            Serialize();
        }

        static string GetShortName(string className)
        {
            if (className.Contains('$'))
            {
                string[] innerClassSplit = className.Split('$');
                return innerClassSplit.Last();
            }

            return className;
        }

        public override int GetHashCode()
        {
            return ContainmentParent.GetHashCode() + originalClassName.GetHashCode();
        }

        /// <summary>
        /// Lcom.example.package.OuterClass$InnerClass1$InnerClass2;
        /// </summary>
        public string ToJavaFullyQualified()
        {
            return $"L{ContainmentPackage}.{originalClassName};";
        }
    }

    [PublicAPI]
    public sealed class FileName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent { get; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.File;

        readonly string path;

        public FileName(string path) : base(GetShortName(path, GetSeparator(path)))
        {
            this.path = path;
            char separator = GetSeparator(path);

            ContainmentParent = path.Contains(separator)
                ? new PackageName(path.Substring(
                    0,
                    path.LastIndexOf(separator)))
                : new PackageName();
            
            Serialize();
        }

        public override int GetHashCode()
        {
            return path.GetHashCode();
        }

        static string GetShortName(string path, char separator)
        {
            return path.Contains(separator)
                ? path.Substring(path.LastIndexOf(separator) + 1)
                : path;
        }

        static char GetSeparator(string path)
        {
            char separator = '/';
            if (IsLocalFile(NormalizedPath(path)))
            {
                separator = '\\';
            }

            return separator;
        }

        static bool IsLocalFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            // see: https://docs.microsoft.com/en-us/dotnet/api/system.uri.scheme?view=netframework-4.5
            return uri.Scheme == "file";
        }

        static string NormalizedPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            return uri.IsAbsoluteUri
                ? uri.LocalPath
                : Path.GetFullPath(new Uri(Path.Combine(AppContext.BaseDirectory, path)).AbsolutePath);
        }
    }

    [PublicAPI]
    public sealed class PackageName : CodebaseElementName
    {
        public readonly string ParentPackage;

        PackageName CreateParentPackage()
        {
            if (string.IsNullOrEmpty(PackageNameString))
            {
                // the parent of the root is the root
                return new PackageName();
            }

            if (PackageNameString.Length > ShortName.Length)
            {
                // the parent is the path above this package
                // e.g. com.org.package.child ->
                //   short name:  child
                //   parent:      com.org.package
                return new PackageName(
                    PackageNameString.Substring(
                        0,
                        PackageNameString.Length - ShortName.Length - 1));
            }

            // the parent of this package is the root
            return new PackageName();
        }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Package;

        public readonly string PackageNameString;

        // these are dead-ends
        public override CodebaseElementName ContainmentParent => null;

        /// <summary>
        /// The root or zero package
        /// </summary>
        public PackageName() : base("")
        {
            PackageNameString = "";
            Serialize();
        }

        /// <summary>
        /// From a package or director path -> create a package name
        /// </summary>
        /// <param name="packageNameString">A package or directory path</param>
        public PackageName(string packageNameString) : base(GetShortName(packageNameString))
        {
            PackageNameString = packageNameString;
            ParentPackage = CreateParentPackage().PackageNameString;
            Serialize();
        }

        static string GetShortName(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                // root
                return "";
            }

            if (!packageName.Contains('.') && !packageName.Contains('/') && !packageName.Contains('\\'))
            {
                // top
                return packageName;
            }

            if (packageName.Contains('.'))
            {
                // a compiler FQN
                return packageName.Substring(packageName.LastIndexOf('.') + 1);
            }

            if (packageName.Contains('/'))
            {
                // a path FQN
                return packageName.Substring(packageName.LastIndexOf('/') + 1);
            }
            
            return packageName.Substring(packageName.LastIndexOf('\\') + 1); 
        }

        public override int GetHashCode()
        {
            return PackageNameString.GetHashCode();
        }
    }

    #endregion
}