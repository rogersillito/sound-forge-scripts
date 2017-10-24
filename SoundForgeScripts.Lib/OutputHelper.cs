using System.Windows.Forms;
using SoundForge;

namespace SoundForgeScripts.Lib
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
    }
}