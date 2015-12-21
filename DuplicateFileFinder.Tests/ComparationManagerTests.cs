using System;
using System.Linq;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Tests.Mocks;
using NUnit.Framework;

namespace DuplicateFileFinder.Tests
{
    [TestFixture]
    public class ComparationManagerTests
    {
        private static readonly Random Random = new Random(13);

        [Test]
        [TestCase(100, 1)]
        [TestCase(100, 5)]
        [TestCase(100, 25)]
        [TestCase(100, 100)]
        public async Task ComparatorManagerGroupingTest(int fileCount, int distinct)
        {
            var files = Enumerable.Range(0, fileCount)
                                  .Select(i => new FileMock((ulong)(i % distinct), name: i.ToString()))
                                  .Cast<IComparableFile>()
                                  .OrderBy(e => Random.Next())
                                  .ToArray();

            var comparators = new IFileComparator[]
            {
                new ComparatorMock(
                    f => new ComparationCriteria(f.GetFileSizeAsync().Result),
                    (f, s) => f.GetValue<ulong>() == s.GetValue<ulong>())
            };
            var result = await GeneralTestAsync(comparators, files);
            Assert.AreEqual(distinct, result.Length);
            foreach (var fileGroup in result)
            {
                var key = fileGroup.Key.GetValue<ulong>();
                foreach (var file in fileGroup)
                {
                    Assert.AreEqual(key, file.GetFileSizeAsync().Result, $"Wrong group for file file{file.FileName}");
                }
            }
        }


        [Test]
        public async Task ComparatorRedunnatCallTest()
        {
            var result = await GeneralTestAsync(new IFileComparator[]
            {
                new ComparatorMock(compareFunction: (f,s) => false),
                new ComparatorMock(compareFunction: (f, s) =>
                {
                    Assert.Fail("Redunant comparator call");
                    return false;
                })
            },
            new IComparableFile[]
            {
               new FileMock(),
               new FileMock()
            });
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        public async Task RedunnatCallForSingeFileTest(int filesCount)
        {
            var result = await GeneralTestAsync(new IFileComparator[]
            {
                new ComparatorMock(
                    f =>
                    {
                        Assert.Fail("Redunant criteria calculation");
                        return new ComparationCriteria(null);
                    },
                    (f, s) =>
                    {
                        Assert.Fail("Redunant comparator call");
                        return false;
                    })
            },
            Enumerable.Range(0, filesCount)
                      .Select(i => (IComparableFile) new FileMock())
                      .ToArray()
            );
        }

        public async Task<FileGroup[]> GeneralTestAsync(IFileComparator[] comparators, IComparableFile[] files)
        {
            var manager = new ComparationManager(new ComparationsFactoryMock(comparators));
            var result = (await manager.FindDuplicatesAsync(files)).ToArray();
            Assert.AreEqual(files.Length, result.Sum(g => g.Count));
            return result;
        }
    }
}
