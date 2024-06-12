namespace Tests;

using uax29;
using System.Text;

[TestFixture]
public class TestBuffer
{
    [Test]
    public void Consume()
    {
        var input = "abcd efg hijkl mnopqr stu v wxyz";
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream(bytes);

        Read<byte> read = stream.Read;
        var buffer = new Buffer<byte>(read, 16);

        const int consume = 5;
        int consumed = 0;
        string result = "";

        while (buffer.Contents.Length > 0)
        {
            var contents = Encoding.UTF8.GetString(buffer.Contents);
            Assert.That(input[consumed..], Does.StartWith(contents));

            var remaining = buffer.Contents.Length;
            var toConsume = remaining > consume ? remaining : consume;
            result += Encoding.UTF8.GetString(buffer.Contents[0..toConsume]);
            buffer.Consume(toConsume);
            consumed += toConsume;
        }

        Assert.That(result, Is.EqualTo(input));
    }
}


