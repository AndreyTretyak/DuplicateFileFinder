using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public interface IFileProvider
    {
        Task<IComparableFile> GetFileAsync(string path);

        Task<IDirectory> GetDirectoryAsync(string path);
    }
}
