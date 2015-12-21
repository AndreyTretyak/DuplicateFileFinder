namespace DuplicateFileFinder.Core
{
    public class ProgressInformation
    {
        public int FilesCount { get; set; }

        public int ComparatorsCount { get; set; }

        public int FilesProcessed { get; set; }

        public string CurrentAction { get; set; }

        public double Percentege => ((double)FilesProcessed) /  (FilesCount * ComparatorsCount);
    }
}