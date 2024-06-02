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
	public Property Lookup(Span<byte> data, out int width, out OperationStatus status)
	{
		status = Rune.DecodeFromUtf8(data, out Rune r, out width);
		if (status != OperationStatus.Done)
		{
			return 0;
		}
		if (lookups.TryGetValue(r.Value, out Property property))
		{
			return property;
		}
		return 0;
	}
}
