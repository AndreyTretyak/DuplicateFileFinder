using System;
using System.Threading;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public interface IFileComparator
    {
        Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressChanged> progress);

        Task<bool> CompareAsync(ComparationCriteria left,  ComparationCriteria right, CancellationToken cancellationToken, IProgress<IProgressChanged> progress);
    }
}