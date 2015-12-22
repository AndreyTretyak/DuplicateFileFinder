using System.IO;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public interface IComparableFile
    {
        string FileName { get; }

        Task<ulong> GetFileSizeAsync();

        Task<Stream> GetFileStreamAsync();
    }
}