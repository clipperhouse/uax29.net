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
}
