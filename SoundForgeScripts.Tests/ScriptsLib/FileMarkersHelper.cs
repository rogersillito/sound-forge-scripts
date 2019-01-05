using System.Collections.Generic;
using Moq;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Tests.ScriptsLib
{
    public class FileMarkersHelper
    {
        public static Mock<IFileMarkersWrapper> CreateStubMarkerList()
        {
            var realMarkerList = new List<SfAudioMarker>();
            var markerList = new Mock<IFileMarkersWrapper>(MockBehavior.Default);
            markerList.Setup(x => x.Add(Moq.It.IsAny<SfAudioMarker>()))
                .Returns<SfAudioMarker>(m =>
                {
                    realMarkerList.Add(m);
                    return realMarkerList.IndexOf(m);
                });
            markerList.Setup(x => x[Moq.It.IsAny<int>()])
                .Returns<int>(idx => realMarkerList[idx]);
            return markerList;
        }
    }
}