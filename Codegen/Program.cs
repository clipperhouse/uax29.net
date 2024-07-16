
using System.Text;

internal class Program
{
	const string version = "15.0.0";

	static async Task Main(string[] args)
	{

		string[] typs = ["Word", "Sentence", "Grapheme"];

		foreach (var typ in typs)
		{
			await WriteCategories(typ);
			await WriteTests(typ);
		}

		static async Task WriteCategories(string typ)
		{
			List<string> urls = [$"https://www.unicode.org/Public/{version}/ucd/auxiliary/{typ}BreakProperty.txt"];
			if (typ == "Word" || typ == "Grapheme")
			{
				// We need Extended_Pictographic
				urls.Add($"https://www.unicode.org/Public/{version}/ucd/emoji/emoji-data.txt");
			}

			var data = "";

			foreach (var url in urls)
			{
				using var client = new HttpClient();
				var response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode();
				data += await response.Content.ReadAsStringAsync();
			}

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

				if ((typ == "Word" || typ == "Grapheme") && cat.StartsWith("Emoji"))   // may be brittle if data changes
				{
					// We only want Extended_Pictographic
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
						currentCat <<= 1;
					}
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

			if (typ == "Word")
			{
				// hack in a Tab category that the spec doesn't use, be we do
				const string tab = "Tab";
				currentCat <<= 1;
				cats.Add(tab, currentCat);
				catsByRune.Add(0x09, tab);
			}

			// write the file
			using var dict = new StreamWriter($"../uax29/{typ}s.Dict.cs");

			dict.WriteLine($"// generated from {urls[0]}");

			dict.Write(@$"
namespace UAX29;

using Property = uint;

internal static partial class {typ}s
{{
");

			foreach (var kv in cats)
			{
				dict.WriteLine($"	const Property {kv.Key} = {kv.Value};");
			}

			dict.Write(@"
	static readonly Dict Dict = new(GetDict());
	static Dictionary<int, Property> GetDict() => new()
	{
");

			foreach (var kv in catsByRune)
			{
				// codegen the dict
				dict.WriteLine($"		{{0x{kv.Key:X4}, {kv.Value}}},");
			}

			dict.WriteLine("	};	// end dict");


			dict.WriteLine("};	// end class");
		}

		static async Task WriteTests(string typ)
		{
			/// Tests
			var url = $"https://www.unicode.org/Public/{version}/ucd/auxiliary/{typ}BreakTest.txt";

			using var client = new HttpClient();
			var response = await client.GetAsync(url);
			response.EnsureSuccessStatusCode();
			var data = await response.Content.ReadAsStringAsync();

			using var reader = new StringReader(data);

			using var dict = new StreamWriter($"../uax29/{typ}s.Test.cs");
			dict.WriteLine($"// generated from {url}");
			dict.Write(@$"namespace Tests;

using System.Text;
using UAX29;

[TestFixture]
public class {typ}sTests
{{
	static UnicodeTest[] Tests => UnicodeTests;

	[Test, TestCaseSource(nameof(Tests))]
	public void Bytes(UnicodeTest test)
	{{
		var tokens = Split.{typ}s(test.input);
		TestUnicode.TestBytes(tokens, test);
	}}

	[Test, TestCaseSource(nameof(Tests))]
	public void String(UnicodeTest test)
	{{
		var s = Encoding.UTF8.GetString(test.input);
		var tokens = Split.{typ}s(s);
		TestUnicode.TestChars(tokens, test);
	}}

	[Test, TestCaseSource(nameof(Tests))]
	public void Stream(UnicodeTest test)
	{{
		using var stream = new MemoryStream(test.input);
		var tokens = Split.{typ}s(stream);
		TestUnicode.TestStream(tokens, test);
	}}

	[Test, TestCaseSource(nameof(Tests))]
	public void TextReader(UnicodeTest test)
	{{
		using var stream = new MemoryStream(test.input);
		using var reader = new StreamReader(stream);
		var tokens = Split.{typ}s(reader);
		TestUnicode.TestTextReader(tokens, test);
	}}

	internal readonly static UnicodeTest[] UnicodeTests = [
");
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

				var parts = line.Split('#');
				var pattern = parts[0].Trim();

				var comment = parts[1].Trim();

				var splits = pattern.Split('÷', StringSplitOptions.RemoveEmptyEntries);

				var input = "[";
				var expected = "[";

				foreach (var split in splits)
				{
					var runes = split.Split('×', StringSplitOptions.TrimEntries);

					expected += "[";
					foreach (var r in runes)
					{
						var i = Convert.ToInt32(r.Trim(), 16);
						var s = Char.ConvertFromUtf32(i);
						var bytes = Encoding.UTF8.GetBytes(s);
						foreach (var b in bytes)
						{
							input += $"0x{b:X4}, ";
							expected += $"0x{b:X4}, ";
						}

					}
					expected = expected.Trim().TrimEnd(',');
					expected += "], ";
				}
				expected = expected.Trim().TrimEnd(',');
				expected += "]";

				input = input.Trim().TrimEnd(',');
				input += "]";

				dict.WriteLine($"		new({input}, {expected}, \"{comment}\"),");
			}
			dict.Write(@$"
	];
}}
");
		}
	}
}