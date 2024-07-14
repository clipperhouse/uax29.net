using System.Text;
using UAX29;

namespace Tests;

/// A bitmap of Unicode categories
using Property = uint;

[TestFixture]
public class TestSplitter
{

    [SetUp]
    public void Setup()
    {
    }

    const Property Yes1 = 1;
    const Property No1 = 2;
    const Property Yes2 = 4;
    const Property No2 = 8;
    const Property Yes3 = 16;
    const Property Yeses = Yes1 | Yes2 | Yes3;

    [Test]
    public void TestIsExclusively()
    {
        {
            var seen = Yes1;
            Assert.That(seen.IsExclusively(Yeses), Is.True);
        }

        {
            var seen = Yes1 | Yes2;
            Assert.That(seen.IsExclusively(Yeses), Is.True);
        }

        {
            var seen = No1;
            Assert.That(seen.IsExclusively(Yeses), Is.False);
        }

        {
            var seen = No1 | No2;
            Assert.That(seen.IsExclusively(Yeses), Is.False);
        }

        {
            var seen = Yes1 | No1;
            Assert.That(seen.IsExclusively(Yeses), Is.False);
        }

        {
            var seen = Yes1 | Yes3 | No1;
            Assert.That(seen.IsExclusively(Yeses), Is.False);
        }

        {
            Property seen = 0;
            Assert.That(seen.IsExclusively(Yeses), Is.False);
        }
    }
}
