using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DuplicateFileFinder.Core
{
    public struct FileGroup : IReadOnlyCollection<IComparableFile>
    {
        internal ComparationCriteria Key { get; }

        private List<IComparableFile> _files { get; }

        internal FileGroup(ComparationCriteria key, IComparableFile file)
        {
            Key = key;
            _files = new List<IComparableFile> { file };
        }

        internal FileGroup(IEnumerable<IComparableFile> files) : this()
        {
            _files = files.ToList();
        }

        internal void AddFile(IComparableFile file)
        {
            _files.Add(file);
        }

        public bool CanContainDuplicates => _files.Count > 1;

        public IEnumerator<IComparableFile> GetEnumerator() => _files.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _files.Count;
    }
}