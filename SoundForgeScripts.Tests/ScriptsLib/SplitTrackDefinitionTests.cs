using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using developwithpassion.specifications.moq;
using developwithpassion.specifications.extensions;
using developwithpassion.specifications.faking;
using Should;
using SoundForge;
using SoundForgeScripts.Scripts.VinylRip3FinalTrackProcessing;
using SoundForgeScriptsLib.VinylRip;

namespace SoundForgeScripts.Tests.ScriptsLib
{   
    public class SplitTrackDefinitionTests
    {
        public abstract class SplitTrackDefinitionContext : Observes<SplitTrackDefinition>
        {
            protected static ISfFileHost _file;

            private Establish context = () =>
            {
                _file = depends.@on<ISfFileHost>();
                //_file.setup(x => x.Length).Return(1000000);

                //_file.setup(x => x.Markers).Return(
                //    new SfAudioMarkerList(ExistingMarkers.ToArray())
                //    );

                sut.Selection = new SfAudioSelection(0, 1000);
                sut.FadeOutStartPosition = 900;
                sut.FadeInLength = 10;
            };

            protected static List<SfAudioMarker> ExistingMarkers;
        }

        [Subject(typeof(SplitTrackDefinition))]
        public class when_setting_fade_out_length : SplitTrackDefinitionContext
        {
            Because of = () => sut.FadeOutLength = 200;

            private It should_update_selection = () =>
                sut.Selection.Length.ShouldEqual(1100);

            private It should_not_change_update_fade_out_start = () =>
                sut.FadeOutStartPosition.ShouldEqual(900);

            private static SplitTrackDefinition _track;
        }
    }
}
