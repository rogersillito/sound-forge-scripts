using System;
using System.IO;
using System.Reflection;

namespace SoundForgeScriptsLib.Utils
{
    public class ResourceHelper
    {
        public static void GetResourceStream(string resourcePath, Action<Stream> streamAccessor)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ResourceHelper));
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    throw new Exception(string.Format("Could not open embedded resource: '{0}'", resourcePath));
                }
                streamAccessor(stream);
            }
        }
    }
}
