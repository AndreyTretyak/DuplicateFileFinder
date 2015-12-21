using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using DuplicateFileFinder.Core;

namespace DuplicateFileFinder.CoreNew.Utils
{
    internal static class Extensions
    {
        public static async Task<IInputStream> GetInputStreamAsync(this IComparableFile file)
        {
            var stream = await file.GetFileStreamAsync();
            return stream.AsInputStream();
        }
    }
}
