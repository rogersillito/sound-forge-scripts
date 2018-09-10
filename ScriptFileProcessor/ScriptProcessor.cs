using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptFileProcessor
{
    public class ScriptProcessor
    {
        private static readonly string[] SourceExtensions = new[] { ".cs" };

        public ScriptInfo BuildEntryPointScript(string scriptDir, string buildDir)
        {
            var script = new ScriptInfo();
            try
            {
                script = FindEntryPoint(scriptDir, buildDir);
                if (script == null)
                    return null;

                Dump(script);

                var entryPointFileText = StripNamespaces(GetFileText(script.SourcePath));
                var builtScriptText = new StringBuilder(entryPointFileText);

                var paths = GetOtherSourcePaths(script);
                foreach (var path in paths)
                {
                    var sourceFileText = GetFileText(path);
                    builtScriptText.Append(Environment.NewLine + Environment.NewLine + StripNamespaces(sourceFileText));
                    Dump($"Included contents of {Path.GetFileName(path)}");
                }

                string fileText;
                //var path = script.SourcePath;
                //fileText = GetFileText(path);

                // REPLACE TEXT
                //fileText = strip

                //Console.WriteLine("File Content: {0}", fileText);

                using (var writer = new StreamWriter(script.BuiltPath, false))
                {
                    // OVERWRITE
                    writer.WriteLine(builtScriptText.ToString());
                }

                //Console.WriteLine("File Content Replaced: {0}", filePath);

                script.Success = true;
            }
            catch (Exception e)
            {
                script.Error = ErrorFormatter.Format(e);
            }
            return script;
        }

        private static IEnumerable<string> GetOtherSourcePaths(ScriptInfo script)
        {
            var sourceFiles = new List<string>();
            DirWalk(script.SourceDir, f =>
            {
                if (f == script.SourcePath) return true;
                if (SourceExtensions.Contains(Path.GetExtension(f)?.ToLower()))
                    sourceFiles.Add(f);
                return true;
            }).ToList();
            return sourceFiles.OrderBy(p => p);
        }

        private static string GetFileText(string path)
        {
            string fileText;
            using (var fileStream = File.OpenText(path))
            {
                // READ
                fileText = fileStream.ReadToEnd();
            }
            return fileText;
        }

        public class ScriptInfo
        {
            public string SourcePath { get; set; }
            public string BuiltPath { get; set; }
            public string SourceDir { get; set; }
            public string Error { get; set; }
            public bool Success { get; set; }
            public override string ToString()
            {
                return $"SourcePath = '{SourcePath}'\nBuiltPath  = '{BuiltPath}'";
            }
        }

        public static void Dump(object output) =>
            Console.WriteLine($"----------------------------\n{output}\n----------------------------");

        private static ScriptInfo FindEntryPoint(string scriptDir, string buildDir)
        {
            ScriptInfo ep = null;
            DirWalk(scriptDir, f =>
            {
                if (ep != null || Path.GetExtension(f)?.ToLower() != ".cs")
                    return true;

                var lines = File.ReadAllLines(f);
                for (var i = lines.Length; i >= 0; i--)
                {
                    var line = lines[i - 1];
                    if (Regex.IsMatch(line, @"public +class +([a-zA-Z0-9_@]+) *: *EntryPointBase"))
                    {
                        ep = new ScriptInfo { SourcePath = f, SourceDir = scriptDir };
                        line = lines[i - 2]; // now check the preceeding line
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
            const string nsRegex = @"namespace [a-zA-Z.0-9_@]+\s*\r?\n\s*{";
            if (!Regex.IsMatch(fileText, nsRegex))
                return fileText;
            fileText = Regex.Replace(fileText, nsRegex, string.Empty);
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
