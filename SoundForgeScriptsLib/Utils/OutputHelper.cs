using System;
using System.Windows.Forms;
using SoundForge;
using SoundForgeScriptsLib.EntryPoints;

namespace SoundForgeScriptsLib.Utils
{
    public interface IOutputHelper
    {
        DialogResult ToMessageBox(string caption, MessageBoxIcon icon, MessageBoxButtons buttons, string fmt, params object[] args);
        DialogResult ToMessageBox(MessageBoxIcon icon, MessageBoxButtons buttons, string fmt, params object[] args);
        DialogResult ToMessageBox(MessageBoxIcon icon, string fmt, params object[] args);
        DialogResult ToMessageBox(string fmt, params object[] args);
        void ToScriptWindow(object obj);
        void ToScriptWindow(string fmt, params object[] args);
        void LineBreakToScriptWindow();
        void ToStatusBar(string fmt, params object[] args);
        void ToStatusField1(string fmt, params object[] args);
        void ToStatusField2(string fmt, params object[] args);
    }

    public class OutputHelper : IOutputHelper
    {
        private readonly IEntryPoint _entryPoint;
        private readonly IScriptableApp _app;

        public delegate void MessageLogger(string fmt, params object[] args);

        public OutputHelper(IEntryPoint entryPoint)
        {
            _entryPoint = entryPoint;
            _app = entryPoint.App;
        }

        public DialogResult ToMessageBox(string caption, MessageBoxIcon icon, MessageBoxButtons buttons, string fmt, params object[] args)
        {
            return MessageBox.Show(_app.Win32Window, string.Format(fmt, args), caption, buttons, icon);
        }

        public DialogResult ToMessageBox(MessageBoxIcon icon, MessageBoxButtons buttons, string fmt, params object[] args)
        {
            return ToMessageBox(_entryPoint.ScriptTitle, icon, buttons, fmt, args);
        }

        public DialogResult ToMessageBox(MessageBoxIcon icon, string fmt, params object[] args)
        {
            return ToMessageBox(_entryPoint.ScriptTitle, icon, MessageBoxButtons.OK, fmt, args);
        }

        public DialogResult ToMessageBox(string fmt, params object[] args)
        {
            return ToMessageBox(_entryPoint.ScriptTitle, MessageBoxIcon.None, MessageBoxButtons.OK, fmt, args);
        }

        public void ToScriptWindow(object obj)
        {
            _app.OutputText(string.Format("{0}: {1}", obj.GetType(), obj));
        }

        public void ToScriptWindow(string fmt, params object[] args)
        {
            _app.OutputText(string.Format(fmt, args));
        }

        public void LineBreakToScriptWindow()
        {
            _app.OutputText(Environment.NewLine);
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