using System;
using System.IO;
using System.Linq;
using ScriptFileProcessor;
using Machine.Specifications;
using Should;
using SoundForgeScripts.Tests.Helpers;

namespace SoundForgeScripts.Tests.ScriptFileProcessor
{
    [Subject(typeof(ScriptProcessor))]
    public class ScriptProcessorTests
    {
        private Establish that = () =>
        {
            var inputFilePath = ResourceHelper.LoadResourceFileToTempPath("SoundForgeScripts.Tests.ScriptFileProcessor.using_test.txt");
            _testFileText = File.ReadAllText(inputFilePath);

        };

        private Because of = () =>
        {
            var processed = ScriptProcessor.ArrangeUsingStatements(_testFileText);
            _result = processed.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        };

        private It should_place_all_usings_at_top_of_file_uniquely_and_reformatted = () =>
        {
            MatchedText(_result[0]).ShouldEqual("using System;");
            MatchedText(_result[1]).ShouldEqual("using System.Collections.Generic;");
            MatchedText(_result[2]).ShouldEqual("using System.IO;");
            MatchedText(_result[3]).ShouldEqual("using System.Linq;");
            MatchedText(_result[4]).ShouldEqual("using bernard=System.CodeDom.Compiler;");
            MatchedText(_result[5]).ShouldEqual("using STUFF=System.CodeDom.Compiler;");
            MatchedText(_result[6]).ShouldEqual("using System.Text.RegularExpressions;");
            MatchedText(_result[7]).ShouldEqual("using System.IO.Compression;");
            MatchedText(_result[8]).ShouldEqual("using System.Text;");

            foreach (var line in _result.Skip(9))
            {
                MatchedText(line).ShouldEqual(string.Empty);
            }
        };

        private It should_be_no_usings_in_remainder_of_file = () =>
        {
            foreach (var line in _result.Skip(9))
            {
                MatchedText(line).ShouldEqual(string.Empty);
            }
        };

        private static string MatchedText(string x) => ScriptProcessor.GetUsingLineMatch(x).Value;
        private static string _testFileText;
        private static string[] _result;
    }
}