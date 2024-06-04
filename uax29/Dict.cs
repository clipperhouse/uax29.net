namespace uax29;

using System.Collections.Frozen;

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
	/// Looks up the Unicode categories for a given rune (codepoint).
	/// </summary>
	/// <param name="rune">The rune to look up.</param>
	/// <returns>The categories (Property) of the rune if found, otherwise 0.</returns>
	public Property Lookup(int rune)
	{
		if (lookups.TryGetValue(rune, out Property property))
		{
			return property;
		}
		return 0;
	}
}
