using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.Comparators
{
    public interface IComparatorsFactory
    {
        IReadOnlyList<IFileComparator> GetComparators();
    }

    public class DefaultComparatorsFactory : IComparatorsFactory
    {
        public IReadOnlyList<IFileComparator> GetComparators()
        {
            return new List<IFileComparator>
            {
                new SizeComparator(),
                new HashComparator(),
                new ExplicitComparator()
            }.AsReadOnly();
        }
    }
}
