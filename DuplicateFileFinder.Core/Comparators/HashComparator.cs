using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DuplicateFileFinder.Core.ProgressChanges;
using DuplicateFileFinder.Core.Utils;

namespace DuplicateFileFinder.Core.Comparators
{
    internal class HashComparator : IFileComparator
    {
        private readonly HashAlgorithm _hashAlgorithm;

        private readonly int _bufferCapacity;

        private readonly string _actionName;

        public HashComparator(string hashAlgorithmName = ComparationSettings.DefaultHashingAlgorithm, int bufferCapacity = ComparationSettings.DefaultBufferSize)
        {
            _hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName);
            _actionName = string.Format(Resources.CalculatingHash, hashAlgorithmName);
            _bufferCapacity = bufferCapacity;
        }

        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            progress?.Report(new CurrentActionChanged(_actionName, file));
            using (var stream = await file.GetBufferedStreamAsync(_bufferCapacity))
            {
                return new ComparationCriteria(_hashAlgorithm.ComputeHash(stream));

                //var buffer = new byte[_bufferCapacity];
                //int bytesRead;
                //do
                //{
                //    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                //    if (bytesRead > 0)
                //    {
                //        _hashAlgorithm.TransformBlock(buffer, 0, buffer.Length, );
                //        HashCore(buffer, 0, bytesRead);
                //    }
                //} while (bytesRead > 0);
                //int offset = 0;
                //while (input.Length - offset >= size)
                //    offset += _hashAlgorithm.TransformBlock(input, offset, size, input, offset);
                //_hashAlgorithm.TransformFinalBlock(input, offset, input.Length - offset);
                //return new ComparationCriteria(_hashAlgorithm.Hash);
                
            }
        }

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right, CancellationToken cancellationToken, IProgress<IProgressChanged> progress)
        {
            return ByteArrayComparator.Compare(left.GetValue<byte[]>(), right.GetValue<byte[]>());
        }
    }
}