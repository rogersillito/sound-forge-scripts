using System;

namespace SoundForgeScriptsLib
{
    public class ScriptAbortedException: Exception
    {
        public ScriptAbortedException()
        {
        }

        public ScriptAbortedException(string message) : base(message)
        {
        }

        public ScriptAbortedException(string messageFormat, params object[] args): this(string.Format(messageFormat, args))
        {
        }
    }
}