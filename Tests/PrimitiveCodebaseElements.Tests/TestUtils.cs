using System;
using System.IO;
using System.Reflection;

namespace PrimitiveCodebaseElements.Tests;

public static class TestUtils
{
    public static string Resource(string name)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"PrimitiveCodebaseElements.Tests.Resources.{name}";
        using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    
    public static string PlatformSpecific(this string str)
    {
        return Environment.NewLine switch
        {
            "\n" when str.Contains("\r\n") => str.Replace("\r\n", "\n"),
            "\r\n" when str.Contains('\n') && !str.Contains("\r\n") => str.Replace("\n", "\r\n"),
            _ => str
        };
    }
}