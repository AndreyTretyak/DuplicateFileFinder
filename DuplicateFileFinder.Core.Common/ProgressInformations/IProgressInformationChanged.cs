using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.Versioning;
using System.Text;

namespace DuplicateFileFinder.Core.Common.ProgressInformations
{
    public interface IProgressInformationChanged
    {
        void ApplyChanges(ProgressInformation progressInformation);
    }

    public struct FilesAndComparatorsCountInfo : IProgressInformationChanged
    {
        private readonly int _filesCount;

        private readonly int _comparatorsCount;

        public FilesAndComparatorsCountInfo(int filesCount, int comparatorsCount)
        {
            _filesCount = filesCount;
            _comparatorsCount = comparatorsCount;
        }

        public void ApplyChanges(ProgressInformation progressInformation)
        {
            progressInformation.ComparatorsCount = _comparatorsCount;
            progressInformation.FilesCount = _filesCount;
        }
    }

    public struct FileAnalizingFinished
    {

    }

    public struct FileProcessedByComparator : IProgressInformationChanged
    {
        public void ApplyChanges(ProgressInformation progressInformation)
        {
            progressInformation.FilesProcessed++;
        }
    }

    public struct FileProcessingFinished : IProgressInformationChanged
    {
        private readonly int _comparatorIndex;

        private readonly int _filesCount;

        public FileProcessingFinished(int comparatorIndex, int filesCount)
        {
            _comparatorIndex = comparatorIndex;
            _filesCount = filesCount;
        }

        public void ApplyChanges(ProgressInformation progressInformation)
        {
            var comparatorsLeft = progressInformation.ComparatorsCount - _comparatorIndex;
            progressInformation.FilesProcessed += comparatorsLeft * _filesCount;
        }
    }

    public struct CurrentActionChanged : IProgressInformationChanged
    {
        private readonly string _actonName;

        private readonly IComparableFile _firstFile;

        private readonly IComparableFile _secondFile;

        public CurrentActionChanged(string actionName, IComparableFile firstFile, IComparableFile secondFile = null)
        {
            _actonName = actionName;
            _firstFile = firstFile;
            _secondFile = secondFile;
        }
        
        public void ApplyChanges(ProgressInformation progressInformation)
        {

            if (_secondFile == null)
                progressInformation.CurrentAction = string.Format(Resources, _actonName, _firstFile.FileName);
            else
                progressInformation.CurrentAction = string.Format("", _actonName, _firstFile.FileName, _secondFile.FileName);
        }
    }

    public class ProgressInformation
    {
        public int FilesCount { get; set; }

        public int ComparatorsCount { get; set; }

        public int FilesProcessed { get; set; }

        public string CurrentAction { get; set; }

        public double Percentege => FilesProcessed / (double) (FilesCount * ComparatorsCount);
    }
}
