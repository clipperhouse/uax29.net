namespace uax29;

using System.Buffers;
using System.Text;

/// A bitmap of Unicode categories
using Property = int;

internal class Dict : Dictionary<int, Property>
{
	public Property Lookup(Span<byte> data, out int width, out OperationStatus status)
	{
		status = Rune.DecodeFromUtf8(data, out Rune r, out width);
		if (status != OperationStatus.Done)
		{
			return 0;
		}
		if (TryGetValue(r.Value, out Property property))
		{
			return property;
		}
		return 0;
	}
}
