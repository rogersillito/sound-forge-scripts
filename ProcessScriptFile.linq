<Query Kind="Program" />

void Main(string[] args)
{
	string fileText;
	using (var fileStream = File.OpenText(args[0]))
	{
		fileText = fileStream.ReadToEnd();
	}
	fileText = Regex.Replace(fileText, @"namespace [a-zA-Z.0-9_@]+\s*\r?\n\s*{", string.Empty);
	fileText = Regex.Replace(fileText, @"\}\s*$", string.Empty);
	fileText.Dump();
}

// Define other methods and classes here