using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core
{
    public static class FileProviderExtensions
    {
        public static async Task<IList<IComparableFile>> GetDirectoryFilesAsync(this IFileProvider provider, string path)
        {
            var result = new List<IComparableFile>();
            var directory = await provider.GetDirectoryAsync(path);
            result.AddRange(await directory.GetFilesAsync());
            result.AddRange(await GetDirectoryFilesAsync(directory));
            return result;
        }


        private static async Task<IList<IComparableFile>> GetDirectoryFilesAsync(IDirectory directory)
        {
            var result = new List<IComparableFile>();
            result.AddRange(await directory.GetFilesAsync());
            foreach (var dir in await directory.GetDirectoriesAsync())
            {
                result.AddRange(await GetDirectoryFilesAsync(dir));
            }
            return result.AsReadOnly();
        }
    }
}