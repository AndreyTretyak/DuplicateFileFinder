using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Comparators;
using DuplicateFileFinder.Tests.Mocks;
using NUnit.Framework;

namespace DuplicateFileFinder.Tests.Comparators
{
    [TestFixture]
    public class SizeComparatorTests : BaseComparatorTests
    {
        public IFileComparator Comparator { get; }

        public SizeComparatorTests()
        {
           Comparator =  new SizeComparator();
        }

        [Test]
        [TestCase(0ul, 0ul)]
        [TestCase(0ul, 1ul)]
        [TestCase(9ul, 0ul)]
        [TestCase(912312312ul, 912312312ul)]
        [TestCase(191ul, 17ul)]
        [TestCase(1ul, 1ul)]
        [TestCase(12281ul, 12280ul)]
        [TestCase(ulong.MaxValue, ulong.MaxValue)]
        [TestCase(ulong.MinValue, ulong.MaxValue)]
        [TestCase(ulong.MinValue, ulong.MinValue)]
        public async Task TestComparator(ulong first, ulong second)
        {
            var result = await Test(Comparator, new FileMock(first), new FileMock(second));
            Assert.AreEqual(first == second, result.Item1, "Result check");
            Assert.AreEqual(first, result.Item2.GetValue<ulong>(), "First value check");
            Assert.AreEqual(second, result.Item3.GetValue<ulong>(), "Second value check");
        }
    }
}
