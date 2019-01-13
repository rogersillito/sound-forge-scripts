using System;
using System.Collections;
using System.Collections.Generic;
using SoundForge;

namespace SoundForgeScriptsLib.Utils
{
    public interface IFileMarkersWrapper : IEnumerable<SfAudioMarker>
    {
        int Add(SfAudioMarker marker);
        void Remove(SfAudioMarker marker);
        SfAudioMarker this[int idx] { get; }
        ISfFileHost File { get; }
        IEnumerable<SfAudioMarker> GetSortedByStartPosition();
    }

    public class FileMarkersWrapper : IFileMarkersWrapper
    {
        public FileMarkersWrapper(ISfFileHost file)
        {
            File = file;
        }

        public int Add(SfAudioMarker marker) => File.Markers.Add(marker);

        public void Remove(SfAudioMarker marker) => File.Markers.Remove(marker);

        public SfAudioMarker this[int idx] => File.Markers[idx];

        public ISfFileHost File { get; }

        public IEnumerator<SfAudioMarker> GetEnumerator() => new SfAudioMarkerEnumerator(File.Markers);

        IEnumerator IEnumerable.GetEnumerator() => File.Markers.GetEnumerator();

        public IEnumerable<SfAudioMarker> GetSortedByStartPosition()
        {
            var sorted = new List<SortableSfAudioMarker>();
            foreach (SfAudioMarker marker in File.Markers)
            {
                sorted.Add(new SortableSfAudioMarker(marker));
            }
            sorted.Sort();
            foreach (var sm in sorted)
            {
                yield return sm.Marker;
            }
        }
    }

    public class SortableSfAudioMarker : IComparable<SortableSfAudioMarker>
    {
        public SfAudioMarker Marker { get; }

        public SortableSfAudioMarker(SfAudioMarker marker)
        {
            Marker = marker;
        }

        public int CompareTo(SortableSfAudioMarker other) => Marker.Start.CompareTo(other.Marker.Start);
    }

    public class SfAudioMarkerEnumerator : IEnumerator<SfAudioMarker>
    {
        private readonly SfAudioMarkerList _collection;
        private int _curIndex;
        private SfAudioMarker _curMarker;

        public SfAudioMarkerEnumerator(SfAudioMarkerList collection)
        {
            _collection = collection;
            _curIndex = -1;
            _curMarker = default(SfAudioMarker);
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_curIndex >= _collection.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _curMarker = _collection[_curIndex];
            }
            return true;
        }

        public void Reset() { _curIndex = -1; }

        public object Current => _curMarker;
        SfAudioMarker IEnumerator<SfAudioMarker>.Current => _curMarker;

        public void Dispose()
        {
        }
    }
}