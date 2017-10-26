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

        protected IScriptableApp App
        {
            get { return _app; }
        }

        public OutputHelper Output
        {
            get { return _outputHelper; }
        }

        public void FromSoundForge(IScriptableApp app)
        {
            _app = app;
            _outputHelper = new OutputHelper(app);
            try
            {
                Execute();
            }
            catch (ScriptAbortedException ex)
            {
                Output.ToMessageBox("Script Aborted", MessageBoxIcon.Error, string.IsNullOrEmpty(ex.Message) ? "" : ex.Message);
                Output.ToStatusBar("Script Aborted");
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