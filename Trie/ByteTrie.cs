namespace Trie;

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

[ProtoContract]
public partial class ByteTrie
{
	[ProtoMember(1)]
	public Node root { get; set; }

	public ByteTrie()
	{
		root = new Node();
	}

	// Inserts a byte array into the trie with an integer payload
	public void Insert(byte[] data, int payload)
	{
		var node = root;
		foreach (var b in data)
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
	}

	// Inserts a byte array into the trie with an integer payload
	public void Insert(string s, int payload)
	{
		var data = Encoding.UTF8.GetBytes(s);
		Insert(data, payload);
	}

	// Returns if the byte array is in the trie
	public (bool found, int payload, int length) Search(byte[] data)
	{
		var length = 0;
		var node = root;
		foreach (var b in data)
		{
			length++;
			if (!node.Children.TryGetValue(b, out node))
			{
				return (false, -1, length);
			}
		}

		var payload = node.Terminal ? node.Value : -1;
		return (node.Terminal, payload, length);
	}

	// Returns if the string is in the trie
	public (bool found, int payload, int length) Search(string s)
	{
		var data = Encoding.UTF8.GetBytes(s);
		return Search(data);
	}
}
