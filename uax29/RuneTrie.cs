namespace uax29;

using System.Text;
using ProtoBuf;

[ProtoContract]
public partial class Node
{
	[ProtoMember(1)]
	public int Value { get; set; }

	[ProtoMember(2)]
	public bool Terminal { get; set; }

	[ProtoMember(3)]
	public Dictionary<byte, Node> Children { get; set; }

	public Node()
	{
		Terminal = false;
		Value = -1; // Assuming -1 means no payload assigned
		Children = new(); // 256 for each possible byte value
	}
}

/// <summary>
/// A trie for looking up integer values associated with UTF-8 runes/codepoints.
/// Similar use cases as a Dictionary<rune/char/int, int> 
/// </summary>
[ProtoContract]
public class RuneTrie
{
	[ProtoMember(1)]
	public Node Root { get; set; }

	public RuneTrie()
	{
		Root = new Node();
	}

	// Inserts a rune into the trie and reports success
	public bool Insert(Rune rune, int payload)
	{
		Span<byte> bytes = stackalloc byte[4];
		if (rune.TryEncodeToUtf8(bytes, out int width))
		{
			Insert(bytes[0..width], payload);
		}
		return false;
	}

	// Inserts a char into the trie and reports success
	public bool Insert(char rune, int payload)
	{
		var bytes = Encoding.UTF8.GetBytes([rune]);
		return Insert(bytes, payload);
	}

	/// <summary>
	/// Inserts UTF-8 bytes into the trie with an associated payload value.
	/// </summary>
	/// <param name="rune">UTF-8 bytes representing a single codepoint. Note: does not check the validity of the bytes; garbage-in, garbage-out.</param>
	/// <param name="payload">An arbitrary value you can associate with the rune.</param>
	/// <returns>Success of the insert</returns>
	public bool Insert(Span<byte> rune, int payload)
	{
		var node = Root;
		foreach (var b in rune)
		{
			var children = node.Children;
			if (!children.TryGetValue(b, out node))
			{
				node = new Node();
				children.Add(b, node);
			}
		}
		node.Terminal = true;
		node.Value = payload;

		return true;
	}

	// Returns if the byte span is in the trie, what the payload is, and how many bytes of rune were consumed.
	// It does not validate the span, garbage-in, garbage-out.
	public (bool found, int payload) Search(Span<byte> rune)
	{
		var length = 0;
		var node = Root;

		foreach (var b in rune)
		{
			length++;
			if (!node.Children.TryGetValue(b, out node))
			{
				return (false, -1);
			}
		}

		var payload = node.Terminal ? node.Value : -1;
		return (node.Terminal, payload);
	}

	public (bool found, int payload) Search(Rune rune)
	{
		Span<byte> bytes = stackalloc byte[4];
		if (rune.TryEncodeToUtf8(bytes, out int width))
		{
			Search(bytes[0..width]);
		}
		return (false, -1);
	}

	public (bool found, int payload) Search(char rune)
	{
		var bytes = Encoding.UTF8.GetBytes([rune]);
		return Search(bytes);
	}
}
