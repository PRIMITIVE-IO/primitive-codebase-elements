#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace PrimitiveCodebaseElements.Primitive
{
    #region BASE

    /// <summary>
    /// Everything in this file is essentially a wrapper for a string. There are various different naming schemes for
    /// all the different aspects of code elements that we need to use, and this is for the purpose of unifying them.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(BaseConverter))]
    public abstract class CodebaseElementName
    {
        /// <summary>
        /// The JSON payload containing all information about this name.
        /// </summary>
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

        /// <summary>
        /// A human-readable representation of the name. Examples are unqualified class names and method names without
        /// their declaring class.
        /// </summary>
        [JsonProperty] public readonly string ShortName;

        /// <summary>
        /// Backing variable assigned in constructor
        /// </summary>
        [JsonProperty] protected int hashCode;

        /// <summary>
        /// The parent element in the containment hierarchy. In particular, packages don't "contain" one another, even
        /// though they are linked in their own hierarchy.
        /// </summary>
        /// <remarks>
        /// May be <c>null</c> if there is no containing element.
        /// </remarks>
        [JsonProperty]
        public abstract CodebaseElementName ContainmentParent { get; }

        /// <summary>
        /// The branch (i.e. version of the code) that this name belongs to.
        /// 
        /// This is re-assignable. 
        /// </summary>
        [JsonProperty]
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

        [JsonProperty] public abstract CodebaseElementType CodebaseElementType { get; }

        protected CodebaseElementName(string shortName)
        {
            ShortName = shortName;
        }

        public virtual FileName ContainmentFile()
        {
            if (this is FileName) return (FileName) this;
            return ContainmentParent == null 
                ? null 
                : ContainmentParent.ContainmentFile();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((CodebaseElementName)obj);
        }

        protected bool Equals(CodebaseElementName other)
        {
            return GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(
            CodebaseElementName a,
            CodebaseElementName b) =>
            a?.GetHashCode() == b?.GetHashCode();

        public static bool operator !=(
            CodebaseElementName a,
            CodebaseElementName b) => !(a == b);

        static readonly Regex RegexWhitespace = new Regex(@"\s+");

        protected static string ReplaceWhitespace(string typeName) =>
            RegexWhitespace.Replace(typeName, string.Empty).Replace(",", ", ");
    }

    #endregion

    #region MEMBERS

    [PublicAPI]
    public sealed class MethodName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent => containmentParent;
        [JsonProperty] readonly CodebaseElementName containmentParent;

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Method;

        [JsonProperty] public readonly string ReturnType;

        [JsonProperty] public readonly IEnumerable<Argument> Arguments;

        string? _fullyQualifiedName;

        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName ??= FullyQualified(); }
        }

        public MethodName(
            CodebaseElementName parent,
            string methodName,
            string returnType,
            IEnumerable<Argument> argumentTypes) : base(methodName)
        {
            containmentParent = parent;
            ReturnType = returnType;

            Arguments = argumentTypes;
            string hashString = ShortName + ReturnType;
            hashString = Arguments.Aggregate(hashString, 
                (current, argument) => current + (argument.Name + argument.Type.Signature));

            hashCode = ContainmentParent.GetHashCode() + hashString.GetHashCode();
        }

        [JsonConstructor]
        MethodName(
            CodebaseElementName parent,
            string methodName,
            string returnType,
            IEnumerable<Argument> argumentTypes,
            bool extra) : base(methodName)
        {
            containmentParent = parent;
            ReturnType = returnType;
            Arguments = argumentTypes;
        }

        string FullyQualified()
        {
            ClassName? parentClass = containmentParent as ClassName;
            string parentFqn = parentClass?.FullyQualifiedName ?? containmentParent.ShortName;
            return $"{parentFqn}.{ShortName}({CommaSeparatedArguments(false)})";
        }

        /// <summary>
        /// use field `FullyQualifiedName` instead
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public string ToJavaFullyQualified()
        {
            if (ContainmentParent is ClassName parentJavaClass &&
                !string.IsNullOrEmpty(parentJavaClass.ToJavaFullyQualified()))
            {
                return $"{parentJavaClass.ToJavaFullyQualified()}.{ShortName}({CommaSeparatedArguments(false)})";
            }

            return "Not Java";
        }

        /// <summary>
        /// use field `FullyQualifiedName` instead
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public string ToCXFullyQualified()
        {
            if (ContainmentParent is ClassName parentCXClass &&
                !string.IsNullOrEmpty(parentCXClass.ToCXFullyQualified()))
            {
                // TODO 
                return $"{parentCXClass.ToCXFullyQualified()}." +
                       $"{ShortName}:({CommaSeparatedArguments()}){ReturnType}";
            }

            return "Not CX";
        }

        string GetArgumentType(Argument argument)
        {
            if (argument.Type is ClassName argumentType)
            {
                if (!string.IsNullOrEmpty(argumentType.ContainmentPackage.PackageNameString))
                {
                    return $"{argumentType.ContainmentPackage.PackageNameString}.{argumentType.Signature}";
                }
            }

            return argument.Type.Signature;
        }

        string CommaSeparatedArguments(bool includeArgNames = true)
        {
            Func<Argument, string> argsWithoutNamesFunc = GetArgumentType;
            Func<Argument, string> argsWithNamesFunc = argument => $"{argument.Name} {GetArgumentType(argument)}";

            Func<Argument, string> argFunction = includeArgNames ? argsWithNamesFunc : argsWithoutNamesFunc;

            return string.Join(",", Arguments.Select(argFunction).ToList());
        }
    }

    [PublicAPI]
    public sealed class FieldName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent => containmentParent;
        [JsonProperty] readonly CodebaseElementName containmentParent;

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Field;

        [JsonProperty] public readonly string FieldType;

        string? _fullyQualifiedName;

        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName ??= FullyQualified(); }
        }

        public FieldName(ClassName containmentClass, string fieldName, string fieldType) : base(fieldName)
        {
            containmentParent = containmentClass;
            FieldType = fieldType;

            hashCode = ContainmentParent.GetHashCode() + (ShortName + FieldType).GetHashCode();
        }

        [JsonConstructor]
        FieldName(ClassName containmentClass, string fieldName, string fieldType,
            bool extra) : base(fieldName)
        {
            containmentParent = containmentClass;
            FieldType = fieldType;
        }

        string FullyQualified()
        {
            ClassName? parentClass = containmentParent as ClassName;
            string parentFqn = parentClass?.FullyQualifiedName ?? containmentParent.ShortName;
            return $"{parentFqn}.{ShortName}";
        }

        /// <summary>
        /// use field `FullyQualifiedName` instead
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public string ToJavaFullyQualified()
        {
            if (ContainmentParent is ClassName parentJavaClass &&
                !string.IsNullOrEmpty(parentJavaClass.ToJavaFullyQualified()))
            {
                // TODO 
                return $"{parentJavaClass.ToJavaFullyQualified()}.{ShortName}:{FieldType}";
            }

            return "Not Java";
        }

        /// <summary>
        /// use field `FullyQualifiedName` instead
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public string ToCXFullyQualified()
        {
            if (ContainmentParent is ClassName parentCsClass &&
                !string.IsNullOrEmpty(parentCsClass.ToCXFullyQualified()))
            {
                // TODO 
                return $"{parentCsClass.ToCXFullyQualified()}{ShortName}:{FieldType}";
            }

            return "Not CX";
        }
    }

    #endregion

    #region TYPES

    public abstract class TypeName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent => new PackageName();

        [JsonProperty] public string Signature;

        public static TypeName For(string signature)
        {
            if (signature.EndsWith("[]"))
            {
                return new ArrayTypeName(signature);
            }

            PrimitiveTypeName primitiveTypeName =
                PrimitiveTypeName.ForPrimitiveTypeSignature(signature);
            if (primitiveTypeName != null)
            {
                return primitiveTypeName;
            }

            string packageNameString = string.Empty;
            string classNameString = signature;
            if (signature.Contains('.'))
            {
                packageNameString = signature[..signature.LastIndexOf('.')];
                classNameString = signature[(signature.LastIndexOf('.') + 1)..];
            }

            PackageName packageName = new PackageName(packageNameString);

            return new ClassName(
                new FileName(string.Empty),
                packageName,
                classNameString);
        }

        protected TypeName(string shortName) : base(shortName)
        {
            Signature = shortName;
            hashCode = Signature.GetHashCode();
        }

        [JsonConstructor]
        protected TypeName(string shortName, bool extra) : base(shortName)
        {
            // do nothing
        }
    }

    public sealed class ArrayTypeName : TypeName
    {
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.ArrayType;

        public ArrayTypeName(string signature) : base(signature)
        {
            Signature = signature;
            hashCode = Signature.GetHashCode();
        }

        [JsonConstructor]
        ArrayTypeName(string signature, bool extra) : base(signature)
        {
            // do nothing
        }
    }

    [PublicAPI]
    public sealed class PrimitiveTypeName : TypeName
    {
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.PrimitiveType;

        PrimitiveTypeName(string signature) : base(signature)
        {
            Signature = signature;
            hashCode = Signature.GetHashCode();
        }

        [JsonConstructor]
        PrimitiveTypeName(string signature, bool extra) : base(signature)
        {
            // do nothing
        }

        internal static PrimitiveTypeName ForPrimitiveTypeSignature(string signature)
        {
            switch (signature.ToLowerInvariant())
            {
                case "v":
                case "void":
                case "bool":
                case "boolean":
                case "byte":
                case "char":
                case "short":
                case "int":
                case "integer":
                case "int16":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    return new PrimitiveTypeName(signature);
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
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Class;

        public override CodebaseElementName ContainmentParent => IsOuterClass
            ? (CodebaseElementName)containmentFile
            : ParentClass;

        public override FileName ContainmentFile()
        {
            return containmentFile ?? base.ContainmentFile();
        }

        // only used if set by constructor
        [JsonProperty] readonly FileName containmentFile;

        [JsonProperty] public readonly PackageName ContainmentPackage;

        [JsonProperty] public readonly bool IsOuterClass;

        [JsonProperty] public readonly ClassName? ParentClass;

        [JsonProperty] public readonly string originalClassName;
        string _fullyQualifiedName;

        public string FullyQualifiedName
        {
            get { return _fullyQualifiedName ??= FullyQualified(); }
        }

        public ClassName(FileName containmentFile, PackageName containmentPackage, string className)
            : base(GetShortName(className))
        {
            this.containmentFile = containmentFile;
            ContainmentPackage = containmentPackage;
            originalClassName = className;

            if (!string.IsNullOrEmpty(className) && className.Contains('$'))
            {
                IsOuterClass = false;
                ParentClass = new ClassName(
                    containmentFile,
                    containmentPackage,
                    className[..className.LastIndexOf('$')]);
            }
            else
            {
                IsOuterClass = true;
            }

            hashCode = ContainmentParent.GetHashCode() + originalClassName.GetHashCode();
        }

        public ClassName(
            FileName containmentFile,
            PackageName containmentPackage,
            ClassName? parentClass,
            string shortName,
            string fqn
        ) : base(shortName)
        {
            this.containmentFile = containmentFile;
            ContainmentPackage = containmentPackage;
            IsOuterClass = parentClass == null;
            ParentClass = parentClass;
            originalClassName = shortName;
            _fullyQualifiedName = fqn;
        }

        public static ClassName FromFqn(string fqn, FileName fileName)
        {
            string packageName = fqn.SubstringBeforeLastOr(".", string.Empty);
            ClassName? parentClassName = fqn.Contains('$') ? FromFqn(fqn.SubstringBeforeLast("$"), fileName) : null;
            string shortClassName = fqn.SubstringAfterLast(fqn.Contains('$') ? "$" : ".");

            return new ClassName(
                fileName,
                packageName != null ? new PackageName(packageName) : null,
                parentClassName,
                shortClassName,
                fqn
            );
        }

        [JsonConstructor]
        ClassName(FileName containmentFile, PackageName containmentPackage, string className, bool extra)
            : base(GetShortName(className), extra)
        {
            this.containmentFile = containmentFile;
            ContainmentPackage = containmentPackage;
        }

        static string GetShortName(string className)
        {
            if (!string.IsNullOrEmpty(className) && className.Contains('$'))
            {
                string[] innerClassSplit = className.Split('$');
                return innerClassSplit.Last();
            }

            return className;
        }

        /// <summary>
        /// Use field  `FullyQualifiedName` instead
        /// </summary>
        [Obsolete]
        public string ToJavaFullyQualified()
        {
            return $"{ContainmentPackage.PackageNameString}.{originalClassName}";
        }

        string FullyQualified()
        {
            ClassName? parentClassName = ContainmentParent as ClassName;
            if (parentClassName != null)
            {
                return $"{parentClassName.FullyQualified()}${originalClassName}";
            }

            string? pkg = ContainmentPackage.PackageNameString == string.Empty ? null : ContainmentPackage.PackageNameString;

            return IEnumerableUtils.EnumerableOfNotNull(pkg, originalClassName)
                .JoinToString(".");
        }

        /// <summary>
        /// Use field `FullyQualifiedName` instead
        /// </summary>
        [Obsolete]
        public string ToCXFullyQualified()
        {
            return $"{ContainmentPackage.PackageNameString}::{originalClassName}";
        }
    }

    [PublicAPI]
    public sealed class FileName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent { get; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.File;

        [JsonProperty] public readonly string FilePath;

        public FileName(string filePath) : base(GetShortName(filePath, GetSeparator(filePath)))
        {
            FilePath = filePath;
            char separator = GetSeparator(filePath);

            ContainmentParent = filePath.Contains(separator)
                ? new PackageName(filePath[..filePath.LastIndexOf(separator)])
                : new PackageName();

            hashCode = FilePath.GetHashCode();
        }

        static string GetShortName(string path, char separator)
        {
            return path.Contains(separator)
                ? path[(path.LastIndexOf(separator) + 1)..]
                : path;
        }

        static char GetSeparator(string path)
        {
            char separator = '/';
            if (path.Contains('\\'))
            {
                separator = '\\';
            }

            return separator;
        }
    }

    [PublicAPI]
    public sealed class PackageName : CodebaseElementName
    {
        [JsonProperty] public readonly string ParentPackage;

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Package;

        [JsonProperty] public readonly string PackageNameString;

        // these are dead-ends
        public override CodebaseElementName ContainmentParent => null;

        /// <summary>
        /// The root or zero package
        /// </summary>
        public PackageName() : base(string.Empty)
        {
            PackageNameString = string.Empty;
            hashCode = 0;
        }

        /// <summary>
        /// From a package or director path -> create a package name
        /// </summary>
        /// <param name="packageNameString">A package or directory path</param>
        public PackageName(string packageNameString) : base(GetShortName(packageNameString))
        {
            PackageNameString = packageNameString;
            ParentPackage = CreateParentPackage().PackageNameString;
            hashCode = !string.IsNullOrEmpty(packageNameString)
                ? PackageNameString.GetHashCode()
                : 0; // "" hash code does not evaluate to 0
        }

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
                    PackageNameString[..(PackageNameString.Length - ShortName.Length - 1)]);
            }

            // the parent of this package is the root
            return new PackageName();
        }

        static string GetShortName(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
            {
                // root
                return string.Empty;
            }

            if (!packageName.Contains('.') && !packageName.Contains('/') && !packageName.Contains('\\'))
            {
                // top
                return packageName;
            }

            if (packageName.Contains('.'))
            {
                // a compiler FQN
                return packageName[(packageName.LastIndexOf('.') + 1)..];
            }

            if (packageName.Contains('/'))
            {
                // a path FQN
                return packageName[(packageName.LastIndexOf('/') + 1)..];
            }

            return packageName[(packageName.LastIndexOf('\\') + 1)..];
        }
    }

    #endregion

    #region JSON Contract Resolution

    // see: https://stackoverflow.com/questions/20995865/deserializing-json-to-abstract-class

    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(CodebaseElementName).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class BaseConverter : JsonConverter
    {
        static readonly JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings
        {
            ContractResolver = new BaseSpecifiedConcreteClassConverter()
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CodebaseElementName);
        }

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object? existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            int? typeInt = jo["CodebaseElementType"]?.Value<int>();
            if (!typeInt.HasValue) return null;
            CodebaseElementType type = (CodebaseElementType)typeInt;
            switch (type)
            {
                case CodebaseElementType.Field:
                    return JsonConvert.DeserializeObject<FieldName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.Method:
                    return JsonConvert.DeserializeObject<MethodName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.Class:
                    return JsonConvert.DeserializeObject<ClassName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.Package:
                    return JsonConvert.DeserializeObject<PackageName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.File:
                    return JsonConvert.DeserializeObject<FileName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.PrimitiveType:
                    return JsonConvert.DeserializeObject<PrimitiveTypeName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.ArrayType:
                    return JsonConvert.DeserializeObject<ArrayTypeName>(jo.ToString(), SpecifiedSubclassConversion);
                case CodebaseElementType.Unknown:
                    break;
                default:
                    throw new Exception();
            }

            throw new NotImplementedException();
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // won't be called because CanWrite returns false
        }
    }

    public static class DefaultSerializerSettings
    {
        public static readonly JsonSerializerSettings Default = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };
    }

    #endregion
}