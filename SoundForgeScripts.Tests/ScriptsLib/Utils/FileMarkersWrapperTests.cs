using System.Collections.Generic;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using Moq;
using System.Linq;
using SoundForge;
using SoundForgeScripts.Tests.Helpers;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;
using Arg = Moq.It;
using It = Machine.Specifications.It;

namespace SoundForgeScripts.Tests.ScriptsLib.Utils
{
    public class FileMarkersWrapperTests
    {
        [Subject(typeof(FileMarkersWrapper))]
        public class when_getting_markers_sorted_by_start_position : Observes<FileMarkersWrapper>
        {
            private Establish context = () =>
            {
                var file = new Mock<ISfFileHost>();

                var realMarkerList = new List<SfAudioMarker>
                {
                    new SfAudioMarker(0, 5) {Name = $"A"},
                    new SfAudioMarker(30, 5) {Name = $"B"},
                    new SfAudioMarker(40, 5) {Name = $"C"},
                    new SfAudioMarker(10, 5) {Name = $"D"},
                    new SfAudioMarker(20, 5) {Name = $"E"}
                };

                file.Setup(x => x.Markers).Returns(
                    new SfAudioMarkerList(realMarkerList.ToArray())
                );

                sut_factory.create_using(() => new FileMarkersWrapper(file.Object));
            };

            private Because of = () => { _results = sut.GetSortedByStartPosition(); };

            private It should_return_expected_order = () => _results.Select(m => m.Name).SequenceEqual(new[] { "A", "D", "E", "B", "C" });

            private static IEnumerable<SfAudioMarker> _results;
        }
    }
}
