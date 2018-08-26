using System.ComponentModel;

namespace SoundForgeScriptsLib
{
    public class ScriptNameAttribute: DescriptionAttribute
    {
        public ScriptNameAttribute(string scriptName) : base(scriptName)
        {
        }
    }
}