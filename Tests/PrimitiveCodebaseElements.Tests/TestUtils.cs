using System.IO;
using System.Reflection;

namespace PrimitiveCodebaseElements.Tests;

public class TestUtils
{
    public static string Resource(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"PrimitiveCodebaseElements.Tests.Resources.{name}";
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }
}