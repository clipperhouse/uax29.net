namespace Tests;

using System.Text.Json;
using Trie;
using NUnit.Framework.Internal;
using ProtoBuf;
using System.Reflection;

[TestFixture]
public class ByteTrieTests
{

	[SetUp]
	public void Setup()
	{
	}


	[Test]
	public void ProtoBufTest()
	{
		var assembly = Assembly.Load("Trie");
		using var stream = assembly.GetManifestResourceStream("Trie.words.proto.bin");
		var trie = Serializer.Deserialize<ByteTrie>(stream);
	}

	[Test]
	public void Basics()
	{
		var trie = new ByteTrie();

		var data1 = "apple";
		var data2 = "app";
		var data3 = "你好";

		trie.Insert(data1, 100);
		trie.Insert(data2, 200);
		trie.Insert(data3, 300);

		void test(string s, (bool found, int payload) expected)
		{
			Assert.Multiple(() =>
			{
				Console.WriteLine(s + ": ");
				var got = trie.Search(s);
				Console.WriteLine(" " + got.length);
				Assert.That(got.found, Is.EqualTo(expected.found), $"'{s}' should have been found");
				Assert.That(got.payload, Is.EqualTo(expected.payload), $"payload for '{s}' was {got.payload}, expected {expected.payload}");
			});
		}

		(string s, (bool found, int payload) expected)[] tests = [
			(data1, (true, 100)),
			(data2,  (true, 200)),
			(data3,  (true, 300)),
			("ap",  (false, -1)),
			("foo",  (false, -1)),
			("你",  (false, -1)),
		];

		foreach (var t in tests)
		{
			test(t.s, t.expected);
		}
		var j = JsonSerializer.Serialize(trie);
		Console.WriteLine(j);


	}
}
