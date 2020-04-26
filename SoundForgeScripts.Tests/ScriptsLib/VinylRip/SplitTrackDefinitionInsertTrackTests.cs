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

            internal const long FadeLength = 100;

            private Establish context = () =>
            {
                Func<int, long> seconds = n => n * 44100;
                ExistingMarkers = new List<SfAudioMarker>
                {
                    // START --> 10000 gap
                    new SfAudioMarker(10000,  10000) { Name = $"0001{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(10000 + FadeLength) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" }, // FadeLength sample fade in
                    new SfAudioMarker(20000 + FadeLength) { Name = $"0001{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" }, // FadeLength sample fade out
                    // FadeLength00 gap
                    new SfAudioMarker(30000,  10000) { Name = $"0002{TrackMarkerNameBuilder.TrackRegionSuffix}" },
                    new SfAudioMarker(30000 + FadeLength) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeInEndSuffix}" }, // FadeLength sample fade in
                    new SfAudioMarker(40000 + FadeLength) { Name = $"0002{TrackMarkerNameBuilder.TrackFadeOutEndSuffix}" }, // FadeLength sample fade out
                    // 19900 gap --> END
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(60000);

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
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(9900);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.First();
                });
            };

            It should_return_true_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeTrue();

            It should_return_true_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeTrue();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_cannot_insert_either_side_of_first_track : SplitTrackDefinitionContext
        {
            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    // required length is 1 sample TOO LONG
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(9901);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.First();
                });
            };

            It should_return_false_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeFalse();

            It should_return_false_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeFalse();
        }
    }
}
