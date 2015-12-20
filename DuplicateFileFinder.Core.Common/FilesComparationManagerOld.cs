using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public class FilesComparationManagerOld
    {
        private readonly IList<IFileComparator> _comparators;

        public FilesComparationManagerOld(IList<IFileComparator> comparators)
        {
            _comparators = comparators;
        }

        public async Task<IEnumerable<FileGroup>> FindDuplicatesAsync(IEnumerable<IComparableFile> files)
        {
            return await FindDuplicatesRecursiveStepAsync(new FileGroup(files));
        }

        private async Task<IEnumerable<FileGroup>> FindDuplicatesRecursiveStepAsync(FileGroup filesGroup, int comparatorIndex = 0)
        {
            var result = new List<FileGroup>();
            if (comparatorIndex >= _comparators.Count() || !filesGroup.CanContainDuplicates)
            {
                result.Add(filesGroup);
            }
            else
            {
                var fileGroups = await FindDuplicatesUsingComparatorAsync(_comparators[comparatorIndex], filesGroup.Files);
                foreach (var fileGroup in fileGroups)
                {
                    result.AddRange(await FindDuplicatesRecursiveStepAsync(fileGroup, comparatorIndex + 1));
                }
            }
            return result;
        }

        private async Task<IEnumerable<FileGroup>> FindDuplicatesUsingComparatorAsync(IFileComparator comparator, IEnumerable<IComparableFile> files)
        {
            var fileGroups = new List<FileGroup>();
            var tasks = files.Select(f => GetCriteriaWithFileReferenceAsync(comparator,f)).ToArray();
            await Task.WhenAll(tasks);
            
            foreach (var criteria in tasks.Select(e => e.Result))
            {
                var isAdded = false;
                foreach (var fileGroup in fileGroups)
                {
                    if (!await comparator.CompareAsync(criteria.Value, fileGroup.Key))
                        continue;

                    fileGroup.Files.Add(criteria.File);
                    isAdded = true;
                    break;
                }
                if (!isAdded)
                {
                    fileGroups.Add(new FileGroup(criteria.Value, criteria.File));
                }
            }
            return fileGroups;
        }

        private static async Task<CriteriaWithFileReference> GetCriteriaWithFileReferenceAsync(IFileComparator comparator, IComparableFile file)
        {
            return new CriteriaWithFileReference(await comparator.GetComparationCriteriaAsync(file), file);
        }

        private struct CriteriaWithFileReference
        {
            public ComparationCriteria Value { get; }

            public IComparableFile File { get; }

            public CriteriaWithFileReference(ComparationCriteria value, IComparableFile file)
            {
                Value = value;
                File = file;
            }
        }
    }
}