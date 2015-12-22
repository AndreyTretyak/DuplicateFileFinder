using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.Providers
{
    public class LocalDirectory : IDirectory
    {
        private readonly DirectoryInfo _directory;

        public LocalDirectory(DirectoryInfo directory)
        {
            _directory = directory;
        }

        public string Name => _directory.Name;

        public async Task<IList<IDirectory>> GetDirectoriesAsync()
        {
            return _directory.GetDirectories().Select(d => (IDirectory)new LocalDirectory(d)).ToList();
        }

        public async Task<IList<IComparableFile>> GetFilesAsync()
        {
            return _directory.GetFiles().Select(f => (IComparableFile)new LocalComparableFile(f)).ToList();
        }
    }
}