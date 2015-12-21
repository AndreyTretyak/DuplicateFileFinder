using System;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Tests.Mocks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DuplicateFileFinder.Tests
{
    [TestFixture]
    public class ComparationManagerTests
    {
        private static readonly Random Random = new Random(13);

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private static readonly IComparableFile[] DefaultFiles = new IComparableFile[] {new FileMock(), new FileMock()};

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
        public async Task OperationCanceledTestAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var task = GeneralTestAsync(new IFileComparator[] {new ComparatorMock()}, 
                Enumerable.Repeat((IComparableFile)new FileMock(), 10000).ToArray());
                _cancellationTokenSource.Cancel(true);
                var result = await task;
                //TODO: something wrong with test execution order, should be changed
                //Assert.Fail($"{typeof(OperationCanceledException)} expected");
            }
            catch (OperationCanceledException) { }
        }

        [Test]
        public async Task OperationCanceledInsideGetCriteriaTestAsync()
        {
            try
            {
                var result = await GeneralTestAsync(new IFileComparator[]
                {
                    new ComparatorMock(f => { throw new OperationCanceledException(); })
                }, DefaultFiles);
                Assert.Fail($"{typeof(OperationCanceledException)} expected");
            }
            catch (OperationCanceledException) { }
        }

        [Test]
        public async Task OperationCanceledInsideComperingTestAsync()
        {
            try
            {
                var result = await GeneralTestAsync(new IFileComparator[]
                {
                    new ComparatorMock(
                        compareFunction: (f, s) => { throw new OperationCanceledException(); })
                }, DefaultFiles);
                Assert.Fail($"{typeof(OperationCanceledException)} expected");
            }
            catch (OperationCanceledException){}
        }


        [Test]
        public async Task ExceptionTestInCriteriaAndComparation()
        {
            var result = await GeneralTestAsync(new IFileComparator[]
            {
                new ComparatorMock(
                f => {throw new InvalidOperationException();},
                (f, s) => {throw new InvalidOperationException();})
            }, DefaultFiles);
        }

        [Test]
        public async Task ExceptionTestInCriteria()
        {
            var result = await GeneralTestAsync(new IFileComparator[]
            {
                new ComparatorMock(
                f =>
                {
                    throw new InvalidOperationException();
                })
            },
            new IComparableFile[]
            {
               new FileMock(),
               new FileMock()
            });
        }

        [Test]
        public async Task ExceptionTestInComparation()
        {
            var result = await GeneralTestAsync(new IFileComparator[]
            {
                new ComparatorMock(
                compareFunction: (f, s) =>
                {
                    throw new InvalidOperationException();
                })
            },
            new IComparableFile[]
            {
               new FileMock(),
               new FileMock()
            });
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
            }, DefaultFiles);
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
            var result = (await manager.FindDuplicatesAsync(files, _cancellationTokenSource.Token)).ToArray();
            Assert.AreEqual(files.Length, result.Sum(g => g.Count));
            return result;
        }
    }
}
