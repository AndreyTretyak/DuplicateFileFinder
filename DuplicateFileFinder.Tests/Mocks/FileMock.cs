using System.IO;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;

namespace DuplicateFileFinder.Tests.Mocks
{
    internal class FileMock : IComparableFile
    {
        private readonly ulong _size;
        private readonly Stream _stream;

        public FileMock(ulong size = 0, Stream stream = null, string name = null)
        {
            _size = size;
            _stream = stream;
            FileName = name;
        }

        public string FileName { get; }

        public async Task<ulong> GetFileSizeAsync() => _size;

        public async Task<Stream> GetFileStreamAsync() => _stream;
    }
}