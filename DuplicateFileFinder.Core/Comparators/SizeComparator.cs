using System;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core.ProgressChanges;

namespace DuplicateFileFinder.Core.Comparators
{
    internal class SizeComparator : IFileComparator
    {
        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            progress?.Report(new CurrentActionChanged(Resources.CalcualtingSize, file));
            return new ComparationCriteria(await file.GetFileSizeAsync());
        }

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            return left.GetValue<ulong>() == right.GetValue<ulong>();
        }
    }
}
