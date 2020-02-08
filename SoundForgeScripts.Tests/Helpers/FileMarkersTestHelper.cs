using System.Collections.Generic;
using System.Linq;
using Moq;
using SoundForge;
using SoundForgeScriptsLib.Utils;

namespace SoundForgeScripts.Tests.Helpers
{
    public class FileMarkersTestHelper
    {
        protected internal List<SfAudioMarker> RealMarkerList;

        public Mock<IFileMarkersWrapper> CreateStubMarkerList(ISfFileHost file)
        {
            RealMarkerList = new List<SfAudioMarker>();
            var markerList = new Mock<IFileMarkersWrapper>(MockBehavior.Default);

            markerList.Setup(x => x.Remove(It.IsAny<SfAudioMarker>()))
                .Callback<SfAudioMarker>(m => RealMarkerList.Remove(m));

            markerList.Setup(x => x.File).Returns(file);

            markerList.Setup(x => x.Add(It.IsAny<SfAudioMarker>()))
                .Returns<SfAudioMarker>(m =>
                {
                    RealMarkerList.Add(m);
                    return RealMarkerList.IndexOf(m);
                });
            
            markerList.Setup(x => x[It.IsAny<int>()])
                .Returns<int>(idx => RealMarkerList[idx]);

            return markerList;
        }
    }
}
