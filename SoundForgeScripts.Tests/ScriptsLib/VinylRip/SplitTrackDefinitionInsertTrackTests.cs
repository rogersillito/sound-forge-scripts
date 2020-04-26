using System;
using System.Collections.Generic;
using System.Linq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.moq;
using Machine.Specifications;
using Should;
using SoundForge;
using SoundForgeScripts.Scripts.VinylRip1SetTrackStartMarkers;
using SoundForgeScripts.Tests.Helpers;
using SoundForgeScriptsLib.Utils;
using SoundForgeScriptsLib.VinylRip;
using It = Machine.Specifications.It;

namespace SoundForgeScripts.Tests.ScriptsLib.VinylRip
{
    public class SplitTrackDefinitionInsertTrackTests
    {
        public abstract class SplitTrackDefinitionContext : Observes<SplitTrackDefinition>
        {
            protected static ISfFileHost _file;

            private Establish context = () =>
            {
                Func<int, long> seconds = n => n * 44100;
                ExistingMarkers = new List<SfAudioMarker>
                {
                    new SfAudioMarker(seconds(10),  seconds(30)) { Name = $"0001{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(seconds(10) + 100) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" }, // fade in == 100 samples
                    new SfAudioMarker(seconds(40) + 100) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" }, // 100 sample fade out

                    new SfAudioMarker(seconds(50),  seconds(30)) { Name = $"0002{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(seconds(50) + 100) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" }, // fade in == 100 samples
                    new SfAudioMarker(seconds(80) + 100) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" }, // 100 sample fade out
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(seconds(120)); // 2 minutes

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray())
                );

                var fileMarkersHelper = new FileMarkersTestHelper();
                var markerList = fileMarkersHelper.CreateStubMarkerList(_file);
                fileMarkersHelper.RealMarkerList.AddRange(ExistingMarkers);
                var markerAndRegionFactory = new TrackMarkerFactory(markerList.Object);

                SplitTrackList = new SplitTrackList(
                    markerAndRegionFactory,
                    markerAndRegionFactory,
                    new TrackMarkerNameBuilder(),
                    markerList.Object,
                    new TrackMarkerSpecifications(),
                    depends.@on<IOutputHelper>()
                );

            };

            protected static List<SfAudioMarker> ExistingMarkers;
            protected internal static SplitTrackList SplitTrackList;
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_can_insert_either_side_of_first_track : SplitTrackDefinitionContext
        {
            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    // TODO: set appropriate values...
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(100);
                    //SplitTrackList.InitTracks(new VinylRipOptions
                    //{
                    //    DefaultTrackFadeInLengthInSamples = 10,
                    //    DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TestSuppliedDefaultTrackFadeOutLengthInSeconds
                    //});
                    return SplitTrackList.First();
                });
            };

            It should_return_true_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeTrue();

            It should_return_true_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeTrue();
        }
    }
}
