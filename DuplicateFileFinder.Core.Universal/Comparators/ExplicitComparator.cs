using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using DuplicateFileFinder.CoreNew.Utils;

namespace DuplicateFileFinder.Core
{
    public class ExplicitComparator : IFileComparator
    {
        private readonly uint _bufferCapacity;

        public ExplicitComparator(uint bufferCapacity = ComparationSettings.DefaultBufferSize)
        {
            _bufferCapacity = bufferCapacity;
        }

        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file)
        {
            return new ComparationCriteria(file);
        }

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right)
        {
            using (var leftInputStream = await left.GetValue<IComparableFile>().GetInputStreamAsync())
            {
                using (var rightInputStream = await right.GetValue<IComparableFile>().GetInputStreamAsync())
                {
                    return await FileStreamsEqualsAsync(leftInputStream, rightInputStream);
                }
            }
        }

        private async Task<bool> FileStreamsEqualsAsync(IInputStream leftStream, IInputStream rightStream)
        {
            var leftBuffer = new Windows.Storage.Streams.Buffer(_bufferCapacity);
            var rightBuffer = new Windows.Storage.Streams.Buffer(_bufferCapacity);

            do
            {
                await leftStream.ReadAsync(leftBuffer, _bufferCapacity, InputStreamOptions.None);
                await rightStream.ReadAsync(rightBuffer, _bufferCapacity, InputStreamOptions.None);
                if (!leftBuffer.IsSameData(rightBuffer))
                    return false;
            } while (leftBuffer.Length > 0 && rightBuffer.Length > 0);
            return true;
        }
    }
}