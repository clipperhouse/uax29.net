This package tokenizes (splits) words, sentences and graphemes, based on [Unicode text segmentation](https://unicode.org/reports/tr29/) (UAX #29), for Unicode version 15.0.0.

### Why tokenize?

Any time our code operates on individual words, we are tokenizing. Often, we do it ad hoc, such as splitting on spaces, which gives inconsistent results. The Unicode standard is better: it is multi-lingual, and handles punctuation, special characters, etc.

### Example

```
dotnet add package uax29.net
```

```csharp
using uax29;
using System.Text;


var example = "Hello, 🌏 world. 你好，世界.";

// The tokenizer can split words, graphemes or sentences.
// It operates on strings, UTF-8 bytes, and streams.

var words = example.GetWords();

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
var graphemes = utf8bytes.GetGraphemes();

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

### Data types

For UTF-8 bytes, you pass `byte[]`, `Span<byte>` or `Stream`. For strings/chars, pass `string`, `char[]`, `Span<char>` or `TextReader`/`StreamReader`.

### Conformance

We use the official [test suites](https://unicode.org/reports/tr41/tr41-26.html#Tests29). Status:

[![.NET](https://github.com/clipperhouse/uax29.net/actions/workflows/dotnet.yml/badge.svg)](https://github.com/clipperhouse/uax29.net/actions/workflows/dotnet.yml)

### Performance

When tokenizing words, I get around 100MB/s on my Macbook M2. For typical text, that's around 25MM tokens/s. [Benchmarks](https://github.com/clipperhouse/uax29.net/tree/main/Benchmarks)

The tokenizer is implemented as a `ref struct`, so you should see zero allocations for static text such as `byte[]` or `string`/`char`.

For `Stream` or `TextReader`/`StreamReader`, a default `byte[]` or `char[]` buffer needs to be allocated behind the scenes. You can specify the size when calling `Create`. You can re-use the buffer by calling `SetStream` on an existing tokenizer, which will avoid re-allocation.   

### Invalid inputs

The tokenizer expects valid (decodable) UTF-8 bytes or UTF-16 chars as input. We [make an effort](https://github.com/clipperhouse/uax29.net/blob/main/Tests/Unicode.cs#L42-L67) to ensure that all bytes will be returned even if invalid, i.e. to be lossless in any case, though the resulting tokenization may not be useful. Garbage in, garbage out.

### Prior art

[clipperhouse/uax29](https://github.com/clipperhouse/uax29)

I previously implemented this for Go.

[StringInfo.GetTextElementEnumerator](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.stringinfo.gettextelementenumerator?view=net-8.0)

The .Net Core standard library has a similar enumerator for graphemes.

### Other language implementations

[JavaScript](https://github.com/tc39/proposal-intl-segmenter)

[Rust](https://unicode-rs.github.io/unicode-segmentation/unicode_segmentation/trait.UnicodeSegmentation.html)

[Java](https://lucene.apache.org/core/3_5_0/api/core/org/apache/lucene/analysis/standard/StandardTokenizerImpl.html)

[Python](https://uniseg-python.readthedocs.io/en/latest/)
