using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace DuplicateFileFinder.Core
{
    internal class UniversalComparableFile : IComparableFile
    {
        private readonly IStorageFile _file;

        public UniversalComparableFile(IStorageFile file)
        {
            _file = file;
        }

        public async Task<ulong> GetFileSizeAsync()
        {
            var propertyes = await _file.GetBasicPropertiesAsync();
            return propertyes.Size;
        }

        public async Task<Stream> GetFileStreamAsync()
        {
            return await _file.OpenStreamForReadAsync();
        }
    }
}
