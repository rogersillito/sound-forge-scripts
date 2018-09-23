using System;
using System.IO;
using System.Reflection;

namespace SoundForgeScripts.Tests.Helpers
{
    public class ResourceHelper
    {
        public static string LoadResourceFileToTempPath(string resourcePath)
        {
            var assembly = Assembly.GetAssembly(typeof(ResourceHelper));
            var filePath = Path.GetTempFileName();
            using (var stream = assembly.GetManifestResourceStream(resourcePath)) 
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                if (stream == null)
                {
                    throw new Exception($"Could not open embedded resource at path: '{resourcePath}'");
                }
                stream.CopyTo(fileStream);
            }
            return filePath;
        }
    }
}
