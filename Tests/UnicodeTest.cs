namespace Tests;

public class UnicodeTest(byte[] input, byte[][] expected, string comment)
{
	public readonly byte[] input = input;
	public readonly byte[][] expected = expected;
	public readonly string comment = comment;
}
