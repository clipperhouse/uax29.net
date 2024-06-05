﻿using System.Runtime.InteropServices;
using System.Text;
using uax29;

namespace Tests;


[TestFixture]
public class Examples
{

	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void Readme()
	{
		var example = "Here is some example text. 你好，世界.";

		// The tokenizer can take a string or ReadOnlySpan<char>
		var tokens = Tokenizer.Create(example);

		// Iterate over the tokens		
		while (tokens.MoveNext())
		{
			// tokens.Current is a ReadOnlySpan<char>
			// If you need it back as a string:
			Console.WriteLine(tokens.Current.ToString());
		}


		// The tokenizer can also take raw UTF-8 bytes
		var utf8bytes = Encoding.UTF8.GetBytes(example);
		var tokens2 = Tokenizer.Create(utf8bytes);

		// Iterate over the tokens		
		while (tokens2.MoveNext())
		{
			// tokens.Current is a ReadOnlySpan<byte> of UTF-8 bytes
			// If you need it back as a string:
			var s = Encoding.UTF8.GetString(tokens2.Current);
			Console.WriteLine(s);
		}
	}
}
