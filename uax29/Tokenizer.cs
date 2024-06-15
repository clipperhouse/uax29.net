namespace uax29;

public static class Tokenizer
{
	public static class Graphemes
	{
		public static Tokenizer<char, GraphemeUtf16Splitter> Create(string input)
		=> Create(input.AsSpan());

		public static Tokenizer<char, GraphemeUtf16Splitter> Create(ReadOnlySpan<char> input)
		=> new(input);

		public static Tokenizer<byte, GraphemeUtf8Splitter> Create(ReadOnlySpan<byte> input)
		=> new(input);

		public static StreamTokenizer<byte, GraphemeUtf8Splitter> Create(Stream stream, int maxTokenBytes = 1024)
		{
			var tok = Create(ReadOnlySpan<byte>.Empty);
			var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
			return new StreamTokenizer<byte, GraphemeUtf8Splitter>(buffer, tok);
		}

		public static StreamTokenizer<char, GraphemeUtf16Splitter> Create(TextReader stream, int maxTokenBytes = 1024)
		{
			var tok = Create(ReadOnlySpan<char>.Empty);
			var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
			return new StreamTokenizer<char, GraphemeUtf16Splitter>(buffer, tok);
		}
	}

	public static class Words
	{
		public static Tokenizer<char, WordUtf16Splitter> Create(string input)
		=> Create(input.AsSpan());

		public static Tokenizer<char, WordUtf16Splitter> Create(ReadOnlySpan<char> input)
		=> new(input);

		public static Tokenizer<byte, WordUtf8Splitter> Create(ReadOnlySpan<byte> input)
		=> new(input);

		public static StreamTokenizer<byte, WordUtf8Splitter> Create(Stream stream, int maxTokenBytes = 1024)
		{
			var tok = Create(ReadOnlySpan<byte>.Empty);
			var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
			return new StreamTokenizer<byte, WordUtf8Splitter>(buffer, tok);
		}

		public static StreamTokenizer<char, WordUtf16Splitter> Create(TextReader stream, int maxTokenBytes = 1024)
		{
			var tok = Create(ReadOnlySpan<char>.Empty);
			var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
			return new StreamTokenizer<char, WordUtf16Splitter>(buffer, tok);
		}
	}

	public static class Sentences
	{
		public static Tokenizer<char, SentenceUtf16Splitter> Create(string input)
		=> Create(input.AsSpan());

		public static Tokenizer<char, SentenceUtf16Splitter> Create(ReadOnlySpan<char> input)
		=> new(input);

		public static Tokenizer<byte, SentenceUtf8Splitter> Create(ReadOnlySpan<byte> input)
		=> new(input);

		public static StreamTokenizer<byte, SentenceUtf8Splitter> Create(Stream stream, int maxTokenBytes = 1024)
		{
			var tok = Create(ReadOnlySpan<byte>.Empty);
			var buffer = new Buffer<byte>(stream.Read, maxTokenBytes);
			return new StreamTokenizer<byte, SentenceUtf8Splitter>(buffer, tok);
		}

		public static StreamTokenizer<char, SentenceUtf16Splitter> Create(TextReader stream, int maxTokenBytes = 1024)
		{
			var tok = Create(ReadOnlySpan<char>.Empty);
			var buffer = new Buffer<char>(stream.Read, maxTokenBytes);
			return new StreamTokenizer<char, SentenceUtf16Splitter>(buffer, tok);
		}
	}
}

public interface ISplit<TSpan>
{
	static abstract int Split(ReadOnlySpan<TSpan> input, bool atEOF = true);
}

public readonly struct GraphemeUtf16Splitter : ISplit<char>
{
	static int ISplit<char>.Split(ReadOnlySpan<char> input, bool atEOF)
	=> Graphemes.Splitter<char, Utf16Decoder>.Split(input, atEOF);
}

public readonly struct GraphemeUtf8Splitter : ISplit<byte>
{
	static int ISplit<byte>.Split(ReadOnlySpan<byte> input, bool atEOF)
	=> Graphemes.Splitter<byte, Utf8Decoder>.Split(input, atEOF);
}

public readonly struct WordUtf16Splitter : ISplit<char>
{
	static int ISplit<char>.Split(ReadOnlySpan<char> input, bool atEOF)
	=> Words.Splitter<char, Utf16Decoder>.Split(input, atEOF);
}

public readonly struct WordUtf8Splitter : ISplit<byte>
{
	static int ISplit<byte>.Split(ReadOnlySpan<byte> input, bool atEOF)
	=> Words.Splitter<byte, Utf8Decoder>.Split(input, atEOF);
}

public readonly struct SentenceUtf16Splitter : ISplit<char>
{
	static int ISplit<char>.Split(ReadOnlySpan<char> input, bool atEOF)
	=> Sentences.Splitter<char, Utf16Decoder>.Split(input, atEOF);
}

public readonly struct SentenceUtf8Splitter : ISplit<byte>
{
	static int ISplit<byte>.Split(ReadOnlySpan<byte> input, bool atEOF)
	=> Sentences.Splitter<byte, Utf8Decoder>.Split(input, atEOF);
}

/// <summary>
/// Tokenizer splits strings or UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// </summary>
public ref struct Tokenizer<TSpan, TSplit>
	where TSpan : struct
	where TSplit : struct, ISplit<TSpan>
{
	ReadOnlySpan<TSpan> input;

	int start = 0;
	int end = 0;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="input">A string, or UTF-8 byte array.</param>
	/// <param name="tokenType">Choose to split words, graphemes or sentences. Default is words.</param>
	internal Tokenizer(ReadOnlySpan<TSpan> input)
	{
		this.input = input;
	}

	/// <summary>
	/// Move to the next token. Use <see cref="Current"/> to retrieve the token.
	/// </summary>
	/// <returns>Whether there are any more tokens. False typically means EOF.</returns>
	public bool MoveNext()
	{
		if (end < input.Length)
		{
			var advance = TSplit.Split(input[end..]);
			// Interpret as EOF
			if (advance == 0)
			{
				return false;
			}

			start = end;
			end = start + advance;

			return true;
		}
		return false;
	}

	/// <summary>
	/// The current token (word, grapheme or sentence).
	/// If the input was a string, <see cref="Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="char"/>.
	/// If the input was UTF-8 bytes, <see cref="Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </summary>
	public readonly ReadOnlySpan<TSpan> Current
	{
		get
		{
			return input[start..end];
		}
	}

	public readonly Tokenizer<TSpan, TSplit> GetEnumerator()
	{
		return this;
	}

	/// <summary>
	/// Resets the tokenizer back to the first token.
	/// </summary>
	public void Reset()
	{
		this.start = 0;
		this.end = 0;
	}

	/// <summary>
	/// (Re)sets the text to be tokenized, and resets the iterator back to the the start.
	/// </summary>
	public void SetText(ReadOnlySpan<TSpan> input)
	{
		Reset();
		this.input = input;
	}
}
