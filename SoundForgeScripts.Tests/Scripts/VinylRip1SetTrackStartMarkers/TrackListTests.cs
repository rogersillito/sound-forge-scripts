using System.Linq;
using developwithpassion.specifications.moq;
using Machine.Specifications;
using Should;
using SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers;

namespace SoundForgeScripts.Tests.Scripts.VinylRip1SetTrackStartMarkers
{
    [Subject(typeof(TrackList))]
    public class TrackListTests: Observes<TrackList>
    {
        Because of = () =>
        {
            sut.AddNew();
            sut.AddNew();
            sut.AddNew();
        };

        private It should_indicate_last_track = () =>
        {
            var last = sut.Last();
            last.IsLast.ShouldBeTrue();
            last.ShouldEqual(sut.LastAdded);
        };

    }
}