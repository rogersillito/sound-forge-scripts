using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using bernard = System.CodeDom.Compiler;
using BUMS = System.CodeDom.Compiler;
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

                Dump(script, true);

                var entryPointFileText = StripNamespaces(GetFileText(script.SourcePath));
                var builtScriptText = new StringBuilder(entryPointFileText);

                var paths = GetOtherSourcePaths(script);
                foreach (var path in paths)
                {
                    var sourceFileText = GetFileText(path);
                    builtScriptText.Append(Environment.NewLine + Environment.NewLine + StripNamespaces(sourceFileText));
                    Dump($"Included contents of {Path.GetFileName(path)}");
                }
                using (var writer = new StreamWriter(script.BuiltPath, false))
                {
                    var usingsProcessed = ArrangeUsingStatements(builtScriptText.ToString());
                    writer.WriteLine(usingsProcessed);
                }
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

        public static void Dump(object output, bool wrapLine = false)
        {
            if (wrapLine) output = $"----------------------------\n{output}\n----------------------------\n";
            Console.WriteLine(output);
        }

        private static ScriptInfo FindEntryPoint(string scriptDir, string buildDir)
        {
            ScriptInfo ep = null;
            DirWalk(scriptDir, f =>
            {
                if (ep != null || Path.GetExtension(f)?.ToLower() != ".cs")
                    return true;

                var lines = File.ReadAllLines(f);
                if (!lines.Any()) return true;

                var lineNum = lines.Length - 1;
                while (lineNum > 0)
                {
                    var line = lines[lineNum];
                    if (Regex.IsMatch(line, @"public +class +EntryPoint *: *EntryPointBase"))
                    {
                        ep = new ScriptInfo { SourcePath = f, SourceDir = scriptDir };
                        if (lineNum > 1)
                        {
                            line = lines[lineNum - 1]; // now check the preceeding line
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
                        }
                        break;
                    }
                    lineNum--;
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

        public static Match GetUsingLineMatch(string line)
        {
            const string usingRegex = @"^\s*using((\s*\w+\s*=)?(\s*\w+\s*\.)*\s*\w+\s*;\s*)$";
            return Regex.Match(line, usingRegex);
        }

        public static string ArrangeUsingStatements(string fileText)
        {
            var outFile = new StringBuilder();
            var usings = new List<string>();
            var lines = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                var matches = GetUsingLineMatch(line);
                if (!matches.Success)
                {
                    outFile.AppendLine(line);
                    continue;
                }
                var ns = matches.Groups[1].Value;
                ns = Regex.Replace(ns, @"\s+", string.Empty);
                var usingLine = $"using {ns}";
                if (usings.Contains(usingLine)) continue;
                usings.Add(usingLine);
            }

            usings.Reverse();
            foreach (var usingLine in usings)
            {
                outFile.Insert(0, usingLine + Environment.NewLine);
            }
            return outFile.ToString();
        }
    }
}
