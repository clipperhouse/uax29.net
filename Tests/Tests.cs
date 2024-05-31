namespace Tests;

using uax29;
using NUnit.Framework.Internal;
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
		var s = Encoding.UTF8.GetBytes("This is a test, with some number or words you know.");
		var seg = new Segmenter(Words.SplitFunc, s);

		while (seg.Next())
		{
			var word = Encoding.UTF8.GetString(seg.Bytes());
			Console.WriteLine(word);
		}
	}

	[Test]
	public void Unicode()
	{
		foreach (var test in UnicodeTests.Words)
		{
			var seg = Words.Segment(test.input);
			var i = 0;
			while (seg.Next())
			{
				var expected = test.expected[i];
				var got = seg.Bytes();
				Assert.That(expected.SequenceEqual(got), $@"Test {i}, {test.comment}
				input {test.input}
				expected {expected}
				got {got}
				");
				i++;
			}
		}
	}
}
