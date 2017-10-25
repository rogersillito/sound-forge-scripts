using System;
using System.Text;

namespace SoundForgeScriptsLib.Utils
{
    public class ErrorFormatter
    {
        public static string Format(Exception exception)
        {
            return DoFormat(exception, 0, new StringBuilder());
        }

        private static string DoFormat(Exception e, int depth, StringBuilder message)
        {
            if (e == null)
                return message.ToString();
            message.AppendFormat("Type: {0} ", e.GetType());
			message.AppendLine(depth == 0 ? "[Outer Exception]" : string.Format("[Inner Exception: {0}]", depth));
            message.AppendFormat("Message: {0}\n StackTrace: {1}\n", e.Message, e.StackTrace);
            return DoFormat(e.InnerException, depth + 1, message);
        }
    }
}