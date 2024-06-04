namespace uax29;

using System.Buffers;
using System.Collections.Frozen;
using System.Text;

/// A bitmap of Unicode categories
using Property = uint;

internal class Dict
{
	internal FrozenDictionary<int, Property> lookups;
	public Dict(Dictionary<int, Property> lookups)
	{
		this.lookups = lookups.ToFrozenDictionary();
	}

	/// <summary>
	/// Lookup decodes the first rune/codepoint in the UTF-8 bytes, and determines its Unicode categories.
	/// </summary>
	/// <param name="data">UTF-8 bytes</param>
	/// <param name="width">The number of bytes of the decoded rune/codepoint.</param>
	/// <param name="status">Whether the rune decoding was successful.</param>
	/// <returns></returns>
	public Property Lookup(ReadOnlySpan<byte> data, out int width, out OperationStatus status)
	{
		status = Rune.DecodeFromUtf8(data, out Rune r, out width);
		if (lookups.TryGetValue(r.Value, out Property property))
		{
			return property;
		}
		return 0;
	}
}
