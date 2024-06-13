namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal interface IDecoder<TSpan>
{
	static abstract Property Ignore { get; }
	static abstract Dict Dict { get; }

	static abstract OperationStatus DecodeLastRune(ReadOnlySpan<TSpan> input, out Rune result, out int consumed);
	static abstract OperationStatus DecodeFirstRune(ReadOnlySpan<TSpan> input, out Rune result, out int consumed);
}

internal interface IDictAndIgnore
{
	static abstract Dict Dict { get; }
	static abstract Property Ignore { get; }
}

internal readonly struct Utf8Decoder<TDictAndIgnore> : IDecoder<byte>
	where TDictAndIgnore : struct, IDictAndIgnore  // for non-ref for devirtualization purposes
{
	static Property IDecoder<byte>.Ignore
	=> TDictAndIgnore.Ignore;

	static Dict IDecoder<byte>.Dict
	=> TDictAndIgnore.Dict;

	static OperationStatus IDecoder<byte>.DecodeFirstRune(ReadOnlySpan<byte> input, out Rune result, out int consumed)
	=> Rune.DecodeFromUtf8(input, out result, out consumed);

	static OperationStatus IDecoder<byte>.DecodeLastRune(ReadOnlySpan<byte> input, out Rune result, out int consumed)
	=> Rune.DecodeLastFromUtf8(input, out result, out consumed);
}

internal readonly struct Utf16Decoder<TDictAndIgnore> : IDecoder<char>
	where TDictAndIgnore : struct, IDictAndIgnore  // for non-ref for devirtualization purposes
{
	static Property IDecoder<char>.Ignore
	=> TDictAndIgnore.Ignore;

	static Dict IDecoder<char>.Dict
	=> TDictAndIgnore.Dict;

	static OperationStatus IDecoder<char>.DecodeFirstRune(ReadOnlySpan<char> input, out Rune result, out int consumed)
	=> Rune.DecodeFromUtf16(input, out result, out consumed);

	static OperationStatus IDecoder<char>.DecodeLastRune(ReadOnlySpan<char> input, out Rune result, out int consumed)
	=> Rune.DecodeLastFromUtf16(input, out result, out consumed);
}
