using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using DuplicateFileFinder.CoreNew.Utils;

namespace DuplicateFileFinder.Core
{
    public class HashComparator : IFileComparator
    {
        private readonly HashAlgorithmProvider _hashAlgorithm;

        private readonly uint _bufferCapacity;

        public HashComparator(string hashAlgorithmName = null, uint bufferCapacity = ComparationSettings.DefaultBufferSize)
        {
            _hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(hashAlgorithmName ?? HashAlgorithmNames.Sha512);
            _bufferCapacity = bufferCapacity;
        }

        public async Task<ComparationCriteria> GetComparationCriteriaAsync(IComparableFile file)
        {
            using (var inputStream = await file.GetInputStreamAsync())
            {
                var buffer = new Windows.Storage.Streams.Buffer(_bufferCapacity);
                var hash = _hashAlgorithm.CreateHash();
                do
                {
                    await inputStream.ReadAsync(buffer, _bufferCapacity, InputStreamOptions.None);
                    hash.Append(buffer);
                } while (buffer.Length > 0);

                return new ComparationCriteria(hash.GetValueAndReset());
            }
        }

        public async Task<bool> CompareAsync(ComparationCriteria left, ComparationCriteria right)
        {
            return CryptographicBuffer.Compare(left.GetValue<IBuffer>(), right.GetValue<IBuffer>());
        }
    }
}