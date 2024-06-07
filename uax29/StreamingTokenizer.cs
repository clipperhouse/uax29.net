namespace uax29;

internal delegate int Read(byte[] buffer, int offset, int count);

public static class StreamTokenizer
{

	/// <summary>
	/// Create a tokenizer for a stream of UTF-8 encoded bytes, to split words, graphemes or sentences.
	/// </summary>
	/// <param name="stream">The stream of UTF-8 bytes to tokenize.</param>
	/// <param name="tokenType">Optional, choose to tokenize words, graphemes or sentences. Default is words.</param>
	/// <param name="maxTokenBytes">
	/// Optional, the maximum token size in bytes. Tokens that exceed this size will simply be cut off at this length, no error will occur.
	/// Defaults to 1024 bytes. The tokenizer is intended for natural language, so we don't expect you'll find text with a token beyond a couple of dozen bytes.
	/// If this cutoff is too small for your data, increase it. If you'd like to save memory, reduce it.
	/// </param>
	/// <returns>
	/// A tokenizer to iterate over, using <see cref="StreamTokenizerImpl{TSpan}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{TSpan}.Current"/>.
	/// <see cref="StreamTokenizerImpl{TSpan}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </returns>
	public static StreamTokenizerImpl Create(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		return new StreamTokenizerImpl(stream, tokenType, maxTokenBytes);
	}
}

/// <summary>
/// Tokenizer splits a stream of UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// Use <see cref="StreamTokenizerImpl{TSpan}.MoveNext"/> to iterate, and <see cref="StreamTokenizerImpl{TSpan}.Current"/> to retrive the current token (i.e. the word, grapheme or sentence).
/// </summary>
public ref struct StreamTokenizerImpl
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
	internal StreamTokenizerImpl(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		tok = new Tokenizer<byte>(null, tokenType);
		buffer = new Buffer(stream.Read, maxTokenBytes);
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
		this.buffer.SetRead(stream.Read);
	}
}

internal ref struct Buffer
{
	readonly byte[] storage;
	int end = 0;
	Read Read;

	internal Buffer(Read read, int size)
	{
		this.Read = read;
		storage = new byte[size];
	}

	internal ReadOnlySpan<byte> Contents
	{
		get
		{
			var read = Read(storage, end, storage.Length - end);
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

	internal void SetRead(Read read)
	{
		this.Read = read;
		end = 0;
	}
}