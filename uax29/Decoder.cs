namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal interface IDecoder<TSpan>
{
	static abstract OperationStatus DecodeLastRune(ReadOnlySpan<TSpan> input, out Rune result, out int consumed);
	static abstract OperationStatus DecodeFirstRune(ReadOnlySpan<TSpan> input, out Rune result, out int consumed);
}

internal interface IDict
{
	static abstract Dict Dict { get; }
}

internal interface IIgnore
{
	static abstract Property Ignore { get; }
}

internal readonly struct Utf8Decoder : IDecoder<byte>
{
	static OperationStatus IDecoder<byte>.DecodeFirstRune(ReadOnlySpan<byte> input, out Rune result, out int consumed)
	=> Rune.DecodeFromUtf8(input, out result, out consumed);

	static OperationStatus IDecoder<byte>.DecodeLastRune(ReadOnlySpan<byte> input, out Rune result, out int consumed)
	=> Rune.DecodeLastFromUtf8(input, out result, out consumed);
}

internal readonly struct Utf16Decoder : IDecoder<char>
{
	static OperationStatus IDecoder<char>.DecodeFirstRune(ReadOnlySpan<char> input, out Rune result, out int consumed)
	=> Rune.DecodeFromUtf16(input, out result, out consumed);

	static OperationStatus IDecoder<char>.DecodeLastRune(ReadOnlySpan<char> input, out Rune result, out int consumed)
	=> Rune.DecodeLastFromUtf16(input, out result, out consumed);
}
