using System.IO;
using System.Reflection;

namespace UnitTests
{
    public class TestUtilities
    {
        public static string GetEmbeddedSampleXml(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileStream = assembly.GetManifestResourceStream("UnitTests.SampleFogbugzXml." + fileName);
            var sr = new StreamReader(fileStream);
            return sr.ReadToEnd();
        }
    }
}
