using System.Text;
using ProtoBuf;
using uax29;

const string url = "https://www.unicode.org/Public/UCD/latest/ucd/auxiliary/WordBreakProperty.txt";

using var client = new HttpClient();
var response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();

using var responseStream = await response.Content.ReadAsStreamAsync();
using var reader = new StreamReader(responseStream);

int currentCat = 0;
var lastCat = "";
var trie = new RuneTrie();

while (true)
{
	var line = await reader.ReadLineAsync();
	if (line == null)
	{
		break;
	}
	line = line.Trim();

	if (line.Length == 0)
	{
		continue;
	}
	if (line.StartsWith('#'))
	{
		continue;
	}

	var parts = line.Split(';');
	var range = parts[0].Trim();
	var cat = parts[1].Split('#')[0].Trim();

	if (cat != lastCat)
	{
		if (currentCat == 0)
		{
			currentCat = 1;
		}
		else
		{
			currentCat = currentCat << 1;
		}
		// Console.WriteLine(Convert.ToString(currentCat, 2).PadLeft(8, '0'));
		lastCat = cat;
	}

	int start, end;
	if (range.Contains(".."))
	{
		// the range is inclusive
		var limits = range.Split("..");
		start = int.Parse(limits[0], System.Globalization.NumberStyles.HexNumber);
		end = int.Parse(limits[1], System.Globalization.NumberStyles.HexNumber);
	}
	else
	{
		start = int.Parse(range.Trim(), System.Globalization.NumberStyles.HexNumber);
		end = start;
	}

	// Console.WriteLine("Starting range " + range);
	for (var i = start; i <= end; i++)
	{
		var bytes = ToUtf8Bytes(i);
		trie.Insert(bytes, currentCat);
	}
}

var protoPath = "../uax29/words.proto.bin";
using (var file = File.Open(protoPath, FileMode.Create))
{
	Serializer.Serialize(file, trie);
}

RuneTrie trie2;
using (var file2 = File.OpenRead(protoPath))
{
	trie2 = Serializer.Deserialize<RuneTrie>(file2);
}


static byte[] ToUtf8Bytes(int codePoint)
{
	// Step 1: Convert the code point to a string
	string character = char.ConvertFromUtf32(codePoint);

	// Step 2: Encode the string to UTF-8 bytes
	byte[] utf8Bytes = Encoding.UTF8.GetBytes(character);

	return utf8Bytes;
}

