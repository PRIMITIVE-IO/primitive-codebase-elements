using System;
using System.IO;
using System.Reflection;

namespace PrimitiveCodebaseElements.Tests;

public static class TestUtils
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
    
    
    public static string PlatformSpecific(this string str)
    {
        if (Environment.NewLine == "\n" && str.Contains("\r\n"))
        {
            return str.Replace("\r\n", "\n");
        } 
        if (Environment.NewLine == "\r\n" && str.Contains('\n') && !str.Contains("\r\n"))
        {
            return str.Replace("\n", "\r\n");
        }
        return str;
    }
}