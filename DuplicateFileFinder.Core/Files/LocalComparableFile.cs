using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.Files
{
    internal class LocalComparableFile : IComparableFile
    {
        private readonly FileInfo _fileInfo;

        public LocalComparableFile(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public string FileName => _fileInfo.FullName;

        public async Task<ulong> GetFileSizeAsync() => (ulong)_fileInfo.Length;

        public async Task<Stream> GetFileStreamAsync() => _fileInfo.OpenRead();
    }
}
