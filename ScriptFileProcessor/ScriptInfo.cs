namespace ScriptFileProcessor
{
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
}