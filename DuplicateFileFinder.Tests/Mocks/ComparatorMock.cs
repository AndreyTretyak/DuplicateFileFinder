using System;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;

namespace DuplicateFileFinder.Tests.Mocks
{
    public class ComparatorMock : IFileComparator
    {
        private readonly Func<IComparableFile, ComparationCriteria> _getCriteriaFunction;
        private readonly Func<ComparationCriteria, ComparationCriteria, bool> _compareFunction;


        public ComparatorMock(Func<IComparableFile, ComparationCriteria> getCriteriaFunction = null, Func<ComparationCriteria, ComparationCriteria, bool> compareFunction = null)
        {
            _getCriteriaFunction = getCriteriaFunction ?? (f => new ComparationCriteria(f));
            _compareFunction = compareFunction ?? ((f, s) => true);
        }

        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressChanged> progress) => _getCriteriaFunction(file);

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right, CancellationToken cancellationToken, IProgress<IProgressChanged> progress) => _compareFunction(left, right);
    }
}