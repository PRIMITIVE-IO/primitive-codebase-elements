using System.Collections.Generic;
using JetBrains.Annotations;

namespace PrimitiveCodebaseElements.Primitive
{
    [PublicAPI]
    public class ClassReferenceStructure
    {
        public readonly ClassName ClassName;
        public readonly ClassInfo ClassInfo;
        public readonly List<MethodReferenceStructure> Methods = new List<MethodReferenceStructure>();

        public readonly List<int> SolvedNodes = new List<int>();

        public readonly List<CodeRangeWithReference> OutboundUsageLinks = new List<CodeRangeWithReference>();
        public readonly List<CodeReferenceEndpoint> ReferencesFromThis = new List<CodeReferenceEndpoint>();

        public ClassReferenceStructure(ClassInfo classInfo)
        {
            ClassName = classInfo.className;
            foreach (MethodInfo methodInfo in classInfo.Methods)
            {
                Methods.Add(new MethodReferenceStructure(methodInfo));
            }
        }

        public ClassReferenceStructure(ClassInfo classInfo, List<MethodReferenceStructure> methodNodes)
        {
            ClassName = classInfo.className;
            ClassInfo = classInfo;
            Methods = methodNodes;
        }
    }

    [PublicAPI]
    public class MethodReferenceStructure
    {
        public readonly MethodName MethodName;
        public readonly MethodInfo MethodInfo;

        public readonly List<int> SolvedCsNodes = new List<int>();

        public readonly List<object> SolvedJavaNodes = new List<object>();
        public readonly Dictionary<object, object> DiscoveredNodes = new Dictionary<object, object>();

        public readonly List<CodeRangeWithReference> OutboundUsageLinks = new List<CodeRangeWithReference>();
        public readonly List<CodeReferenceEndpoint> ReferencesFromThis = new List<CodeReferenceEndpoint>();

        public readonly string MethodString;

        public MethodReferenceStructure(MethodInfo methodInfo)
        {
            MethodName = methodInfo.MethodName;
            MethodInfo = methodInfo;
        }

        public MethodReferenceStructure(
            MethodInfo methodInfo,
            Dictionary<object, object> discoveredNodes,
            string methodString)
        {
            MethodName = methodInfo.MethodName;
            MethodInfo = methodInfo;
            DiscoveredNodes = discoveredNodes;
            MethodString = methodString;
        }
    }
}