using System.Collections.Generic;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Comparators;

namespace DuplicateFileFinder.Tests.Mocks
{
    public class ComparationsFactoryMock : IComparatorsFactory
    {
        private readonly IReadOnlyList<IFileComparator> _comparator;

        public ComparationsFactoryMock(params IFileComparator[] comparator)
        {
            _comparator = comparator;
        }
        
        public IReadOnlyList<IFileComparator> GetComparators() => _comparator;
    }
}