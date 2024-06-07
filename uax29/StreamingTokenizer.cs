namespace uax29;

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
	/// A tokenizer to iterate over, using <see cref="StreamTokenizer{TSpan}.MoveNext"/>, and retrieving each individual token with <see cref="Tokenizer{TSpan}.Current"/>.
	/// <see cref="StreamTokenizer{TSpan}.Current"/> will be <see cref="ReadOnlySpan"/> of <see cref="byte"/>.
	/// </returns>
	public static StreamTokenizer<byte> Create(Stream stream, TokenType tokenType = TokenType.Words, int maxTokenBytes = 1024)
	{
		var tok = Tokenizer.Create(ReadOnlySpan<byte>.Empty, tokenType);
		return new StreamTokenizer<byte>(stream, tok, maxTokenBytes);
	}
}

/// <summary>
/// Tokenizer splits a stream of UTF-8 bytes as words, sentences or graphemes, per the Unicode UAX #29 spec.
/// Use <see cref="StreamTokenizer{TSpan}.MoveNext"/> to iterate, and <see cref="StreamTokenizer{TSpan}.Current"/> to retrive the current token (i.e. the word, grapheme or sentence).
/// </summary>
public ref struct StreamTokenizer<T> where T : struct
{
	Tokenizer<T> tok;

	Buffer<T> buffer;

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
	internal StreamTokenizer(Stream stream, Tokenizer<T> tok, int maxTokenBytes = 1024)
	{
		this.tok = tok;
		buffer = new Buffer<byte>(stream.Read, maxTokenBytes) as Buffer<T> ?? throw new NotImplementedException();
	}

	readonly ReadOnlySpan<T> empty = [];

	public bool MoveNext()
	{
		buffer.Consume(tok.Current.Length); // the previous token
		var input = buffer.Contents;
		tok.SetText(input);
		return tok.MoveNext();
	}

	public readonly ReadOnlySpan<T> Current => tok.Current;

	public void SetStream(Stream stream)
	{
		this.tok.SetText(empty);
		buffer = new Buffer<byte>(stream.Read, buffer.storage.Length) as Buffer<T> ?? throw new NotImplementedException();
	}
}

internal delegate int Read<T>(T[] buffer, int offset, int count) where T : struct;

internal class Buffer<T> where T : struct
{
	readonly internal T[] storage;
	Read<T> Read;
	int end = 0;

	internal Buffer(Read<T> read, int size)
	{
		this.Read = read;
		storage = new T[size];
	}

	internal ReadOnlySpan<T> Contents
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

	internal void SetRead(Read<T> read)
	{
		this.Read = read;
		end = 0;
	}
}