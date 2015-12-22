using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core.ProgressChanges;
using DuplicateFileFinder.Core.Utils;

namespace DuplicateFileFinder.Core.Comparators
{
    internal class ExplicitComparator : IFileComparator
    {
        private readonly int _bufferCapacity;

        public ExplicitComparator(int bufferCapacity = ComparationSettings.DefaultBufferSize)
        {
            _bufferCapacity = bufferCapacity;
        }

        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            return new ComparationCriteria(file);
        }

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            var leftFile = left.GetValue<IComparableFile>();
            var rightFile = right.GetValue<IComparableFile>();
            progress.Report(new CurrentActionChanged(string.Format(Resources.ExplitlyComparingFiles, leftFile.FileName, rightFile.FileName), leftFile, rightFile));

            using (var leftStream = await leftFile.GetBufferedStreamAsync(_bufferCapacity))
            {
                using (var rightStream = await right.GetValue<IComparableFile>().GetBufferedStreamAsync(_bufferCapacity))
                {
                    return await FileStreamsEqualsAsync(leftStream, rightStream);
                }
            }
        }

        private async Task<bool> FileStreamsEqualsAsync(Stream leftStream, Stream rightStream)
        {
            var leftBuffer = new byte[_bufferCapacity];
            var rightBuffer = new byte[_bufferCapacity];

            int bytesReadLeft;
            int bytesReadRight;
            do
            {
                bytesReadLeft = await leftStream.ReadAsync(leftBuffer, 0, leftBuffer.Length);
                bytesReadRight = await rightStream.ReadAsync(rightBuffer, 0,  rightBuffer.Length);
                if (bytesReadLeft != bytesReadRight || !ByteArrayComparator.Compare(leftBuffer, rightBuffer))
                    return false;
            } while (bytesReadLeft > 0 && bytesReadRight > 0);
            return true;
        }
    }
}