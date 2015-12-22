using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.Providers
{
    public class LocalFileProvider : IFileProvider
    {
        public async Task<IComparableFile> GetFileAsync(string path)
        {
            return new LocalComparableFile(new FileInfo(path));
        }

        public async Task<IDirectory> GetDirectoryAsync(string path)
        {
            return new LocalDirectory(new DirectoryInfo(path));
        }
    }
}