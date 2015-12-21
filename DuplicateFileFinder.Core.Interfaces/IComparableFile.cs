using System.IO;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public interface IComparableFile //: IFileSystemItem
    {
        string FileName { get; }

        Task<ulong> GetFileSizeAsync();

        Task<Stream> GetFileStreamAsync();
    }

    public interface IDirectory : IFileSystemItem
    {
        string FileName { get; }

        Task<Stream> GetDirectoryContentAsync();
    }

    public interface IFileSystemItem
    {
        string Name { get; }
    }
}