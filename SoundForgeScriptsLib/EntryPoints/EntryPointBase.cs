using System;
using System.Windows.Forms;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.EntryPoints
{
    public abstract class EntryPointBase: IEntryPoint
    {
        private IScriptableApp _app;
        private OutputHelper _outputHelper;
        private string _scriptTitle;

        public IScriptableApp App
        {
            get { return _app; }
        }

        public OutputHelper Output
        {
            get { return _outputHelper; }
        }

        public string ScriptTitle
        {
            get { return _scriptTitle ?? "Sound Forge Script"; }
            set { _scriptTitle = value; }
        }

        public void FromSoundForge(IScriptableApp app)
        {
            _app = app;
            _outputHelper = new OutputHelper(this);
            try
            {
                Execute();
            }
            catch (ScriptAbortedException ex)
            {
                const string aborted = "Script Aborted";
                string msgText = string.IsNullOrEmpty(ex.Message) ? aborted  : ex.Message;
                Output.ToMessageBox(MessageBoxIcon.Error, MessageBoxButtons.OK, msgText);
                Output.ToStatusBar(aborted);
                Output.ToScriptWindow(ErrorFormatter.Format(ex));
            }
            catch (Exception ex)
            {
                Output.ToStatusBar("Script Terminated: An unhandled exception occurred while executing EntryPoint '{0}'", GetType().Name);
                Output.ToScriptWindow(ErrorFormatter.Format(ex));
            }
        }

        protected abstract void Execute();
    }
}