namespace Tests;

using System.Text.Json;
using uax29;
using NUnit.Framework.Internal;
using ProtoBuf;
using System.Reflection;
using System.Text;

[TestFixture]
public class Tests
{

	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Segmenter()
	{
		var s = Encoding.UTF8.GetBytes("This is a test.");
		var seg = new Segmenter(SplitFuncs.Whitespace, s);

		while (seg.Next())
		{
			var word = Encoding.UTF8.GetString(seg.Bytes());
			Console.WriteLine(word);
		}
	}

	[Test]
	public void Deserialize()
	{
		var assembly = Assembly.Load("uax29");
		using var stream = assembly.GetManifestResourceStream("uax29.words.proto.bin");
		var trie = Serializer.Deserialize<RuneTrie>(stream);
	}

	[Test]
	public void HappyPath()
	{
		var trie = new RuneTrie();

		var data1 = 'a';
		var data2 = 'b';
		var data3 = '你';

		trie.Insert(data1, 100);
		trie.Insert(data2, 200);
		trie.Insert(data3, 300);

		void test(char c, (bool found, int payload) expected)
		{
			Assert.Multiple(() =>
			{
				Console.WriteLine(c + ": ");
				var got = trie.Search(c);
				Assert.That(got.found, Is.EqualTo(expected.found), $"'{c}' should have been found");
				Assert.That(got.payload, Is.EqualTo(expected.payload), $"payload for '{c}' was {got.payload}, expected {expected.payload}");
			});
		}

		(char s, (bool found, int payload) expected)[] tests = [
			(data1, (true, 100)),
			(data2,  (true, 200)),
			(data3,  (true, 300)),
		];

		foreach (var t in tests)
		{
			test(t.s, t.expected);
		}
		var j = JsonSerializer.Serialize(trie);
		Console.WriteLine(j);
	}
}
