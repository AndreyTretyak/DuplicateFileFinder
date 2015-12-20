using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public class SizeComparator : IFileComparator
    {
        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file)
        {
            return new ComparationCriteria(await file.GetFileSizeAsync());
        }

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right)
        {
            return left.GetValue<ulong>() == right.GetValue<ulong>();
        }
    }
}
