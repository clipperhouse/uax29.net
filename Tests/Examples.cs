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
		var segment = new Words.Segmenter(example);

		while (segment.Next())
		{
			Console.WriteLine(segment);
		}
	}
}
