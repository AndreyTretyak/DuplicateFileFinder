using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuplicateFileFinder.Core;
using DuplicateFileFinder.Core.Comparators;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DuplicateFileFinder.Tests.Comparators
{
    [TestFixture]
    public class ExplicitComparatorTests : BaseComparatorTests
    {
        private static readonly Random Random = new Random();

        public IFileComparator Comparator { get; }

        public ExplicitComparatorTests()
        {
            Comparator = new ExplicitComparator();
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(7120000)]
        [TestCase(ComparationSettings.DefaultBufferSize)]
        [TestCase(ComparationSettings.DefaultBufferSize * 10)]
        public async Task ComparatorTest(int size)
        {
            var firstBuffer = new byte[size];
            var secondBuffer = new byte[size];

            Random.NextBytes(firstBuffer);
            firstBuffer.CopyTo(secondBuffer, 0);
            Assert.IsTrue((await Test(Comparator, firstBuffer, secondBuffer)).Item1);

            if (size == 0) return;

            secondBuffer[Random.Next(secondBuffer.Length)]++;
            Assert.IsFalse((await Test(Comparator, firstBuffer, secondBuffer)).Item1);
        }

        [Test]
        [TestCase(0, 40000000)]
        [TestCase(1, ComparationSettings.DefaultBufferSize)]
        [TestCase(7120000, 12)]
        [TestCase(ComparationSettings.DefaultBufferSize, ComparationSettings.DefaultBufferSize * 3)]
        [TestCase(ComparationSettings.DefaultBufferSize * 2, ComparationSettings.DefaultBufferSize * 4)]
        public async Task DifferentSize(int firstSize, int secondSize)
        {
            var firstBuffer = new byte[firstSize];
            var secondBuffer = new byte[secondSize];
            Assert.IsFalse((await Test(Comparator, firstBuffer, secondBuffer)).Item1);
        }
    }
}
