using SoundForge;

namespace SoundForgeScriptsLib.EntryPoints
{
    public interface IEntryPoint
    {
        void FromSoundForge(IScriptableApp app);
        IScriptableApp App { get; }
        string ScriptTitle { get; set; }
    }
}