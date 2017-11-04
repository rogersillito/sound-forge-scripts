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
            get { return _scriptTitle ?? Script.Name; }
            set { _scriptTitle = value; }
        }

        public void FromSoundForge(IScriptableApp app)
        {
            _app = app;
            _outputHelper = new OutputHelper(this);
            const string aborted = "Script Aborted";
            try
            {
                Execute();
                Output.ToStatusBar(ScriptTitle + " Finished.");
            }
            catch (ScriptAbortedException ex)
            {
                string msgText = string.IsNullOrEmpty(ex.Message) ? aborted  : ex.Message;
                Output.ToMessageBox(MessageBoxIcon.Error, MessageBoxButtons.OK, msgText);
                Output.ToStatusBar(aborted);
                Output.ToScriptWindow(ErrorFormatter.Format(ex));
            }
            catch (Exception ex)
            {
                Output.ToStatusBar("Script Terminated: An unhandled exception occurred while executing {0}!", ScriptTitle);
                Output.ToScriptWindow(ErrorFormatter.Format(ex));
            }
        }

        protected abstract void Execute();
    }
}