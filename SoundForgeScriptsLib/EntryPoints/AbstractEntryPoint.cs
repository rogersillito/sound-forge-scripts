using System;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScriptsLib.EntryPoints
{
    public abstract class AbstractEntryPoint: IEntryPoint
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

        public void Begin(IScriptableApp app)
        {
            _app = app;
            _outputHelper = new OutputHelper(app);
            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                Output.ToStatusBar("Script Terminated: An exception occurred while executing EntryPoint '{0}'", GetType().Name);
                Output.ToScriptWindow(ErrorFormatter.Format(ex));
            }
        }

        protected abstract void Execute();
    }
}