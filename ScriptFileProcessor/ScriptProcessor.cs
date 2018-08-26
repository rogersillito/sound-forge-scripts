using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace ScriptFileProcessor
{
    public class ScriptProcessor
    {
        public ScriptInfo BuildEntryPointScript(string scriptDir, string buildDir)
        {
            var script = FindEntryPoint(scriptDir, buildDir);
            Dump(script);

            //TODO: read script name  - description or filename

            //TODO: concat all cs file contents

            //TODO: strip NSes

            //TODO: save as scriptname

            //TODO: copy renamed config + icon to buildDir

            string fileText;
            using (var fileStream = File.OpenText(script.SourcePath))
            {
                // READ
                fileText = fileStream.ReadToEnd();
            }

            // REPLACE TEXT
            //fileText = strip

            //Console.WriteLine("File Content: {0}", fileText);

            using (var writer = new StreamWriter(script.BuiltPath, false))
            {
                // OVERWRITE
                writer.WriteLine(fileText);
            }

            //Console.WriteLine("File Content Replaced: {0}", filePath);

            return script;
        }

        public class ScriptInfo
        {
            public string SourcePath { get; set; }
            public string BuiltPath { get; set; }
            public override string ToString()
            {
                return $"SourcePath = '{SourcePath}'\nBuiltPath  = '{BuiltPath}'";
            }
        }

        public static void Dump(object output) => Console.WriteLine($"----------------------------\n{output}\n----------------------------");

        private static ScriptInfo FindEntryPoint(string scriptDir, string buildDir)
        {
            ScriptInfo ep = null;
            DirWalk(scriptDir, f =>
            {
                if (ep != null || Path.GetExtension(f).ToLower() != ".cs")
                    return true;

                var lines = File.ReadAllLines(f);
                for (var i = lines.Length; i >= 0; i--)
                {
                    var line = lines[i-1];
                    if (Regex.IsMatch(line, @"public +class +([a-zA-Z0-9_@]+) *: *EntryPointBase"))
                    {
                        ep = new ScriptInfo {SourcePath = f};
                        line = lines[i-2]; // now check the preceeding line
                        var attributeMatch = Regex.Match(line, @"^\s*\[\s*ScriptName(Attribute)?\s*\(\s*""([^\""]+)""\s*\)\s*\]");
                        if (attributeMatch.Success)
                        {
                            // check for ScriptName attribute
                            var name = attributeMatch.Groups.Cast<Group>().Last().Value.Trim();
                            if (name != string.Empty)
                            {
                                ep.BuiltPath = Path.Combine(buildDir, $"{name}.cs");
                            }
                        }
                        break;
                    }
                }
                return true;
            }).ToList();
            if (ep == null)
                throw new Exception("Could not find class deriving from EntryPointBase");
            if (ep.BuiltPath == null)
                ep.BuiltPath = ep.SourcePath;
            return ep;
        }

        private static string StripNamespaces(string fileText)
        {
            fileText = Regex.Replace(fileText, @"namespace [a-zA-Z.0-9_@]+\s*\r?\n\s*{", string.Empty);
            fileText = Regex.Replace(fileText, @"\}\s*$", string.Empty);
            return fileText;
        }

        public static IEnumerable<T> DirWalk<T>(string sDir, Func<string, T> callback)
        {
            foreach (string d in Directory.GetDirectories(sDir))
            {
                foreach (var yielded in DirWalk(d, callback))
                {
                    yield return yielded;
                }
            }
            foreach (string f in Directory.GetFiles(sDir))
            {
                yield return callback(f);
            }
        }
    }
}
