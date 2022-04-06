using System.IO;

namespace PrimitiveCodebaseElements.Primitive.db.util
{
    public static class Util
    {
        public static string SanitizePathSeparators(this string path)
        {
            return Path.GetDirectoryName(path).Replace('\\', '/');
        }
    }
}