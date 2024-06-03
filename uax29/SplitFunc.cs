namespace uax29;

/// <summary>
/// A delegate for splitting UTF-8 byte strings.
/// </summary>
/// <param name="data">Input UTF-8 byte data to be processed.</param>
/// <param name="atEOF">
/// Whether the input data contains all expected data.
/// (This is always true in the current implementation. Future implementations may support streams.)
/// </param>
/// <returns>The number of bytes to advance, i.e. the length of the first token in the input UTF-8 data.</returns>
delegate int SplitFunc(Span<byte> data, bool atEOF = true);
