using System.Text;
using uax29;

namespace Tests;


[TestFixture]
public class Examples
{

	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Readme()
	{
		var example = "Here is some example text. 你好，世界.";

		// The simple version
		var words = example.TokenizeWords();
		foreach (var word in words)
		{
			// We return raw UTF-8 bytes
			var s = Encoding.UTF8.GetString(word);
			Console.WriteLine(s);
		}

		// The higher-performance version
		var bytes = Encoding.UTF8.GetBytes(example);
		var tokens = new Tokenizer(bytes);
		while (tokens.MoveNext())
		{
			// We return raw UTF-8 bytes
			var s = Encoding.UTF8.GetString(tokens.Current);
			Console.WriteLine(s);
		}
	}
}
