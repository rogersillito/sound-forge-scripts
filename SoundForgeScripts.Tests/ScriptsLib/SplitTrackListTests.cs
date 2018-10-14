﻿using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.faking;
using Should;
using SoundForge;
using SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing;

namespace SoundForgeScripts.Tests.ScriptsLib
{   
    public class SplitTrackListTests
    {
        public abstract class SplitTrackListContext : Observes<SplitTrackList>
        {
            protected static ISfFileHost _file;

            private Establish context = () =>
            {
                ExistingMarkers = new List<SfAudioMarker>
                {
                    new SfAudioMarker(100, 500000) { Name = $"{SplitTrackList.TrackRegionPrefix}0001" },
                    new SfAudioMarker(500500), // ignore - marker
                    new SfAudioMarker(600000, 100000) { Name = $"{SplitTrackList.TrackRegionPrefix}0002" }, // too close to next for full fade!
                    new SfAudioMarker(700500), // ignore - marker
                    new SfAudioMarker(701000, 100000) { Name = $"{SplitTrackList.TrackRegionPrefix}0003" },
                    new SfAudioMarker(900000, 100000) { Name = $"NOT_A_TRACK!" }, // name not expected format
                };

                _file = depends.@on<ISfFileHost>();
                _file.setup(x => x.Length).Return(1100000);

                _file.setup(x => x.Markers).Return(
                    new SfAudioMarkerList(ExistingMarkers.ToArray())
                    );
            };

            protected static List<SfAudioMarker> ExistingMarkers;
        }

        [Subject(typeof(SplitTrackList))]
        public class when_getting_list_of_tracks : SplitTrackListContext
        {
            Because of = () => { _tracks =  sut.InitTracks(30, 7000); };

            It should_ignore_non_track_regions = () =>
                _tracks.Count.ShouldEqual(3);

            It should_set_fade_in_length_to_each_track = () =>
                _tracks.All(t => t.FadeInLength == 30).ShouldBeTrue();

            It should_set_start_to_be_same_as_original_marker = () =>
                _tracks.All(t => t.Selection.Start == t.RegionFound.Start).ShouldBeTrue();

            It should_return_true_when_checking_add_fade_in_outs = () =>
            {
                _tracks.All(t => t.CanAddFadeIn).ShouldBeTrue();
                _tracks.All(t => t.CanAddFadeOut).ShouldBeTrue();
            };

            It should_set_requested_fadeout_when_possible = () =>
            {
                _tracks[0].Selection.Length.ShouldEqual(507000);
                _tracks[2].Selection.Length.ShouldEqual(107000);
            };

            It should_set_fadeout_to_before_next_track_start_when_gap_too_short_for_requested_fade = () =>
            {
                _tracks[1].Selection.Length.ShouldEqual(100999);
            };

            It should_set_incremental_track_numbers = () =>
            {
                _tracks[0].Number.ShouldEqual(1);
                _tracks[1].Number.ShouldEqual(2);
                _tracks[2].Number.ShouldEqual(3);
            };

            It should_set_found_region_on_each_track = () =>
            {
                _tracks[0].RegionFound.ShouldEqual(ExistingMarkers[0]);
                _tracks[1].RegionFound.ShouldEqual(ExistingMarkers[2]);
                _tracks[2].RegionFound.ShouldEqual(ExistingMarkers[4]);
            };

            private static SplitTrackList _tracks;
        }

        [Subject(typeof(SplitTrackList))]
        public class when_getting_list_of_tracks_with_no_fades : SplitTrackListContext
        {
            Because of = () => { _tracks =  sut.InitTracks(0,0); };

            It should_set_no_fade_in_length_to_each_track = () =>
                _tracks.All(t => t.FadeInLength == 0).ShouldBeTrue();

            It should_set_length_to_be_same_as_original_marker = () =>
                _tracks.All(t => t.Selection.Length == t.RegionFound.Length).ShouldBeTrue();

            It should_return_false_when_checking_add_fade_in_outs = () =>
            {
                _tracks.All(t => t.CanAddFadeIn).ShouldBeFalse();
                _tracks.All(t => t.CanAddFadeOut).ShouldBeFalse();
            };

            private static SplitTrackList _tracks;
        }
    }
}
