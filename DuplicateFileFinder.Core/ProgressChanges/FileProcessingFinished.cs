namespace DuplicateFileFinder.Core.ProgressChanges
{
    public struct FileProcessingFinished : IProgressChanged
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
}