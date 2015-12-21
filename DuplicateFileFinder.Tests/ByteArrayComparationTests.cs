using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuplicateFileFinder.Core.Utils;
using NUnit.Framework.Internal;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace DuplicateFileFinder.Tests
{
    [TestFixture]
    public class ByteArrayComparationTests
    {
        private static readonly Random Random = new Random(19);

        [Test]
        [TestCase(0,   0, true)]
        [TestCase(10, 10, true)]
        [TestCase(0,  10, false)]
        [TestCase(100, 0, false)]
        public void EmptyArraysTests(int firstSize, int secondSize, bool result)
        {
            Assert.AreEqual(result, ByteArrayComparator.Compare(new byte[firstSize], new byte[secondSize]));
        }


        [Test]
        [TestCase(new byte[0], null)]
        [TestCase(null, new byte[0])]
        [TestCase(null, null)]
        public void NullTests(byte[] first, byte[] second)
        {
            Assert.Throws<ArgumentNullException>(() => ByteArrayComparator.Compare(first, second));
        }



        [Test]
        [TestCase(1)]
        [TestCase(12)]
        [TestCase(1289)]
        public void RandomContestTests(int size)
        {
            var first = new byte[size];
            var second = new byte[size];

            Random.NextBytes(first);
            first.CopyTo(second, 0);

            Assert.IsTrue(ByteArrayComparator.Compare(first, second));
            
            second[Random.Next(second.Length)]++;
            Assert.IsFalse(ByteArrayComparator.Compare(first, second));
        }
    }
}
