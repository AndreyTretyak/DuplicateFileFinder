using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Tests.Mocks;

namespace DuplicateFileFinder.Tests.Comparators
{
    public class BaseComparatorTests
    {
        protected static readonly Random Random = new Random(17);
        
        private readonly IProgress<IProgressChanged> _progressTracker;

        public CancellationTokenSource CancellationTokenSource { get; }

        public ProgressInformation ProgressInformation { get; }

        public List<IProgressChanged> ProgressChanges { get; }

        public BaseComparatorTests()
        {
            CancellationTokenSource = new CancellationTokenSource();
            ProgressInformation = new ProgressInformation();
            ProgressChanges = new List<IProgressChanged>();
            _progressTracker = new Progress<IProgressChanged>(change =>
            {
                change.ApplyChanges(ProgressInformation);
                ProgressChanges.Add(change);
            });
        }

        public async Task<Tuple<bool, ComparationCriteria, ComparationCriteria>> Test(IFileComparator comparator, IComparableFile first, IComparableFile second)
        {
            var firstResult = await comparator.GetComparationCriteriaAsync(first, CancellationTokenSource.Token, _progressTracker);

            var secondResult =  await comparator.GetComparationCriteriaAsync(second, CancellationTokenSource.Token, _progressTracker);

            var result = await comparator.CompareAsync(firstResult, secondResult, CancellationTokenSource.Token, _progressTracker);

            return new Tuple<bool, ComparationCriteria, ComparationCriteria>(result, firstResult, secondResult);
        }

        public async Task<Tuple<bool, ComparationCriteria, ComparationCriteria>> Test(IFileComparator comparator, byte[] first, byte[] second)
        {
            using (var fMemoryStream = new MemoryStream(first))
            {
                using (var sMemoryStream = new MemoryStream(second))
                {
                    var firstFile = new FileMock(stream: fMemoryStream);
                    var secondFile = new FileMock(stream: sMemoryStream);
                    return await Test(comparator, firstFile, secondFile);
                }
            }
        }
    }
}