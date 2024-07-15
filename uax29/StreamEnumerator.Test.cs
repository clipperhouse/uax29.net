namespace Tests;

using System.Text;
using UAX29;

[TestFixture]
public class TestStreamTokenizer
{
    [SetUp]
    public void Setup()
    {
    }

    static readonly Options[] options = [Options.None, Options.OmitWhitespace];

    /// <summary>
    /// Ensure that streamed text and static text return identical results.
    /// </summary>
    [Test]
    public void StreamMatchesStatic()
    {
        var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
        var examples = new List<string>()
        {
            example,											// smaller than the buffer
			string.Concat(Enumerable.Repeat(example, 999))		// larger than the buffer
		};
        foreach (var option in options)
        {

            foreach (var input in examples)
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var staticTokens = Split.Words(bytes, Options.OmitWhitespace);

                using var stream = new MemoryStream(bytes);
                var streamTokens = Split.Words(stream, Options.OmitWhitespace);

                foreach (var streamToken in streamTokens)
                {
                    staticTokens.MoveNext();

                    var expected = Encoding.UTF8.GetString(staticTokens.Current);
                    var got = Encoding.UTF8.GetString(streamToken);

                    Assert.That(got, Is.EqualTo(expected));
                }

                Assert.That(staticTokens.MoveNext(), Is.False, "Static tokens should have been consumed");
                Assert.That(streamTokens.MoveNext(), Is.False, "Stream tokens should have been consumed");
            }
        }
    }

    /// <summary>
    /// Ensure that streamed text and static text return identical results.
    /// </summary>
    [Test]
    public void StreamReaderMatchesStatic()
    {
        var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
        var examples = new List<string>()
        {
            example,											// smaller than the buffer
			string.Concat(Enumerable.Repeat(example, 999))		// larger than the buffer
		};

        foreach (var option in options)
        {
            foreach (var input in examples)
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var staticTokens = Split.Words(bytes, option);

                using var stream = new MemoryStream(bytes);
                using var reader = new StreamReader(stream);
                var streamTokens = Split.Words(reader, option);

                foreach (var streamToken in streamTokens)
                {
                    staticTokens.MoveNext();
                    var staticCurrent = Encoding.UTF8.GetString(staticTokens.Current);

                    Assert.That(staticCurrent, Is.EqualTo(streamToken.ToString()));
                }

                Assert.That(staticTokens.MoveNext(), Is.False, "Static tokens should have been consumed");
                Assert.That(streamTokens.MoveNext(), Is.False, "Stream tokens should have been consumed");
            }
        }
    }

    [Test]
    public void SetStream()
    {
        var input = "Hello, how are you?";
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream(bytes);

        var tokens = Split.Words(stream);

        var first = new List<string>();
        foreach (var token in tokens)
        {
            var s = Encoding.UTF8.GetString(token);
            first.Add(s);
        }

        Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

        using var stream2 = new MemoryStream(bytes);

        tokens.SetStream(stream2);

        var second = new List<string>();
        foreach (var token in tokens)
        {
            var s = Encoding.UTF8.GetString(token);
            second.Add(s);
        }

        Assert.That(first.SequenceEqual(second));
    }

    [Test]
    public void SetStreamReader()
    {
        var input = "Hello, how are you?";
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream(bytes);
        using var reader = new StreamReader(stream);

        var tokens = Split.Words(reader);

        var first = new List<string>();
        foreach (var token in tokens)
        {
            var s = token.ToString();
            first.Add(s);
        }

        Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

        using var stream2 = new MemoryStream(bytes);
        using var reader2 = new StreamReader(stream2);

        tokens.SetStream(reader2);

        var second = new List<string>();
        foreach (var token in tokens)
        {
            var s = token.ToString();
            second.Add(s);
        }

        Assert.That(first.SequenceEqual(second));
    }

    [Test]
    public void StreamEnumerator()
    {
        var input = "Hello, how are you?";
        var bytes = Encoding.UTF8.GetBytes(input);
        using var stream = new MemoryStream(bytes);

        var tokens = Split.Words(stream);

        var first = new List<string>();
        while (tokens.MoveNext())
        {
            var s = tokens.Current.ToString();
            first.Add(s);
        }

        Assert.That(first, Has.Count.GreaterThan(1));   // just make sure it did the thing

        using var stream2 = new MemoryStream(bytes);
        var tokens2 = Split.Words(stream2);

        var second = new List<string>();
        foreach (var token in tokens2)
        {
            var s = token.ToString();
            second.Add(s);
        }
        Assert.That(first.SequenceEqual(second));
    }


    [Test]
    public void StreamToList()
    {
        var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
        var bytes = Encoding.UTF8.GetBytes(example);
        using var stream = new MemoryStream(bytes);

        var list = Split.Words(stream).ToList();

        stream.Seek(0, SeekOrigin.Begin);
        var tokens = Split.Words(stream);

        var i = 0;
        foreach (var token in tokens)
        {
            Assert.That(token.SequenceEqual(list[i]));
            i++;
        }

        Assert.That(list, Has.Count.EqualTo(i), "ToList should return the same number of tokens as iteration");

        var threw = false;
        tokens.MoveNext();
        try
        {
            tokens.ToList();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        Assert.That(threw, Is.True, "Calling ToList after iteration has begun should throw");
    }

    [Test]
    public void StreamToArray()
    {
        var example = "abcdefghijk lmnopq r stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
        var bytes = Encoding.UTF8.GetBytes(example);
        using var stream = new MemoryStream(bytes);

        var list = Split.Words(stream).ToList();

        stream.Seek(0, SeekOrigin.Begin);
        var tokens = Split.Words(stream);

        var i = 0;
        foreach (var token in tokens)
        {
            Assert.That(token.SequenceEqual(list[i]));
            i++;
        }

        Assert.That(list, Has.Count.EqualTo(i), "ToArray should return the same number of tokens as iteration");

        var threw = false;
        tokens.MoveNext();
        try
        {
            tokens.ToArray();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        Assert.That(threw, Is.True, "Calling ToArray after iteration has begun should throw");
    }

    [Test]
    public void Position()
    {
        var example = "abcdefghi jklmnopqr stu vwxyz; ABC DEFG HIJKL MNOP Q RSTUV WXYZ! 你好，世界.";
        var bytes = Encoding.UTF8.GetBytes(example);

        {
            using var stream = new MemoryStream(bytes);
            var tokens = Split.Words(stream, minBufferBytes: 8);
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(0));    // ab...
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(9));    // <space>
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(10));   // jk...
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(19));   // <space>
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(20));   // stu...
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(23));   // <space>
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(24));   // vw...
        }

        {
            using var stream = new MemoryStream(bytes);
            var tokens = Split.Words(stream, minBufferBytes: 8, options: Options.OmitWhitespace);
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(0));        // ab...
            // tokens.MoveNext();
            // Assert.That(tokens.Position, Is.EqualTo(9));     // <space>
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(10));       // jk...
            // tokens.MoveNext();
            // Assert.That(tokens.Position, Is.EqualTo(19));    // <space>
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(20));       // stu...
            // tokens.MoveNext();
            // Assert.That(tokens.Position, Is.EqualTo(23));    // <space>
            tokens.MoveNext();
            Assert.That(tokens.Position, Is.EqualTo(24));       // vw...
        }
    }
}