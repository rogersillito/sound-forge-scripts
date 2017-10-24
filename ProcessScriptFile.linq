<Query Kind="Program" />

void Main(string[] args)
{
	string fileText;
	using (var fileStream = File.OpenText(args[0]))
	{
		// READ
		fileText = fileStream.ReadToEnd();
	}
	
	// REPLACE TEXT
	fileText = Regex.Replace(fileText, @"namespace [a-zA-Z.0-9_@]+\s*\r?\n\s*{", string.Empty);
	fileText = Regex.Replace(fileText, @"\}\s*$", string.Empty);
	//fileText.Dump();

	using (StreamWriter newTask = new StreamWriter(args[0], false))
	{
		// OVERWRITE
		newTask.WriteLine(fileText);
	}

	$"File Content Replaced: {args[0]}".Dump();
}

// Define other methods and classes here