This package tokenizes (splits) words, sentences and graphemes, based on [Unicode text segmentation](https://unicode.org/reports/tr29/) (UAX #29), for Unicode version 15.0.0.

### Why tokenize?

Any time our code operates on individual words, we are tokenizing. Often, we do it ad hoc, such as splitting on spaces, which gives inconsistent results. The Unicode standard is better: it is multi-lingual, and handles punctuation, special characters, etc.

### Example

```csharp
using uax29;
using System.Text;

// The API is bytes in and bytes out

var example = "Here is some example text. 你好，世界.";
var bytes = Encoding.UTF8.GetBytes(example);

var tokens = new Tokenizer(bytes);
while (tokens.MoveNext())
{
	var s = Encoding.UTF8.GetString(tokens.Current);
	Console.WriteLine(s);
}

/*
Here
 
is
 
some
 
example
 
text
.
 
你
好
，
世
界
.
*/
```

### Conformance

We use the official [test suites](https://unicode.org/reports/tr41/tr41-26.html#Tests29). Status:

[![.NET](https://github.com/clipperhouse/uax29.net/actions/workflows/dotnet.yml/badge.svg)](https://github.com/clipperhouse/uax29.net/actions/workflows/dotnet.yml)

### Prior art

[clipperhouse/uax29](https://github.com/clipperhouse/uax29)

I previously implemented this for Go. This .Net version is something of a port of that.

### Other language implementations

[JavaScript](https://github.com/tc39/proposal-intl-segmenter)

[Rust](https://unicode-rs.github.io/unicode-segmentation/unicode_segmentation/trait.UnicodeSegmentation.html)

[Java](https://lucene.apache.org/core/3_5_0/api/core/org/apache/lucene/analysis/standard/StandardTokenizerImpl.html)

[Python](https://uniseg-python.readthedocs.io/en/latest/)
