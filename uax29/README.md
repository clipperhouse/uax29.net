This package tokenizes (splits) words, sentences and graphemes, based on [Unicode text segmentation](https://unicode.org/reports/tr29/) (UAX #29), for Unicode version 15.0.0.

### Why tokenize?

Any time our code operates on individual words, we are tokenizing. Often, we do it ad hoc, such as splitting on spaces, which gives inconsistent results. The Unicode standard is better: it is multi-lingual, and handles punctuation, special characters, etc.

### Example

```
dotnet add package UAX29
```

```csharp
using UAX29;
using System.Text;

var example = "Hello, 🌏 world. 你好，世界.";

// The tokenizer can split words, graphemes or sentences.
// It operates on strings, UTF-8 bytes, and streams.

var words = Split.Words(example);

// Iterate over the tokens
foreach (var word in words)
{
    // word is ReadOnlySpan<char>
    // If you need it back as a string:
    Console.WriteLine(word.ToString());
}

/*
Hello
,

🌏

world
.

你
好
，
世
界
.
*/

var utf8bytes = Encoding.UTF8.GetBytes(example);
var graphemes = Split.Graphemes(utf8bytes);

// Iterate over the tokens
foreach (var grapheme in graphemes)
{
    // grapheme is a ReadOnlySpan<byte> of UTF-8 bytes
    // If you need it back as a string:
    var s = Encoding.UTF8.GetString(grapheme);
    Console.WriteLine(s);
}

/*
H
e
l
l
o
,

🌏

w
o
r
l
d
.

你
好
，
世
界
.
*/
```

There are also optional extension methods in the spirit of `string.Split`:

```csharp
using UAX29.Extensions;

example.SplitWords();
```

### Data types

For UTF-8 bytes, pass `byte[]`, `Span<byte>` or `Stream`; the resulting tokens will be `ReadOnlySpan<byte>`.

For strings/chars, pass `string`, `char[]`, `Span<char>` or `TextReader`/`StreamReader`; the resulting tokens will be `ReadOnlySpan<char>`.

If you have `Memory<byte|char>`, pass `Memory.Span`.

### Conformance

We use the official Unicode [test suites](https://unicode.org/reports/tr41/tr41-26.html#Tests29). Status:

[![.NET](https://github.com/clipperhouse/uax29.net/actions/workflows/dotnet.yml/badge.svg)](https://github.com/clipperhouse/uax29.net/actions/workflows/dotnet.yml)

This is the same spec that is implemented in Lucene's [StandardTokenizer](https://lucene.apache.org/core/6_5_0/core/org/apache/lucene/analysis/standard/StandardTokenizer.html).

### Performance

When tokenizing words, I get around 120MB/s on my Macbook M2. For typical text, that's around 30 million tokens/s. [Benchmarks](https://github.com/clipperhouse/uax29.net/tree/main/Benchmarks)

The tokenizer is implemented as a `ref struct`, so you should see zero allocations for static text such as `byte[]` or `string`/`char`.

Calling `Split.Words` returns a lazy enumerator, and will not allocate per-token. There are `ToList` and `ToArray` methods for convenience, which will allocate.

For `Stream` or `TextReader`/`StreamReader`, a buffer needs to be allocated behind the scenes. You can specify the size when calling `GetWords`. You can also optionally pass your own `byte[]` or `char[]` to do your own allocation, perhaps with [ArrayPool](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1). Or, you can re-use the buffer by calling `SetStream` on an existing tokenizer, which will avoid re-allocation.

### Options

Pass `Options.OmitWhitespace` if you would like whitespace-only tokens not to be returned (for words only).

### Invalid inputs

The tokenizer expects valid (decodable) UTF-8 bytes or UTF-16 chars as input. We [make an effort](https://github.com/clipperhouse/uax29.net/blob/main/uax29/Unicode.Test.cs#L55) to ensure that all bytes will be returned even if invalid, i.e. to be lossless in any case, though the resulting tokenization may not be useful. Garbage in, garbage out.

### Major version changes

#### v2 → v3

Renamed methods:

`Tokenizer.GetWords(input)` → `Split.Words(input)`

#### v1 → v2

Renamed package, namespace and methods:

`dotnet add package uax29.net` → `dotnet add package UAX29`

`using uax29` → `using UAX29`

`Tokenizer.Create(input)` → `Tokenizer.GetWords(input)`

`Tokenizer.Create(input, TokenType.Graphemes)` → `Tokenizer.GetGraphemes(input)`

### Prior art

[clipperhouse/uax29](https://github.com/clipperhouse/uax29)

I previously implemented this for Go.

[StringInfo.GetTextElementEnumerator](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.stringinfo.gettextelementenumerator?view=net-8.0)

The .Net Core standard library has a similar enumerator for graphemes.

### Other language implementations

[Java](https://lucene.apache.org/core/6_5_0/core/org/apache/lucene/analysis/standard/StandardTokenizer.html)

[JavaScript](https://github.com/tc39/proposal-intl-segmenter)

[Rust](https://unicode-rs.github.io/unicode-segmentation/unicode_segmentation/trait.UnicodeSegmentation.html)

[Python](https://uniseg-python.readthedocs.io/en/latest/)
