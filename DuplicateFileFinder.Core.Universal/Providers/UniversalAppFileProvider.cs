using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace DuplicateFileFinder.Core
{
    public class UniversalAppFileProvider : IFileProvider
    {
        private readonly IStorageFolder _folder;

        public UniversalAppFileProvider(IStorageFolder folder)
        {
            _folder = folder;
        }

        public async Task<IReadOnlyList<IComparableFile>> GetDirectoryFilesAsync(string path)
        {
            var currentFolder = await _folder.GetFolderAsync(path);
            return await GetDirectoryFilesAsync(currentFolder);
        }

        private async Task<IReadOnlyList<IComparableFile>> GetDirectoryFilesAsync(IStorageFolder dir)
        {
            var result = new List<IComparableFile>();

            var files = await dir.GetFilesAsync();
            result.AddRange(files.Select(f => new UniversalComparableFile(f)));

            foreach (var subFolder in await dir.GetFoldersAsync())
            {
                result.AddRange(await GetDirectoryFilesAsync(subFolder));
            }

            return result.AsReadOnly();
        }


        public async Task<IComparableFile> GetFileAsync(string path)
        {
            return new UniversalComparableFile(await _folder.GetFileAsync(path));
        }
    }
}