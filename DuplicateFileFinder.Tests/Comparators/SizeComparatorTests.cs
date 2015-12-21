using System;
using DuplicateFileFinder.Core.Comparators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DuplicateFileFinder.Tests
{
    [TestClass]
    public class ComparatorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            new SizeComparator();
        }
    }
}
