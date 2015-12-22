namespace DuplicateFileFinder.Core.ProgressChanges
{
    public struct FilesProcessedByComparator : IProgressChanged
    {
        private readonly int _comparatorIndex;
        private readonly int _filesCount;

        public FilesProcessedByComparator(int comparatorIndex, int filesCount)
        {
            _comparatorIndex = comparatorIndex;
            _filesCount = filesCount;
        }

        public void ApplyChanges(ProgressInformation progressInformation)
        {
            progressInformation.FilesProcessed += _filesCount;
        }
    }
}