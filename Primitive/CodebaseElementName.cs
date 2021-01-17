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
        [JsonIgnore] public string Serialized;

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
        
        public override int GetHashCode()
        {
            return hashCode;
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

        [JsonProperty] public readonly string ReturnType;

        [JsonProperty] public readonly IEnumerable<Argument> Arguments;

        public MethodName(
            CodebaseElementName parent,
            string methodName,
            string returnType,
            IEnumerable<Argument> argumentTypes) : base(methodName)
        {
            ContainmentParent = parent;
            ReturnType = returnType;

            Arguments = argumentTypes;
            string hashString = ShortName + ReturnType;
            foreach (Argument argument in Arguments)
            {
                hashString += argument.Name + argument.Type.Signature;
            }

            hashCode = ContainmentParent.GetHashCode() + hashString.GetHashCode();
            Serialize();
        }

        [JsonConstructor]
        MethodName(
            CodebaseElementName parent,
            string methodName,
            string returnType,
            IEnumerable<Argument> argumentTypes,
            bool extra) : base(methodName)
        {
            ContainmentParent = parent;
            ReturnType = returnType;
            Arguments = argumentTypes;
            Serialize();
        }

        public string ToJavaFullyQualified()
        {
            if (ContainmentParent is ClassName parentJavaClass &&
                !string.IsNullOrEmpty(parentJavaClass.ToJavaFullyQualified()))
            {
                string paramString = Arguments.Aggregate(
                    "", 
                    (current, argument) => current + (argument.Name + " " + argument.Type.Signature + ", "));

                // TODO 
                return $"{parentJavaClass.ToJavaFullyQualified()}" +
                       $"{ShortName}:({paramString}){ReturnType}";
            }

            return "Not Java";
        }

        public string ToCSharpFullyQualified()
        {
            if (ContainmentParent is ClassName parentCSharpClass &&
                !string.IsNullOrEmpty(parentCSharpClass.ToCSharpFullyQualified()))
            {
                string paramString = Arguments.Aggregate(
                    "", 
                    (current, argument) => current + (argument.Name + " " + argument.Type.Signature + ", "));
                
                // TODO 
                return $"{parentCSharpClass.ToCSharpFullyQualified()}." +
                       $"{ShortName}:({paramString}){ReturnType}";
            }

            return "Not C#";
        }

        public string ToCXFullyQualified()
        {
            if (ContainmentParent is ClassName parentCXClass &&
                !string.IsNullOrEmpty(parentCXClass.ToCXFullyQualified()))
            {
                string paramString = Arguments.Aggregate(
                    "", 
                    (current, argument) => current + (argument.Name + " " + argument.Type.Signature + ", "));
                
                // TODO 
                return $"{parentCXClass.ToCXFullyQualified()}." +
                       $"{ShortName}:({paramString}){ReturnType}";
            }

            return "Not CX";
        }
    }

    [PublicAPI]
    public sealed class FieldName : CodebaseElementName
    {
        public override CodebaseElementName ContainmentParent { get; }

        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Field;

        [JsonProperty] public readonly string FieldType;

        public FieldName(ClassName containmentClass, string fieldName, string fieldType) : base(fieldName)
        {
            ContainmentParent = containmentClass;
            FieldType = fieldType;
            
            hashCode = ContainmentParent.GetHashCode() + (ShortName + FieldType).GetHashCode();
            Serialize();
        }
        
        [JsonConstructor]
        FieldName(ClassName containmentClass, string fieldName, string fieldType,
            bool extra) : base(fieldName)
        {
            ContainmentParent = containmentClass;
            FieldType = fieldType;
            Serialize();
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

        public string ToCSharpFullyQualified()
        {
            if (ContainmentParent is ClassName parentCsClass &&
                !string.IsNullOrEmpty(parentCsClass.ToCSharpFullyQualified()))
            {
                // TODO 
                return $"{parentCsClass.ToCSharpFullyQualified()}" +
                       $"{ShortName}:{FieldType}";
            }

            return "Not C#";
        }
        
        public string ToCXFullyQualified()
        {
            if (ContainmentParent is ClassName parentCsClass &&
                !string.IsNullOrEmpty(parentCsClass.ToCSharpFullyQualified()))
            {
                // TODO 
                return $"{parentCsClass.ToCXFullyQualified()}" +
                       $"{ShortName}:{FieldType}";
            }

            return "Not CX";
        }
    }

    #endregion

    #region TYPES

    public abstract class TypeName : CodebaseElementName
    {
        public override CodebaseElementType CodebaseElementType =>
            CodebaseElementType.Unknown;

        public override CodebaseElementName ContainmentParent => null;

        [JsonProperty] public string Signature;

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
            Signature = shortName;
            hashCode = Signature.GetHashCode();
            Serialize();
        }

        [JsonConstructor]
        protected TypeName(string shortName, bool extra) : base(shortName)
        {
            // do nothing
        }
    }

    public sealed class ArrayTypeName : TypeName
    {
        public ArrayTypeName(string signature) : base(GetShortName(signature))
        {
            Signature = signature;
            hashCode = Signature.GetHashCode();
            Serialize();
        }

        static string GetShortName(string signature)
        {
            // Include a U+200A HAIR SPACE in order to ensure, no matter what font is used to render this name, the
            // braces don't join together visually.
            TypeName componentType = For(signature.Substring(0, signature.IndexOf('[')));
            return $"{componentType.ShortName}[\u200A]";
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

        public readonly string FullyQualified;

        PrimitiveTypeName(string fullyQualified, string shortName) : base(shortName)
        {
            FullyQualified = fullyQualified;
            hashCode = FullyQualified.GetHashCode();
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
        [JsonProperty] readonly FileName containmentFile;

        [JsonProperty] public readonly PackageName ContainmentPackage;

        [JsonProperty] public readonly bool IsOuterClass;

        [JsonProperty] public readonly ClassName ParentClass;

        [JsonProperty] public readonly string originalClassName;

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
                    className.Substring(0, className.LastIndexOf('$')));
            }
            else
            {
                IsOuterClass = true;
            }

            hashCode = ContainmentParent.GetHashCode() + originalClassName.GetHashCode();
            Serialize();
        }
        
        [JsonConstructor]
        ClassName(FileName containmentFile, PackageName containmentPackage, string className, bool extra)
            : base(GetShortName(className), extra)
        {
            this.containmentFile = containmentFile;
            ContainmentPackage = containmentPackage;
            Serialize();
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
        /// Lcom.example.package.OuterClass$InnerClass1$InnerClass2;
        /// </summary>
        public string ToJavaFullyQualified()
        {
            return $"L{ContainmentPackage.PackageNameString}.{originalClassName};";
        }

        public string ToCSharpFullyQualified()
        {
            return $"{ContainmentPackage.PackageNameString}.{originalClassName}";
        }

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
                ? new PackageName(filePath.Substring(
                    0,
                    filePath.LastIndexOf(separator)))
                : new PackageName();

            hashCode = FilePath.GetHashCode();
            Serialize();
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
        public PackageName() : base("")
        {
            PackageNameString = "";
            hashCode = 0;
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
            hashCode = !string.IsNullOrEmpty(packageNameString) 
                ? PackageNameString.GetHashCode() 
                : 0; // "" hash code does not evaluate to 0

            Serialize();
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
                    PackageNameString.Substring(
                        0,
                        PackageNameString.Length - ShortName.Length - 1));
            }

            // the parent of this package is the root
            return new PackageName();
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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            int? typeInt = jo["CodebaseElementType"]?.Value<int>();
            if (!typeInt.HasValue) return null;
            CodebaseElementType type = (CodebaseElementType) typeInt;
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

    #endregion
}