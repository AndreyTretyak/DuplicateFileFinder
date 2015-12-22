namespace DuplicateFileFinder.Core.ProgressChanges
{
    public struct FilesAndComparatorsCountInfo : IProgressChanged
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
}