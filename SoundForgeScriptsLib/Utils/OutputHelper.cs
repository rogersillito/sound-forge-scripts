using System;
using System.Windows.Forms;
using SoundForge;

namespace SoundForgeScriptsLib.Utils
{
    public class OutputHelper
    {
        private readonly IScriptableApp _app;

        public OutputHelper(IScriptableApp app)
        {
            _app = app;
        }

        public void ToMessageBox(string fmt, params object[] args)
        {
            MessageBox.Show(string.Format(fmt, args));
        }

        public void ToMessageBox(string caption, MessageBoxIcon icon, string fmt, params object[] args)
        {
            MessageBox.Show(string.Format(fmt, args), caption, MessageBoxButtons.OK, icon);
        }

        public void ToScriptWindow(object obj)
        {
            _app.OutputText(string.Format("{0}: {1}", obj.GetType(), obj));
        }

        public void ToScriptWindow(string fmt, params object[] args)
        {
            _app.OutputText(string.Format(fmt, args));
        }

        public void ToStatusBar(string fmt, params object[] args)
        {
            _app.SetStatusText(string.Format(fmt, args));
        }

        public void ToStatusField1(string fmt, params object[] args)
        {
            _app.SetStatusField(0, string.Format(fmt, args));
        }

        public void ToStatusField2(string fmt, params object[] args)
        {
            _app.SetStatusField(1, string.Format(fmt, args));
        }

        public static string FormatToTimeSpan(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string time = string.Format("{0:D2}:{1:D2}:{2:D2}.{3:D3}",
                            t.Hours,
                            t.Minutes,
                            t.Seconds,
                            t.Milliseconds);
            return time;
        }
    }
}