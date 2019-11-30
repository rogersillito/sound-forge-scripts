using System;
using System.Collections;
using System.Collections.Generic;
using SoundForge;

namespace SoundForgeScriptsLib.Utils
{
    public interface IFileMarkersWrapper
    {
        int Add(SfAudioMarker marker);
        void Remove(SfAudioMarker marker);
        SfAudioMarker this[int idx] { get; }
        ISfFileHost File { get; }
    }

    public class FileMarkersWrapper : IFileMarkersWrapper
    {
        public FileMarkersWrapper(ISfFileHost file)
        {
            File = file;
        }

        public int Add(SfAudioMarker marker) => File.Markers.Add(marker);

        public void Remove(SfAudioMarker marker)
        {
            var idx = File.Markers.IndexOf(marker);
            File.Markers.RemoveAt(idx);
        }

        public SfAudioMarker this[int idx] => File.Markers[idx];

        public ISfFileHost File { get; }
    }
}
