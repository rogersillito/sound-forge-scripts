using System;
using System.Text;

namespace ScriptFileProcessor
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
            message.AppendFormat("Message: {0}", e.Message);
            message.AppendLine();
            message.AppendFormat("StackTrace: {0}", e.StackTrace);
            message.AppendLine();
            return DoFormat(e.InnerException, depth + 1, message);
        }
    }
}