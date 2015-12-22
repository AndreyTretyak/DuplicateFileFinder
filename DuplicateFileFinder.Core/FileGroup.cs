using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DuplicateFileFinder.Core
{
    public struct FileGroup : IReadOnlyCollection<IComparableFile>
    {
        private List<IComparableFile> _files { get; }

        internal Exception Exception { get; }

        internal ComparationCriteria Key { get; }

        internal FileGroup(ComparationCriteria key, IComparableFile file) : this()
        {
            Key = key;
            _files = new List<IComparableFile> { file };
        }

        //stub for reading errors, may be in future should be moved in separate class
        internal FileGroup(Exception exception, IComparableFile file)
        {
            Exception = exception;
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

        public bool IsError => Exception != null;
    }
}