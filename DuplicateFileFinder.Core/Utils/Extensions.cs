using System.IO;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.Utils
{
    internal static class Extensions
    {
        public static async Task<BufferedStream> GetBufferedStreamAsync(this IComparableFile file, int bufferSize)
        {
            return new BufferedStream(await file.GetFileStreamAsync(), bufferSize);
        }
    }
}
