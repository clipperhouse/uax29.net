using System.Text;
using UAX29;

namespace Tests;


[TestFixture]
public class TestExample
{

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Readme()
    {
        var example = "Hello, 🌏 world. 你好，世界.";

        // The tokenizer can split words, graphemes or sentences.
        // It operates on strings, UTF-8 bytes, and streams.

        var words = Tokenizer.GetWords(example);

        // Iterate over the tokens
        foreach (var word in words)
        {
            // word is ReadOnlySpan<char>
            // If you need it back as a string:
            Console.WriteLine(word.ToString());
        }

        /*
        Hello
        ,
        
        🌏
        
        world
        .
        
        你
        好
        ，
        世
        界
        .
        */

        var utf8bytes = Encoding.UTF8.GetBytes(example);
        var graphemes = Tokenizer.GetGraphemes(utf8bytes);

        // Iterate over the tokens		
        foreach (var grapheme in graphemes)
        {
            // grapheme is a ReadOnlySpan<byte> of UTF-8 bytes
            // If you need it back as a string:
            var s = Encoding.UTF8.GetString(grapheme);
            Console.WriteLine(s);
        }

        /*
        H
        e
        l
        l
        o
        ,
        
        🌏
        
        w
        o
        r
        l
        d
        .
        
        你
        好
        ，
        世
        界
        .
        */
    }
}
