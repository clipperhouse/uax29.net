using System.Text;

namespace uax29;

/// <summary>
/// Tokenizer splits a stream of UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// Use <see cref="StreamingTokenizer{TSpan}.MoveNext"/> to iterate, and <see cref="StreamingTokenizer{TSpan}.Current"/> to retrive the current token (i.e. the word, grapheme or sentence).
/// </summary>
public ref struct StreamingTokenizer
{
	Tokenizer<byte> tok;

	Buffer buffer;

	/// <summary>
	/// Tokenizer splits strings (or UTF-8 bytes) as words, sentences or graphemes, per the Unicode UAX #29 spec.
	/// </summary>
	/// <param name="stream">A stream of UTF-8 encoded bytes.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Defaults is 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	internal StreamingTokenizer(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		tok = new Tokenizer<byte>(null, tokenType);
		buffer = new Buffer(stream, maxTokenBytes);
	}

	public bool MoveNext()
	{
		buffer.Consume(tok.Current.Length); // the previous token
		var input = buffer.Contents;
		tok.SetText(input);
		return tok.MoveNext();
	}

	public readonly ReadOnlySpan<byte> Current => tok.Current;

	public void SetStream(Stream stream)
	{
		this.tok.SetText(null);
		this.buffer.SetStream(stream);
	}
}

internal ref struct Buffer
{
	readonly byte[] storage;
	int end = 0;
	Stream stream;

	internal Buffer(Stream stream, int size)
	{
		this.stream = stream;
		storage = new byte[size];
	}

	internal ReadOnlySpan<byte> Contents
	{
		get
		{
			var read = stream.Read(storage, end, storage.Length - end);
			end += read;

			return storage.AsSpan(0, end);
		}
	}

	internal void Consume(int consumed)
	{
		// Move the remaining unconsumed data to the start of the buffer
		end -= consumed;
		Array.Copy(storage, consumed, storage, 0, end);
	}

	internal void SetStream(Stream stream)
	{
		this.stream = stream;
		end = 0;
	}
}