using SoundForge;

namespace SoundForgeScriptsLib.Utils
{
    public interface IFileMarkersWrapper
    {
        int Add(SfAudioMarker marker);
        SfAudioMarker this[int idx] { get; }
        ISfFileHost File { get; }
    }

    public class FileMarkersWrapper : IFileMarkersWrapper
    {
        private readonly ISfFileHost _file;

        public FileMarkersWrapper(ISfFileHost file)
        {
            _file = file;
        }

        public int Add(SfAudioMarker marker)
        {
            return File.Markers.Add(marker);
        }

        public SfAudioMarker this[int idx]
        {
            get { return File.Markers[idx]; }
        }

        public ISfFileHost File
        {
            get { return _file; }
        }
    }
}