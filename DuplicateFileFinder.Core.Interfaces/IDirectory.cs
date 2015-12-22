using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public interface IDirectory 
    {
        string Name { get; }

        Task<IList<IDirectory>> GetDirectoriesAsync();

        Task<IList<IComparableFile>> GetFilesAsync();
    }
}