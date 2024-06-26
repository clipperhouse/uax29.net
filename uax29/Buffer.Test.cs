﻿namespace Tests;

using System.Text;
using UAX29;

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

	[Test]
	public void Moving()
	{
		var input = "abcd efg hijkl mnopqr stu v wxyz; AB CDEF GH I JKLMN OPQ RST UVWXYZ";
		var bytes = Encoding.UTF8.GetBytes(input);
		using var stream = new MemoryStream(bytes);

		const int minItems = 16;
		const int factor = 2;
		const int storageSize = factor * minItems;

		Read<byte> read = stream.Read;
		var buffer = new Buffer<byte>(read, minItems, new byte[storageSize]);

		var consumed = 0;

		// Trigger a read by calling Contents, should be full
		Assert.That(buffer.Contents.Length, Is.EqualTo(storageSize));
		Assert.That(buffer.start, Is.EqualTo(0));
		Assert.That(buffer.end, Is.EqualTo(storageSize));

		{
			// start should move forward, since we haven't consumed half yet
			var consume = 4;
			buffer.Consume(consume);
			consumed += consume;

			Assert.That(buffer.start, Is.EqualTo(consume));
			Assert.That(buffer.end, Is.EqualTo(storageSize));

			// trigger a read
			Assert.That(buffer.Contents.Length, Is.EqualTo(storageSize - consumed));
		}

		{
			// now exceed minItems
			var consume = 15;
			buffer.Consume(consume);
			consumed += consume;   // gets us to 19 = 15 + 4

			// trigger a read, should be full
			Assert.That(buffer.Contents.Length, Is.EqualTo(storageSize));
			Assert.That(buffer.start, Is.EqualTo(0));
			Assert.That(buffer.end, Is.EqualTo(storageSize));
		}
	}
}


