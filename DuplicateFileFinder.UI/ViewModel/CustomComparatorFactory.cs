using System.Collections.Generic;
using System.Linq;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Comparators;

namespace DuplicateFileFinder.UI.ViewModel
{
    public class CustomComparatorFactory : IComparatorsFactory
    {
        private readonly IReadOnlyList<IFileComparator> _comparators;

        public CustomComparatorFactory(IEnumerable<IFileComparator> comparators)
        {
            _comparators = comparators.ToList().AsReadOnly();
        }


        public IReadOnlyList<IFileComparator> GetComparators()
        {
            return _comparators;
        }
    }
}