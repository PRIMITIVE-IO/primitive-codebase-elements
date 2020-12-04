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
        /// <para>The full, unique value that identifies this name. Any two name objects with the same fully qualified
        /// string representation are equivalent.</para>
        ///
        /// <para>The fully qualified name, being unique across a codebase, is suitable for serialization purposes.</para>
        /// </summary>
        public abstract string FullyQualified { get; }

        /// <summary>
        /// A human-readable representation of the name. Examples are unqualified class names and method names without
        /// their declaring class.
        /// </summary>
        public abstract string ShortName { get; }

        /// <summary>
        /// The parent element in the containment hierarchy. In particular, packages don't "contain" one another, even
        /// though they are linked in their own hierarchy.
        /// </summary>
        /// <remarks>
        /// May be <c>null</c> if there is no containing element.
        /// </remarks>
        public abstract CodebaseElementName ContainmentParent { get; }

        public abstract string BranchName { get; set; }

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

        public virtual FileName ContainmentFile()
        {
            if (this is FileName) return this as FileName;
            if (ContainmentParent == null) return null;
            return ContainmentParent.ContainmentFile();
        }

        public override bool Equals(object obj) =>
            obj is CodebaseElementName name &&
            name.FullyQualified == FullyQualified;

        public static bool operator ==(
            CodebaseElementName a,
            CodebaseElementName b) =>
            (a?.FullyQualified == b?.FullyQualified);

        public static bool operator !=(
            CodebaseElementName a,
            CodebaseElementName b) => !(a == b);

        public override int GetHashCode() => FullyQualified.GetHashCode();

        static readonly Regex RegexWhitespace = new Regex(@"\s+");

        protected static string ReplaceWhitespace(string typeName) =>
            RegexWhitespace.Replace(typeName, "").Replace(",", ", ");
    }

    #endregion

    #region MEMBERS

    [PublicAPI]
    public sealed class MethodName : CodebaseElementName
    {
        public override string FullyQualified { get; }
        public readonly string JavaFullyQualified;
        public override string ShortName { get; }
        public override CodebaseElementName ContainmentParent { get; }
        public override string BranchName { get; set; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Method;

        string returnType;
        IEnumerable<Argument> arguments;

        public MethodName(
            CodebaseElementName parent,
            string methodName,
            string returnType,
            IEnumerable<Argument> argumentTypes)
        {
            ContainmentParent = parent;
            this.returnType = returnType;

            arguments = argumentTypes;

            if (parent is ClassName className && !string.IsNullOrEmpty(className.JavaFullyQualified))
            {
                JavaFullyQualified =
                    $"{className.JavaFullyQualified}" +
                    $"{methodName}";
                // TODO information loss -> this should include the params and the return type but they aren't solved fully
                //$"{methodName}:{paramString}){returnType}";
            }

            // To the user, constructors are identified by their declaring class' names.
            ShortName =
                methodName == "<init>"
                    ? ContainmentParent.ShortName
                    : methodName;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            FullyQualified = JsonConvert.SerializeObject(this, settings);
        }
    }

    [PublicAPI]
    public sealed class FieldName : CodebaseElementName
    {
        // Suppose we have a field:
        //
        //   - Declared in "com.example.DeclaringClass"
        //   - Named "fieldName"
        //   - Has type of "java.lang.Object"
        //
        // The fully-qualified name would be:
        //
        // dir1/dir2/filename.ext|my.class.package.DeclaringClass;fieldName:java.lang.Object

        public override string FullyQualified { get; }
        public override string ShortName { get; }
        public override CodebaseElementName ContainmentParent { get; }
        public override string BranchName { get; set; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Field;

        public FieldName(ClassName containmentClass, string fieldName, string fieldType)
        {
            ShortName = fieldName;
            ContainmentParent = containmentClass;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            FullyQualified = JsonConvert.SerializeObject(this, settings);
        }

        static string FieldJvmSignature(string fieldName, string typeName) =>
            $"{fieldName}:{typeName}";

        static string FieldFqn(string className, string jvmSignature) =>
            $"{className};{jvmSignature}";
    }

    #endregion

    #region TYPES

    public abstract class TypeName : CodebaseElementName
    {
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Unknown;

        public override CodebaseElementName ContainmentParent => null;

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
    }

    public sealed class ArrayTypeName : TypeName
    {
        public override string FullyQualified { get; }
        public override string ShortName { get; }
        public override string BranchName { get; set; }

        TypeName ComponentType { get; }

        public ArrayTypeName(string signature)
        {
            ComponentType = For(signature.Substring(0, signature.IndexOf('[')));
            FullyQualified = signature;

            // Include a U+200A HAIR SPACE in order to ensure, no matter what font is used to render this name, the
            // braces don't join together visually.
            ShortName = $"{ComponentType.ShortName}[\u200A]";
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

        public override string FullyQualified { get; }
        public override string ShortName { get; }
        public override string BranchName { get; set; }

        PrimitiveTypeName(string fullyQualified, string shortName)
        {
            FullyQualified = fullyQualified;
            ShortName = shortName;
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
    }

    #endregion

    #region CONTAINERS

    [PublicAPI]
    public sealed class ClassName : TypeName
    {
        public override string FullyQualified { get; }
        public readonly string JavaFullyQualified;
        public override string ShortName { get; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Class;

        public override CodebaseElementName ContainmentParent => IsOuterClass
            ? (CodebaseElementName) containmentFile
            : ParentClass;

        public override string BranchName { get; set; }

        public override FileName ContainmentFile()
        {
            return containmentFile ?? base.ContainmentFile();
        }

        // only used if set by constructor
        readonly FileName containmentFile;
        public readonly PackageName ContainmentPackage;

        public readonly bool IsOuterClass;
        public readonly ClassName ParentClass;

        // note: fullyQualified must look like:
        // dir1/dir2/filename.ext|my.class.package.OuterClass$InnerClass1$InnerClass2
        public ClassName(FileName containmentFile, PackageName containmentPackage, string className)
        {
            this.containmentFile = containmentFile;
            ContainmentPackage = containmentPackage;

            // only required to make the Java runtime trace match
            JavaFullyQualified = $"L{containmentPackage.FullyQualified}.{className};";

            if (className.Contains('$'))
            {
                IsOuterClass = false;
                string[] innerClassSplit = className.Split('$');
                ShortName = innerClassSplit.Last();

                ParentClass = new ClassName(
                    containmentFile,
                    containmentPackage,
                    className.Substring(0, className.LastIndexOf('$')));
            }
            else
            {
                IsOuterClass = true;
                ShortName = className;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
            FullyQualified = JsonConvert.SerializeObject(this, settings);
        }
    }

    [PublicAPI]
    public sealed class FileName : CodebaseElementName
    {
        public override string FullyQualified { get; }
        public override string ShortName { get; }

        public override CodebaseElementName ContainmentParent { get; }
        public override string BranchName { get; set; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.File;

        public FileName(string path)
        {
            FullyQualified = path;

            char separator = '/';
            if (IsLocalFile(NormalizedPath(path)))
            {
                separator = '\\';
            }

            ShortName = path.Contains(separator)
                ? path.Substring(path.LastIndexOf(separator) + 1)
                : path;

            ContainmentParent = path.Contains(separator)
                ? new PackageName(FullyQualified.Substring(
                    0,
                    FullyQualified.LastIndexOf(separator)))
                : new PackageName();
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
        public override string FullyQualified { get; }
        public override string ShortName { get; }

        public readonly PackageName ParentPackage;

        PackageName CreateParentPackage()
        {
            if (string.IsNullOrEmpty(FullyQualified))
            {
                // the parent of the root is the root
                return new PackageName();
            }

            if (FullyQualified.Length > ShortName.Length)
            {
                // the parent is the path above this package
                // e.g. com.org.package.child ->
                //   short name:  child
                //   parent:      com.org.package
                return new PackageName(
                    FullyQualified.Substring(
                        0,
                        FullyQualified.Length - ShortName.Length - 1));
            }

            // the parent of this package is the root
            return new PackageName();
        }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Package;

        // these are dead-ends
        public override CodebaseElementName ContainmentParent => null;
        public override string BranchName { get; set; }

        /// <summary>
        /// The root or zero package
        /// </summary>
        public PackageName()
        {
            FullyQualified = "";
            ShortName = "";
        }

        /// <summary>
        /// From a package or director path -> create a package name
        /// </summary>
        /// <param name="packageName">A package or directory path</param>
        public PackageName(string packageName)
        {
            FullyQualified = packageName;

            if (string.IsNullOrEmpty(packageName))
            {
                // root
                ShortName = "";
            }
            else if (!packageName.Contains('.') && !packageName.Contains('/'))
            {
                // top
                ShortName = packageName;
            }
            else if (packageName.Contains('.'))
            {
                // a compiler FQN
                ShortName = packageName.Substring(packageName.LastIndexOf('.') + 1);
            }
            else
            {
                // a path FQN
                ShortName = packageName.Substring(packageName.LastIndexOf('/') + 1);
            }

            ParentPackage = CreateParentPackage();
        }

        public List<PackageName> Lineage()
        {
            List<PackageName> lineage = string.IsNullOrEmpty(FullyQualified)
                ? new List<PackageName>()
                : ParentPackage.Lineage();

            lineage.Add(this);

            return lineage;
        }
    }

    #endregion
}