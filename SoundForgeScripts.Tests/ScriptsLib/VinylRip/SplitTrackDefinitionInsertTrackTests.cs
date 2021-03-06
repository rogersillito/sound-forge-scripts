using System;
using System.Collections.Generic;
using System.Linq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.moq;
using Machine.Specifications;
using Moq;
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
                    //  9900 gap
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
                _markerList = fileMarkersHelper.CreateStubMarkerList(_file);
                fileMarkersHelper.RealMarkerList.AddRange(ExistingMarkers);
                var markerAndRegionFactory = new TrackMarkerFactory(_markerList.Object);

                SplitTrackList = new SplitTrackList(
                    markerAndRegionFactory,
                    markerAndRegionFactory,
                    new TrackMarkerNameBuilder(),
                    _markerList.Object,
                    new TrackMarkerSpecifications(),
                    depends.@on<IOutputHelper>()
                );

            };

            protected static List<SfAudioMarker> ExistingMarkers;
            protected internal static SplitTrackList SplitTrackList;
            protected static Mock<IFileMarkersWrapper> _markerList;
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_can_insert_before_first_track : SplitTrackDefinitionContext
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

            It should_return_false_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeFalse();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_can_insert_either_side_of_first_track : SplitTrackDefinitionContext
        {
            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(9800);
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

        [Subject(typeof(SplitTrackDefinition))]
        public class when_can_insert_either_side_of_second_track : SplitTrackDefinitionContext
        {
            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(9800);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.Last();
                });
            };

            It should_return_true_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeTrue();

            It should_return_true_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeTrue();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_can_insert_max_possible_length_track_after_second_track : SplitTrackDefinitionContext
        {
            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(19800);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.Last();
                });
            };

            It should_return_true_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeTrue();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_cannot_insert_either_side_of_second_track : SplitTrackDefinitionContext
        {
            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    // required length is 1 sample TOO LONG
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(19801);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.Last();
                });
            };

            It should_return_false_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeFalse();

            It should_return_false_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeFalse();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_track_inserted_before_first_track : SplitTrackDefinitionContext
        {
            private static SplitTrackDefinition _newTrack;

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

            Because of = () => _newTrack = sut.InsertTrackBefore();

            It should_create_track = () => _newTrack.ShouldNotBeNull();

            It should_create_track_to_fill_available_space = () =>
            {
                _newTrack.TrackRegion.Start.ShouldEqual(0);
                MarkerHelper.GetMarkerEnd(_newTrack.TrackRegion).ShouldEqual(9900);
                _newTrack.FadeOutEndMarker.Start.ShouldEqual(10000);
            };

            It should_add_markers_to_file = () =>
                _markerList.Verify(x => x.Add(Moq.It.IsAny<SfAudioMarker>()), Times.Exactly(3));

            It should_name_new_track = () =>
            {
                _newTrack.TrackRegion.Name.ShouldContain("0001");
                _newTrack.FadeInEndMarker.Name.ShouldContain("0001");
                _newTrack.FadeOutEndMarker.Name.ShouldContain("0001");
            };

            It should_add_track_to_list = () => SplitTrackList.Count.ShouldEqual(3);

            It should_reset_track_numbering = () =>
            {
                _newTrack.Number.ShouldEqual(1);
                sut.Number.ShouldEqual(2);
                SplitTrackList.Last().Number.ShouldEqual(3);
            };

            It should_return_false_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeFalse();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_track_inserted_after_first_track : SplitTrackDefinitionContext
        {
            private static SplitTrackDefinition _newTrack;

            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(9800);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.First();
                });
            };

            Because of = () => _newTrack = sut.InsertTrackAfter();

            It should_create_track = () => _newTrack.ShouldNotBeNull();

            It should_create_track_to_fill_available_space = () =>
            {
                _newTrack.TrackRegion.Start.ShouldEqual(20100);
                MarkerHelper.GetMarkerEnd(_newTrack.TrackRegion).ShouldEqual(29900);
                _newTrack.FadeOutEndMarker.Start.ShouldEqual(30000);
            };

            It should_add_markers_to_file = () =>
                _markerList.Verify(x => x.Add(Moq.It.IsAny<SfAudioMarker>()), Times.Exactly(3));

            It should_name_new_track = () =>
            {
                _newTrack.TrackRegion.Name.ShouldContain("0002");
                _newTrack.FadeInEndMarker.Name.ShouldContain("0002");
                _newTrack.FadeOutEndMarker.Name.ShouldContain("0002");
            };

            It should_add_track_to_list = () => SplitTrackList.Count.ShouldEqual(3);

            It should_reset_track_numbering = () =>
            {
                sut.Number.ShouldEqual(1);
                _newTrack.Number.ShouldEqual(2);
                SplitTrackList.Last().Number.ShouldEqual(3);
            };

            It should_return_false_checking_can_insert_after = () => sut.CanInsertTrackAfter().ShouldBeFalse();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_track_inserted_before_second_track : SplitTrackDefinitionContext
        {
            private static SplitTrackDefinition _newTrack;

            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(9800);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.Last();
                });
            };

            Because of = () => _newTrack = sut.InsertTrackBefore();

            It should_create_track = () => _newTrack.ShouldNotBeNull();

            It should_add_markers_to_file = () =>
                _markerList.Verify(x => x.Add(Moq.It.IsAny<SfAudioMarker>()), Times.Exactly(3));

            It should_add_track_to_list = () => SplitTrackList.Count.ShouldEqual(3);

            It should_create_track_to_fill_available_space = () =>
            {
                _newTrack.TrackRegion.Start.ShouldEqual(20100);
                MarkerHelper.GetMarkerEnd(_newTrack.TrackRegion).ShouldEqual(29900);
                _newTrack.FadeOutEndMarker.Start.ShouldEqual(30000);
            };

            It should_name_new_track = () =>
            {
                _newTrack.TrackRegion.Name.ShouldContain("0002");
                _newTrack.FadeInEndMarker.Name.ShouldContain("0002");
                _newTrack.FadeOutEndMarker.Name.ShouldContain("0002");
            };

            It should_reset_track_numbering = () =>
            {
                SplitTrackList.First().Number.ShouldEqual(1);
                _newTrack.Number.ShouldEqual(2);
                sut.Number.ShouldEqual(3);
            };

            It should_return_false_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeFalse();
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_track_inserted_after_second_track : SplitTrackDefinitionContext
        {
            private static SplitTrackDefinition _newTrack;

            private Establish context = () =>
            {
                sut_factory.create_using(() =>
                {
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup)).Return(FadeLength);
                    _file.setup(x => x.SecondsToPosition(VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup)).Return(19800);
                    SplitTrackList.InitTracks(new VinylRipOptions
                    {
                        DefaultTrackFadeInLengthInSamples = FadeLength,
                        DefaultTrackFadeOutLengthInSeconds = VinylRipTestHelpers.TrackFadeOutLengthInSecondsForMockSetup,
                        MinimumTrackLengthInSeconds = VinylRipTestHelpers.MinimumTrackLengthInSecondsForMockSetup
                    });
                    return SplitTrackList.Last();
                });
            };

            Because of = () => _newTrack = sut.InsertTrackAfter();

            It should_create_track = () => _newTrack.ShouldNotBeNull();

            It should_add_markers_to_file = () =>
                _markerList.Verify(x => x.Add(Moq.It.IsAny<SfAudioMarker>()), Times.Exactly(3));

            It should_add_track_to_list = () => SplitTrackList.Count.ShouldEqual(3);

            It should_create_track_to_fill_available_space = () =>
            {
                _newTrack.TrackRegion.Start.ShouldEqual(40100);
                MarkerHelper.GetMarkerEnd(_newTrack.TrackRegion).ShouldEqual(59900);
                _newTrack.FadeOutEndMarker.Start.ShouldEqual(60000);
            };

            It should_name_new_track = () =>
            {
                _newTrack.TrackRegion.Name.ShouldContain("0003");
                _newTrack.FadeInEndMarker.Name.ShouldContain("0003");
                _newTrack.FadeOutEndMarker.Name.ShouldContain("0003");
            };

            It should_reset_track_numbering = () =>
            {
                SplitTrackList.First().Number.ShouldEqual(1);
                sut.Number.ShouldEqual(2);
                _newTrack.Number.ShouldEqual(3);
            };

            It should_return_false_checking_can_insert_before = () => sut.CanInsertTrackBefore().ShouldBeFalse();
        }
    }
}
