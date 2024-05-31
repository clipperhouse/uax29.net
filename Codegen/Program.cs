
const string version = "15.1.0";
const string url = $"https://www.unicode.org/Public/{version}/ucd/auxiliary/WordBreakProperty.txt";
const string url2 = $"https://www.unicode.org/Public/{version}/ucd/emoji/emoji-data.txt";

var data = "";

using var client = new HttpClient();
var response = await client.GetAsync(url);
response.EnsureSuccessStatusCode();
data += await response.Content.ReadAsStringAsync();

var response2 = await client.GetAsync(url2);
response.EnsureSuccessStatusCode();
data += await response2.Content.ReadAsStringAsync();

using var reader = new StringReader(data);

int currentCat = 0;
var lastCat = "";
var cats = new Dictionary<string, int>();
var catsByRune = new Dictionary<int, string>();

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

	if (cat.StartsWith("Emoji"))
	{
		continue;
	}

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

		cats.Add(cat, currentCat);
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
		if (catsByRune.TryGetValue(i, out string? existing))
		{
			catsByRune[i] = $"{existing} | {cat}";
		}
		else
		{
			catsByRune.Add(i, cat);
		}
	}
}

// write the file
using var dict = new StreamWriter("../uax29/WordsDict.cs");

dict.WriteLine($"// generated from {url}");

dict.Write(@"
namespace uax29;

using Property = int;

public static partial class Words
{
");

foreach (var kv in cats)
{
	dict.WriteLine($"	const Property {kv.Key} = {kv.Value};");
}

dict.Write(@"
	private static readonly Dictionary<int, Property> dict = new()
	{
");

foreach (var kv in catsByRune)
{
	// codegen the dict
	dict.WriteLine($"		{{0x{kv.Key:X4}, {kv.Value}}},");
}

dict.WriteLine("	};	// end dict");


dict.WriteLine("};	// end class");

