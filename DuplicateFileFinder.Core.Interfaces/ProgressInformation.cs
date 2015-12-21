using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DuplicateFileFinder.Core
{
    public class ProgressInformation
    {
        public int FilesCount { get; set; }

        public int ComparatorsCount { get; set; }

        public int FilesProcessed { get; set; }

        public int DuplicatesCount { get; set; }

        public string CurrentAction { get; set; }

        public double Percentege => ((double)FilesProcessed) /  (FilesCount * ComparatorsCount);

        public ConcurrentDictionary<IComparableFile, List<Exception>> Exceptions = new ConcurrentDictionary<IComparableFile, List<Exception>>();
    }
}