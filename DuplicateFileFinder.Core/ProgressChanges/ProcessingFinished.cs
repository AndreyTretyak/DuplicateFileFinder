using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFileFinder.Core.ProgressChanges
{
    public struct ProcessingFinished : IProgressChanged
    {
        private readonly int _duplicates;

        public ProcessingFinished(int duplicates)
        {
            _duplicates = duplicates;
        }

        public void ApplyChanges(ProgressInformation progressInformation)
        {
            progressInformation.FilesProcessed = progressInformation.FilesCount * progressInformation.ComparatorsCount;
            progressInformation.DuplicatesCount = _duplicates;
            progressInformation.CurrentAction = string.Format(Resources.Done, progressInformation.FilesCount, _duplicates);
        }
    }
}
