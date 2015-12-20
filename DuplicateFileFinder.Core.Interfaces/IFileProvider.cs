using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public interface IFileProvider
    {
        Task<IReadOnlyList<IComparableFile>> GetDirectoryFilesAsync(string path);

        Task<IComparableFile> GetFileAsync(string path);
    }
}
