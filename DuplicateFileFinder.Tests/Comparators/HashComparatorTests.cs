using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Comparators;
using DuplicateFileFinder.Core.Utils;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DuplicateFileFinder.Tests.Comparators
{
    [TestFixture]
    public class HashComparatorTests : BaseComparatorTests
    {
        [Test]
        [TestCase(1236)]
        [TestCase(9851)]
        [TestCase(ComparationSettings.DefaultBufferSize)]
        [TestCase(ComparationSettings.DefaultBufferSize * 3)]
        public async Task ComparatorTest(int size)
        {
            await ComparatorTest(size, ComparationSettings.DefaultHashingAlgorithm);
        }

        [Test]
        [TestCase(6870, "MD5")]
        [TestCase(8401, "SHA256")]
        public async Task ComparatorTest(int size, string hashType)
        {
            await ComparatorTest(size, size, hashType);
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(1, 1)]
        [TestCase(9731, 9730)]
        public async Task ComparatorTest(int firstSize, int secondSize)
        {
            await ComparatorTest(firstSize, secondSize, ComparationSettings.DefaultHashingAlgorithm);
        }

        [Test]
        [TestCase(5840, 5849, "MD5")]
        [TestCase(5841, 5840, "SHA1")]
        public async Task ComparatorTest(int firstSize, int secondSize, string hashType)
        {
            if (hashType == null)
                hashType = ComparationSettings.DefaultHashingAlgorithm;

            var comparator = new HashComparator(hashType);
            var hashAlgorithm = HashAlgorithm.Create(hashType);

            var firstBuffer = new byte[firstSize];
            Random.NextBytes(firstBuffer);
            var secondBuffer = new byte[secondSize];
            Random.NextBytes(secondBuffer);

            var result = await Test(comparator, firstBuffer, secondBuffer);


            var firstHash = hashAlgorithm.ComputeHash(firstBuffer);
            var secondHash = hashAlgorithm.ComputeHash(secondBuffer);

            CollectionAssert.AreEqual(firstHash, result.Item2.GetValue<byte[]>(), "first hash");
            CollectionAssert.AreEqual(firstHash, result.Item2.GetValue<byte[]>(), "second hash");

            Assert.AreEqual(firstHash.SequenceEqual(secondHash), result.Item1);
        }
    }
}
